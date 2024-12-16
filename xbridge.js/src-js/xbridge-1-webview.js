(function(){

if(window.__wkxbridge__ != null)
{
	var server = window.__wkxbridge__;
	xbridge._send = function(url)
	{
		//console.log("bridge-webview: _send: '" + url + "'");
		server.exec(url);
	}
	xbridge._init('webview');
}else if(window.webkit != null && window.webkit.messageHandlers.__wkxbridge__ != null)
{
	var server = window.webkit.messageHandlers.__wkxbridge__;
	xbridge._send = function(url)
	{
		//console.log("bridge-webview: _send: '" + url + "'");
		server.postMessage(url);
	}
	xbridge._init('webview');
}

})();