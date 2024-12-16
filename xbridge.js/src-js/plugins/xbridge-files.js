(function(){



Object.assign(xbridge.local, {
	// channel argument is only valid with xbridge server
	'files.pickAndRead': function()
	{
		return xbridge.exec('files.pick').then(function(fileName){
			if(fileName == null)
			{
				return null;
			}
			console.log('picked: ' + fileName);
			return xbridge.exec('files.read', fileName);
		});
	}
});


if(!xbridge.local['files.saveAs'])
	xbridge.local['files.saveAs'] = function(suggested)
	{
		return $.kui({type: 'xbridge-Files', pos: {width: 'max', height: 'max'}, mode: 'save'}).kui('show', {title: false, name: suggested}).then(function(result){
				console.log('>> closing dialog, result: ' + result);
				//files.kui('hide');
				//router.back();
				return result;
		});
	}
if(!xbridge.local['files.pick'])
	xbridge.local['files.pick'] = function()
	{
		return $.kui({type: 'xbridge-Files', pos: {width: 'max', height: 'max'}, mode: 'open'}).kui('show', {title: false}).then(function(result){
				console.log('>> closing dialog, result: ' + result);
				//files.kui('hide');
				//router.back();
				return result;
		});
	}

})();