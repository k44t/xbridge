(function(){

if(xbridge._local)
{
	var AudioContext = window.AudioContext || window.webkitAudioContext;
	var sounds = {};

	function Sound(context, buffer)
	{
		this.context = context;
		this.buffer = buffer;
	}

	Sound.prototype = {
		'volume': function(volume)
		{
			var sound = this;
			if(sound.gain == null)
			{
				sound.gain = sound.context.createGain()
				if(sound.source)
					sound.source.connect(sound.gain);
				sound.gain.connect(sound.context.destination);
			}
			sound.gain.gain.value = volume;
			return kutil.promise();
		},
		'ended': function(id){
			var sound = this;
			return new Promise(function(res, rej){
				if(sound.source == null)
					rej(new Error('not playing'));
				if(sound.context.state != 'running')
					rej(new Error('not playing'));
				var source = sound.source;
				function listener(){
					source.removeEventListener('ended', listener);
					res();
				}
				sound.source.addEventListener('ended', listener);
			})
		},
		'play': function(id)
		{
			var sound = this;
			sound.context.resume();
			return new Promise(function(res, rej){
				if(sound.source == null)
				{
					sound.source = sound.context.createBufferSource();
					sound.source.buffer = sound.buffer;
					if(sound.gain != null)
						sound.source.connect(sound.gain);
					else
						sound.source.connect(sound.context.destination);
					sound.source.addEventListener('ended', function(){
						sound.source.disconnect();
						sound.source = null;
						//console.log('closing source');
					});
					//sound.source.loop = true;
					sound.source.start();
					//
				}
				setTimeout(function(){
					//sound.context.resume();
					if(sound.context.state == 'running')
						res();
					else
						rej(new Error('could not start AudioContext. Some browsers limit the ability to start audio to event handlers. You might have to add a play button.'));
				}, 0);
			})
		},
		'pause': function(id)
		{
			var sound = this;
			sound.context.suspend();
			return kutil.promise();
		},
		'playing': function(id)
		{
			return kutil.promise(this.context.state == 'running');
		},
		'stop': function(id)
		{
			var sound = this;
			sound.context.suspend();
			sound.source = null;
			return kutil.promise();
		},
		'destroy': function(id)
		{
			var sound = this;
			sound.source.disconnect();
			if(sound.gain)
				sound.gain.disconnect();
			sound.context.close();
		}
	}


	var sound = {
		create: function(url, channel)
		{
			return new Promise(function(res, rej){
				try{
					var context = new AudioContext();
					var request = new XMLHttpRequest();
					request.open('GET', url, true);
					request.responseType = 'arraybuffer';

					// Decode asynchronously
					request.onload = function() {
						context.decodeAudioData(request.response, function(buffer) {
							res(new Sound(context, buffer));
							
						}, function(err){
							rej(err);
						});
					};
					request.onerror = rej;
					request.send();
				}catch(err){
					rej(err);
				}
			});
		},
		// channel argument is only valid with xbridge server
		'load': function(id, url, channel)
		{
			sounds[id] = this.create(url, channel);
		},
		// must be called inside an event handler, only required if no xbridge backend is present...
		'enable': function(){
			new AudioContext();
			for(var k in sounds)
			{
				if(sounds.hasOwnProperty(k))
				{
					sounds[k].context.resume();
				}
			}
			return new Promise(function(res){
				res();
			})
		},
		'volume': function(id, volume)
		{
			return sounds[id].volume(volume);
		},
		'ended': function(id){
			return sounds[id].ended();
		},
		'play': function(id)
		{
			return sounds[id].play();
		},
		'pause': function(id)
		{
			return sounds[id].pause();
		},
		'playing': function(id)
		{
			return sounds[id].playing();
		},
		'stop': function(id)
		{
			return sounds[id].stop();
		},
		'destroy': function(id)
		{
			sounds[id].destroy;
			delete sounds[id];
		}
	}

	xbridge.uninitialized('sound');

	xbridge.promise("init", 'sound').then(function(){
		xbridge.exec('core.hasModule', 'sound').then(function(has){
			if(has)
			{
				xbridge.local('sound', {
					enable: kutil.promise
				});
			}else{
				xbridge.local('sound', sound);
			}
			xbridge.initialized('sound');
		});
	});
}



})();