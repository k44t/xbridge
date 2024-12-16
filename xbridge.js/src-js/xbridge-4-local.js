(function(){

var fileSelector;
var fileSender = document.createElement('a');

var modules = {
	files: {},
};
var methods = {
	'core.hasMethod': function(module, method)
	{
		return methods[module + '.' + method] != null;
	},
	'core.hasModule': function(module)
	{
		var mod = modules[module];
		if(mod == null)
			return false;
		return true;
	},
	'events.triggerOnServer': kutil.nop,
	'files.pick': function(){
		return new Promise(function(resolve, reject){

			fileSelector = document.createElement('input');
			fileSelector.setAttribute("type", "file");
			//document.body.appendChild(fileSelector);
			fileSelector.onchange = function(e){
				resolve(e.target.files[0].name);
			};
			var listener = function(){
				window.removeEventListener('focus', listener);
				window.setTimeout(function(){
					resolve(null);
				}, 500);
			};
			window.addEventListener('focus', listener);
			fileSelector.click();
		});
	},
	'files.read': function(path){
		return new Promise(function(resolve, reject){
			if(fileSelector == null || fileSelector.files[0] == null || path != fileSelector.files[0].name)
				throw "in the browser files must first be picked by the user (`xbridge.exec('files.pick')`)";
			var reader = new FileReader();
			reader.onload = function(e){
				resolve(e.target.result);
			}
			reader.readAsText(fileSelector.files[0]);
		});
	},
	'files.exists': function(){
		return new Promise(function(resolve, reject){
			resolve(false);
		});
	},
	'files.saveAs': function(suggested){
		return new Promise(function(resolve, reject){
			resolve(suggested || "file.txt");
		});
	},
	'files.write': function(path, text){
		return new Promise(function(resolve, reject){
			var uriContent = "data:application/octet-stream," + encodeURIComponent(text);
			fileSender.setAttribute("href", uriContent)
			fileSender.setAttribute("download", path);
			fileSender.click();
			resolve();
		});
	},
	'vibration.vibrate': function(ms){
		return kutil.promise(function(){
			window.navigator.vibrate(ms);
		})
	}
}

methods = Object.assign(methods, xbridge._common);

function exec(method)
{
	var args = Array.prototype.slice.call(arguments, 1);
	//console.log('xbridge: local: exec: ' + method);
	return new Promise(function(resolve, reject){
		if(method.indexOf('.') < 0)
			method = 'core.' + method;
		var fn = methods[method];
		if(fn == null)
			fn = xbridge.local[method];
		if(fn == null)
			reject('xbridge: local: method not supported: ' + method);
		var result = fn.apply(null, args);
		resolve(result);
	});
}

xbridge._local = {
	exec: exec,
	methods: methods,
	modules: modules
};

if(!xbridge.connecting && !xbridge.connected)
{
	console.log('xbridge: local: connected');
	xbridge.exec = exec;
	xbridge._remote = function(){
		return new Promise(function(_, rej){
			rej("xbridge: local: no remote calls possible")
		});
	}
	xbridge._init('local');
}

})();