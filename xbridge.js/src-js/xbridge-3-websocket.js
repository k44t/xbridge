(function(){

var remoteExec = xbridge._remote;
var localMethods = xbridge._common;

var socket;

function send(url)
{
	//console.log("xbridge: websocket: _send: '" + url + "'");
	try{
		socket.send(url);
	} catch(err)
	{
		console.err(err);
	}
}


function connect(url, timeout)
{
	xbridge.connecting = true;
	if(url == null)
		if (location.protocol === 'https:') {
			url = 'wss:localhost:8444';
		}else{
			url = 'ws:localhost:8084';
		}
	if(timeout == null)
		timeout = 3000;
	return new Promise(function(resolve, reject){

		if ("WebSocket" in window) {
			//console.log("xbridge: websocket: trying to connect to `" + url + "`");
			try{
				// Let us open a web socket
				socket = new WebSocket(url);

				socket.onmessage = function (evt) {
					if(evt.data == 'connected')
					{
						console.log('xbridge: websocket: connected');
						resolve();
						socket.onmessage = normalMessage;
					}else{
						var msg = "xbridge: websocket: service is not protocol conform: service answer should have been 'connected' but was: '" + evt.data + "'";
						console.error(msg);
						reject(msg);
					}
				};
				function normalMessage(evt)
				{
					eval(evt.data);
				}

				socket.onclose = function(evt)
				{
					if(evt.code != 3001)
						reject("The connection was closed abnormally (without sending or receiving a close control frame)");
				}

				setTimeout(function(){
					reject('Connection to websocket has timed out. This exception usually occurs when the request is blocked by some security policy. Maybe you are trying to connect to an unsecure websocket (ws) instead of a secure websocket (wss) from a secure page (https)?');
				}, timeout);
			}catch(err){
				reject(err);
			}
		} else {
			// The browser doesn't support WebSocket
			reject("WebSocket NOT supported by your Browser!");
		}
	}).then(function(){
		Object.assign(xbridge.local, xbridge._common);
		xbridge._send = send;
		xbridge._init('websocket');
	}).catch(function(err){
		xbridge.connecting = false;
		console.warn(err);
		if(xbridge._local && !xbridge.connected)
		{
			xbridge.exec = xbridge._local.exec;
			xbridge._init('local');
		}
	});
}

xbridge._websocket = {
	connect: connect
};

if(!xbridge.connected && xbridge.auto != false && (xbridge.url != null || location.hostname == 'localhost'))
{
	connect(xbridge.url);
}

})();


