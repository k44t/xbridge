(function(){




if(xbridge._local)
{
	function worker(){

		function sendSignal(accurateOrWarmup){
			self.postMessage(accurateOrWarmup);
		}
		function accurateInterval(interval){
			var expected = Date.now() + interval;
			var running = true;
			function step() {
				var dt = Date.now() - expected; // the drift (positive for overshooting)
				if(running)
				{
					var accuracy = Math.abs(dt);
					if (accuracy > 7) {
						//console.log('inaccurate beat: ' + dt);
					}
					//console.log('timer worker ticking');
					sendSignal(accuracy ? accuracy : 1);
					expected += interval;
					setTimeout(step, Math.max(0, interval - dt)); // take into account drift
				}
			}
			setTimeout(step, interval);
			return function(){
				//console.log('timer worker stopping');
				running = false;
			}
		}
		var stopFn;
		self.addEventListener('message', function(e){
			if(typeof e.data === 'number')
			{
				if(stopFn)
					stopFn();
				// warming up the signaling, so there is no lag at the first beat
				for (var i = 15; i >= 0; i--) {
					sendSignal(false);
				}
				//console.log('timer worker starting');
				sendSignal(null);
				stopFn = accurateInterval(e.data);
			}
			else
			{
				if(stopFn)
					stopFn();
			}
		}, false)
	};


	function Timer(milliseconds, isTimeout, sendZero)
	{
		var me = this;
		me._isTimeout = isTimeout;
		me._milliseconds = milliseconds;
		me._sendZero = sendZero;
		me._worker = kutil.createWorker(worker);
		me._worker.addEventListener("message", function(evt){
			if(evt.data || (evt.data === null && me._sendZero))
			{
				if(me._isTimeout && evt.data)
					me.stop();
				me.trigger('elapsed', me.data == true);
			}
		});
	}

	Timer.prototype = Object.assign({}, kutil.eventsPrototype, {
		sendZero: kutil.toPromise(function(v)
		{
			if(v == null)
				return this._sendZero;
			this._sendZero = v;
		}),
		start: kutil.toPromise(function()
		{
			this.running = true;
			this._worker.postMessage(this._milliseconds);
		}),
		stop: kutil.toPromise(function(){
			this.running = false;
			this._worker.postMessage(false);
		}),
		interval: kutil.toPromise(function(v)
		{
			if(v == null)
			{
				if(this.isTimeout)
					return null;
				return this._milliseconds;
			}
			this._isTimeout = false;
			this._milliseconds = v;
			if(this.running)
				this.start();
		}),
		timeout: kutil.toPromise(function(v)
		{
			if(v == null)
			{
				if(this.isTimeout)
					return null;
				return this._milliseconds;
			}
			this._isTimeout = true;
			this._milliseconds = v;
			if(this.running)
			{
				this.start();
			}
		})
	});


	var timers = {
		create: kutil.toPromise(function(milliseconds, isTimeout, sendZero)
		{
			return new Timer(milliseconds, isTimeout, sendZero);
		})
	}

	xbridge.uninitialized('timers');

	xbridge.promise("init", 'timers').then(function(){
		xbridge.exec('core.hasModule', 'timers').then(function(has){
			if(!has)
			{
				xbridge.local('timers', timers);
			}
			xbridge.initialized('timers');
		});
	});
}



})();