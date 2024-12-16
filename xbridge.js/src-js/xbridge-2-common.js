xbridge._common = {
	'core.alert': function(msg)
	{
		alert(msg);
	},
	'core.log': kutil.nop,
	'window.title': function(title)
	{
		if(title != null)
		{
			document.title = title;
		}else
		{
			return document.title;
		}
	},
	'resources.list': function(directory){
		var parentPath = window.location.pathname;
		if(directory.startsWith('/'))
			parentPath = directory;
		else if(directory)
			parentPath = parentPath + '/' + directory;
		var uri = new URI(window.location);
		uri.path(parentPath);
		uri.normalizePathname(uri);
		parentPath = uri.path();
		return fetch(directory).then(function(response){
			return response.text();
		}).then(function(text){
			var regex = /href="([^"]+)/g
			var resources = [];
			while (match = regex.exec(text))
			{
				match = match[1];

				var index = match.indexOf('/');
				if(index < 0)
					resources.push(match);
				else
				{
					if(index == match.length - 1)
					{
						resources.push(match);
					}else if(match.startsWith(directory) && match.length > directory.length)
					{
						resources.push(match.substring(directory.length));
					}else if(match.startsWith(parentPath) && match.length > parentPath.length)
					{
						resources.push(match.substring(parentPath.length));
					}
				}
			}
			return resources;
		});
	}
}
