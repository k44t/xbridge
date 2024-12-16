(function(){

if(xbridge._local)
{
	

	var core = {
		// channel argument is only valid with xbridge server
		'core.platform': function()
		{
			return xbridge.exec('core.os').then(function(os){
				if(os.startsWith('mac') || os.startsWith('windows') || os.startsWith('linux'))
					return 'desktop';
				else
					return 'mobile';
			}).catch(function(){
				if(kutil.isMobile())
					return 'mobile-browser';
				else
				{
					return 'desktop-browser';
				}
			});
		}
	}

	Object.assign(xbridge._local.methods, core);
	Object.assign(xbridge.local, core);

}


})();