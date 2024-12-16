(function(){

var audio;

Object.assign(xbridge.local, {
	'background.enable': function(){
		var args = Array.prototype.slice.call(arguments);
		args.splice(0,0, 'background.enable');
		return xbridge._remote.apply(xbridge, args).then(function(){
			if(!audio)
			{
				audio = kutil.emptySound();
				audio.loop = true;
				audio.volume = 0;
				audio.muted = true;
			}
			audio.play();

		})
	},
	'background.disable': function(){
		var args = Array.prototype.slice.call(arguments);
		args.splice(0,0, 'background.disable');
		return xbridge._remote.apply(xbridge, args).then(function(){
			// todo stop playing empty sound
			if(audio)
				audio.pause();
		})
	}
});
	
})();