using System;
using Foundation;
using UIKit;
using WebKit;

namespace xbridge.ios
{
    public class XBridgeIOSAdapter : NSObject, IXBridgeAdapter, IWKScriptMessageHandler
    {
        private WKWebView view = null;
        public XBridge Bridge { get; set; }



        public XBridgeIOSAdapter(WKWebView view)
        {
            this.SetView(view);
        }

        public XBridgeIOSAdapter()
        {
        }

        private XBridgeIOSAdapter(XBridge bridge)
        {
            this.Bridge = bridge;
        }

        public void SetView(WKWebView view)
        {
            this.view = view;
        }


        public void ExecJS(string command)
        {
            //  this will run on the right thread
            InvokeOnMainThread(() =>
            {
                view.EvaluateJavaScriptAsync(command);
            });
        }

        public WKWebView CreateWebView()
        {
            var conf = new WKWebViewConfiguration();
            conf.AllowsAirPlayForMediaPlayback = true;
            conf.AllowsInlineMediaPlayback = true;
            conf.MediaPlaybackAllowsAirPlay = true;
            conf.RequiresUserActionForMediaPlayback = false;

            var ctrl = new WKUserContentController();

            ctrl.AddScriptMessageHandler(this, "__wkxbridge__");

            conf.UserContentController = ctrl;

            var view = new WKWebView(frame: UIScreen.MainScreen.Bounds, configuration: conf);


            view.AllowsBackForwardNavigationGestures = true;
            this.view = view;
            return view;
        }

        public void SetLocation(string url)
        {
            view.LoadRequest(new NSUrlRequest(new NSUrl(url)));
        }


        [Export("exec:")]
        public bool Exec(string xbridgeCommand)
        {
            return Bridge.HandleDecodedURL(xbridgeCommand);
        }

        [Export("isSelectorExcludedFromWebScript:")]
        public static bool IsSelectorExcludedFromWebScript(ObjCRuntime.Selector aSelector)
        {
            // For security, you must explicitly allow a selector to be called from JavaScript.
            if (aSelector.Name == "exec:")
                return false;

            return true; // disallow everything else
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            string msg = (NSString)message.Body;
            Bridge.HandleDecodedURL(msg);
        }

    }
}
