(function(){



var promises = {};

function randomPositiveInt(){
	return Math.floor(Math.random()*Number.MAX_SAFE_INTEGER) + 1
}

var listeners = {};
var uninitialized = {};

var sharedTypes = {};
var sharedObjects = {};


function SharedType(){};
SharedType.prototype = {};

var me;
var xbridge = me = {
	plugins: {

	},
	local: function local(name, module){
		for(var k in module)
		{
			if(module.hasOwnProperty(k)){
				local[name + '.' + k] = module[k];
			};
		}
	},
	util: {

	},
	promises: {

	},

	exec: function(method)
	{
		var args = Array.prototype.slice.call(arguments, 1);
		//console.log('xbridge: exec: ' + method);
		if(method.indexOf('.') < 0)
			method = 'core.' + method;
		var fn = this.local[method];
		if(fn != null)
			return fn.apply(this, args);
		else
		{
			//console.log('xbridge.exec: ' + JSON.stringify(arguments));
			return this._remote.apply(this, arguments);
		}
	},

	stringify: function(obj)
	{
		if(obj != null && obj.__proto__ instanceof SharedType)
		{
			return "" + obj.id;
		}
		return JSON.stringify(obj, function(k, v){
			if(v != null && v.__proto__.id == "SharedType")
			{
				return v.id;
			}
			return v;
		});
	},

	_remote: function(method)
	{
		var args = Array.prototype.slice.call(arguments, 1);
		var pointer = {};
		var id = randomPositiveInt();
		while(id in promises)
			id = randomPositiveInt();
		promises[id] = pointer;
		var url = 'xbridge:' + id + ':' + method + '(' + xbridge.stringify(args).slice(1, -1) + ')';
		return new Promise(function(resolve, reject){
			pointer.resolve = resolve;
			pointer.reject = reject;
			xbridge._send(url);
		});
	},

	/*
	 * this is called by the server to return the result of some operation that the client invoked using xbridge.exec()
	 */
	callbackOnClient: function(id, result){
		if(id == null)
		{
			console.log("callback without caller. result: " + result);
		}else{
			var pointer = promises[id];
			if(pointer == null)
			{
				console.error("no promise given for id: " + id);
			}else{
				//console.log("resolving promise: " + id);
				delete promises[id];
				pointer.resolve(result);
			}
		}
	},

	/*
	 * when the server executes something on the client, this is how it gets the return value...
	 * for example the server will execute this javascript code on the client:
	 *  callbackOnServer(1234, 1 + 2);
	 * and on the server whatever is registered to be waiting under 1234 gets the result (3).
	 */
	callbackOnServer: function(id, result){
		throw "not implemented";
	},


	error: function(id, error){
		if(id == null)
		{
			console.error("xbridge global error: " + error);
		}else{
			var pointer = promises[id];
			if(pointer == null)
			{
				console.error("no promise given for id: " + id);
			}else{
				//console.error("error (thus rejecting promise...): " + error);
				delete promises[id];
				pointer.reject(error);
			}
		}
	},


	trigger: function(event)
	{
		event = me.triggerOnClient(event);
		me.exec("events.triggerOnServer", event.type, event);
	},

	log: function(value)
	{
		//console.log(value);
		this.exec('core.log', value);
	},


	triggerOnClient: function(event, __todoEventObject)
	{
		if(typeof event === 'string')
			event = {type: event}
		else if(event.type == null)
			throw "event.type cannot be null";
		var list = listeners[event.type]
		//console.log('triggerOnClient: ' + event.type);
		//console.log('listeners: ');
		//console.dir(list);
		if(list != null)
		{
			list = list.slice();
			for (var i = 0; i < list.length; i++) {
				try{
					list[i](event);
				}catch(err)
				{
					if(err.stack)
						console.error("error in " + event.type + " event handler: " + err.stack);
					else
						console.error("error in " + event.type + " event handler: " + err);
				}
			}
		}
		return event;
	},

	_triggerOnClientWaiting: function(id, event)
	{
		console.log('this must complete or the app will freeze');
		if(typeof event === 'string')
			event = {type: event}
		else if(event.type == null)
			throw "event.type cannot be null";
		var list = listeners[event.type];
		if(list != null)
		{
			var num = list.length;
			function finishOne(){
				if(--num == 0)
					xbridge.exec('events.finishOnServer', id);
			}
			for (var i = 0; i < list.length; i++) {
				var result;
				try{
					result = list[i](event);
				}catch(err)
				{
					console.error(err);
				}
				if(result != null && typeof result.then === 'function')
					result.then(finishOne);
				else
					finishOne();
			}
		}else{
			xbridge.exec('events.finishOnServer', id);
		}
		return event;
	},


	on: function(event, listener)
	{
		var list = listeners[event];
		if(list == null)
			list = listeners[event] = [];
		list.push(listener);
	},

	off: function(event, listener)
	{
		if(listener == null)
			delete listeners[event];
		else{
			var list = listeners[event];
			if(list != null)
			{
				var index = list.indexOf(listener);
				if(index > -1)
					list.splice(index, 1);
			}
		}
	},
	
	one: function(event, listener)
	{
		var me = this;
		function ls(){
			me.off(event, ls);
			listener.apply(me, arguments);
		}
		me.on(event, ls);
	},

	// supposed to be called by the connection implementation
	_init: function(client)
	{
		console.log('xbridge: initialized using client: ' + client);
		this.client = client;
		this.connected = true;
		this.connecting = false;
		this.oneTimePromises.init = true;

		this.trigger("init");
	},

	promise: function(hook, origin){
		var me = this;
		return new Promise(function(res){
			if(oneTimePromises[hook])
				res();
			else
				me.one(hook, function(){
					//console.log('hook: ' + hook + ', origin: ' + origin);
					res();
				});
		});
	},

	_registerSharedTypeOnClient: function(id, fqn, methods)
	{
		console.log('registering shared type: ' + id + ' ' + fqn);
		var prototype = {
			id: "SharedType"
		};
		methods.map(function(method){
			prototype[method] = function(){
				var args = Array.prototype.slice.call(arguments);
				args = [this.id + '.' + method].concat(args);
				return xbridge.exec.apply(xbridge, args);
			}
		});
		prototype.destroy = function(){
			return xbridge.exec(this.id + '.destroy').then(function(){
				delete sharedObjects[this.id];
			});
		};
		prototype = Object.assign({}, kutil.eventsPrototype, prototype);

		function Constructor(id){
			this.id = id;
		}
		Constructor.prototype = prototype;
		Constructor.name = fqn;
		Constructor.id = id;
		sharedTypes[id] = Constructor;
		return Constructor;
	},

	_callbackOnClientWithSharedObject: function(callbackId, typeid, id)
	{
		var o = sharedObjects[id];
		if(o != null)
			this.callbackOnClient(callbackId, o);
		else{
			var type = sharedTypes[typeid];
			if(type == null)
			{
				xbridge.exec('core.getSharedType', typeid).then(function(type){
					type = xbridge._registerSharedTypeOnClient(type.id, type.name, type.methods);
					xbridge.callbackOnClient(callbackId, sharedObjects[id] = new type(id));
				}).catch(function(err){
					xbridge.error(callbackId, err);
				});
			}else
				this.callbackOnClient(callbackId, sharedObjects[id] = new type(id));
		}
		
	},

	uninitialized: function(moduleName)
	{
		uninitialized[moduleName] = true;
	},

	initialized: function(moduleName)
	{
		delete uninitialized[moduleName];
	},




	_triggerOnSharedObject: function(id, event, __todoEventObject)
	{
		sharedObjects[id].trigger(event);
	}

}

xbridge.once = xbridge.one;

//shorthand aliases (those aint public..., might change anytime)
xbridge._cc = xbridge.callbackOnClient;
xbridge._cs = xbridge.callbackOnServer;
xbridge._tc = xbridge.triggerOnClient;
xbridge._tcw = xbridge._triggerOnClientWaiting;
xbridge._rst = xbridge._registerSharedTypeOnClient;
xbridge._ccso = xbridge._callbackOnClientWithSharedObject;
xbridge._tso = xbridge._triggerOnSharedObject;

var oneTimePromises = {}

xbridge.oneTimePromises = oneTimePromises;

xbridge.promise("init", "xbridge.go").then(function(){
	function triggerGo(){
		//console.log('go');
		oneTimePromises.go = true;
		xbridge.trigger("go");
	}
	window.setTimeout(function(){
		if (document.readyState === "complete") { 
			triggerGo(); 
		}else{
			if(window.addEventListener) {
				//console.log('addEventListener');
				window.addEventListener('load',triggerGo,false); //W3C
			} else {
				//console.log('attachEvent');
				window.attachEvent('onload',triggerGo); //IE
			}
		}
	}, 0);
});

window.xbridge = xbridge;

})();