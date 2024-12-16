using System;
using System.Net;
using System.Threading;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using SuperSocket.WebSocket;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using SuperSocket.Common;
using SuperSocket.WebSocket.Protocol;

namespace xbridge.websocket
{
    public class XBridgeWebsocket: WebSocketServer<XBridgeServiceSession>
    {
        private Action<XBridge> configure;

        public XBridgeWebsocket(Action<XBridge> configure){
            this.configure = configure;
            this.NewMessageReceived += Handle_NewMessageReceived;
            this.NewSessionConnected += Handle_NewSessionConnected;

        }


        void Handle_NewSessionConnected(XBridgeServiceSession session)
        {
            Console.WriteLine("session started");
            session.Send("connected");
            try
            {
                configure(session.Bridge);
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        void Handle_NewMessageReceived(XBridgeServiceSession session, string value)
        {
            Console.WriteLine("message received");
            session.Handle(value);
        }


    }

    public class XBridgeServiceSession : WebSocketSession<XBridgeServiceSession>, IXBridgeAdapter
    {
        public XBridge Bridge { set; get; }

        public XBridgeServiceSession()
        {
            Bridge = new XBridge(this);

        }



        public void Handle(string requestInfo)
        {
            Console.WriteLine("handling request: " + requestInfo);
            if(!Bridge.HandleDecodedURL(requestInfo))
                Bridge.report(null, requestInfo, "not a valid xbridge call: " + requestInfo);
        }

        public void ExecJS(string command)
        {
            Console.WriteLine("sending command to client: " + command);
            Send(command);
        }

        public void SetLocation(string url)
        {
            throw new NotSupportedException("not supported by websocket service");
        }


    }


}