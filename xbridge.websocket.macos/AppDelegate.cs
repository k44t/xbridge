using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using SuperSocket.SocketBase.Config;
using xbridge.macos;

namespace xbridge.websocket.macos
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        public AppDelegate()
        {
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var window = new XBridgeWindow(new CoreGraphics.CGRect(100, 100, 400, 400), NSWindowStyle.FullSizeContentView | NSWindowStyle.Closable | NSWindowStyle.Miniaturizable | NSWindowStyle.Resizable | NSWindowStyle.Titled, NSBackingStore.Buffered, false);
            window.MakeKeyAndOrderFront(this);
            window.Title = "XBridge Websocket Server";
            var text = new NSTextField();
            window.ContentView = text;
            text.Editable = false;
            text.DrawsBackground = false;
            text.Selectable = false;
            text.Bezeled = false;

            Console.WriteLine(System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            var service = new XBridgeWebsocket((bridge) => bridge
                                               .Export(typeof(xbridge.Modules.Events))
                                               .Export(typeof(xbridge.Modules.SharedObjects))
                                               .Export(typeof(xbridge.macos.Modules.Core))
                                               .Export(typeof(xbridge.macos.Modules.Notifications))
                                               .Export(typeof(xbridge.Modules.Files))
                                               .Export("sqlite", typeof(xbridge.Modules.SQLite))
                                               .Export(typeof(xbridge.websocket.Modules.Clipboard)));
            var port = 8084;
            service.Setup(new RootConfig(), new ServerConfig
            {
                Port = port,
                Ip = "Any",
                MaxConnectionNumber = 100,
                MaxRequestLength = 1048576,
                Name = "SuperWebSocket Server"
            });
            service.Start();
            Console.WriteLine("xbridge websocket service started on port " + port + ". Press any key to stop.");


            text.StringValue = "\n\n\nThe xbridge websocket server is running on port 8084. To exit you can simply close this window or quit the application.";

        }

        public override void WillTerminate(NSNotification notification)
        {
        }
        // when the window closes, close the application too.
        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender)
        {
            return true;
        }
    }
}
