(function(){



xbridge.ui = {
	makeWindowDraggableOn: function(element, drag, end){
		return xbridge.exec('hasModule', 'window').then(function(has){
			if(!has) return;
			return xbridge.exec('window.rect').then(function(){
				function startHandler(evt){
					//console.log('drag start...');
					var x = evt.screenX;
					var y = evt.screenY;
					var orgPosition = null;
					xbridge.exec('window.rect').then(function(rect){
						orgPosition = rect;
					});
					var newPosition = null;
					kutil.drag({
						drag: function(evt){
							//console.log('drag event...');
							if(orgPosition)
							{
								newPosition = {
									l: orgPosition.l + evt.screenX - x,
									t: orgPosition.t + evt.screenY - y,
									w: orgPosition.w,
									h: orgPosition.h
								}
								//xbridge.log(JSON.stringify(newPosition));
								xbridge.exec('window.rect', newPosition);
								// xbridge.exec('window.rect', {l: 100, t: 100, w: 100, h: 100});
							}
							if(drag != null)
								drag(newPosition);
							evt.stopPropagation();
							evt.preventDefault();
						},
						end: function(evt){
							//console.log('drag end event...');
							if(end != null)
								end(newPosition);

						}
					}, true);
				}
				//$(element).on('mousedown', startHandler);
				$(element).on('mousedown', function(evt){
					xbridge.exec('window.startDrag', evt.clientX, evt.clientY);
				});
					
			});
		})
	},
	makeWindowRememberRect: function(storeAndRetrieveFn)
	{
		return xbridge.exec('hasModule', 'window').then(function(has){
			if(!has) return;
			return xbridge.exec('window.rect').then(function(position){
				var storeTimeout = null;
				function doStore(){
					storeAndRetrieveFn(position);
				}
				function handleChange(){
					xbridge.exec('window.rect').then(function(rect){
						position = rect;
						if(storeTimeout != null)
							clearTimeout(storeTimeout)
						storeTimeout = setTimeout(doStore, 500);
					});
				}

				$(window).on("resize", handleChange);
				xbridge.on('window.move', handleChange);
				
				return storeAndRetrieveFn().then(function(data){
					position = data;
					return xbridge.exec('window.rect', position);
				}).catch(function(){
					//not yet stored in db
				});
			})
		});
	},
	// the promise does not get rejected 
	windowInfoToCssClasses: function(element, prefix)
	{
		prefix = prefix != null ? prefix : '';
		element = $(element ? element : 'body');
		element.addClass(prefix + xbridge.client);
		return Promise.all([
			xbridge.exec('window.styles').then(function(styles){
				for (var i = 0; i < styles.length; i++) {
					element.addClass(prefix + styles[i]);
				}
			}).catch(function(err){
				return err;
			}),
			xbridge.exec('core.os').then(function(os){
				var infos = os.split(/-|\./);
				for (var i = 0; i < infos.length; i++) {
					element.addClass(prefix + infos.slice(0, i + 1).join('-'));
				}
			}).catch(function(err){
				return err;
			}),
		]).then(function(result){
			if(result.length > 0)
				throw new Error(result);
		});
	}
}

})();