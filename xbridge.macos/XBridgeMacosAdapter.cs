using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using WebKit;
using MimeSharp;

namespace xbridge.macos
{

    public class XBridgeMacosAdapter : NSObject, IXBridgeAdapter, IWKScriptMessageHandler, IWKUrlSchemeHandler, WebKit.IWKUIDelegate
    {
        public WKWebView View;

        public NSApplicationDelegate AppDelegate { get; }

        public XBridgeMacosAdapter(NSApplicationDelegate appDelegate)
        {
            this.AppDelegate = appDelegate;


        }

        public WKWebView Create(CoreGraphics.CGRect frame)
        {
            var conf = new WKWebViewConfiguration();
            //view.Preferences.JavaScriptCanOpenWindowsAutomatically = true;
            var ctrl = new WKUserContentController();

            ctrl.AddScriptMessageHandler(this, "__wkxbridge__");

            conf.UserContentController = ctrl;
            conf.MediaTypesRequiringUserActionForPlayback = WKAudiovisualMediaTypes.None;
            conf.AllowsAirPlayForMediaPlayback = true;

            //conf.Preferences.SetValueForKey(NSObject.FromObject(true), (NSString)"allowFileAccessFromFileURLs");
            //conf.Preferences.SetValueForKey(NSObject.FromObject(true), (NSString)"allowUniversalFileAccessFromFileURLs");
            //conf.AllowsInlineMediaPlayback = true;
            conf.SetUrlSchemeHandler(this, "resource");
            conf.AllowsAirPlayForMediaPlayback = true;

            this.View = new XBridgeWebView(frame, conf);
            this.View.UIDelegate = this;
            return this.View;

        }

        


        public string LocalRoot => "/";

        public XBridge Bridge { get; set; }

        public void ExecJS(string command)
        {
            //  this will run on the right thread
            InvokeOnMainThread(() =>
            {
                View.EvaluateJavaScriptAsync(command);
            });
        }

        async public Task<bool> GetPermission(string v)
        {
            return true;
        }

        public Stream OpenPackagedFile(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        public void SetLocation(string url)
        {
            var parent = url.Substring(0, url.LastIndexOf("/", StringComparison.Ordinal));
            View.LoadFileUrl(new NSUrl(url), new NSUrl(parent));
            //View.LoadRequest(new NSUrlRequest(new NSUrl(url)));
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
        // handles resource: urls
        public void StartUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
        {
            var url = urlSchemeTask.Request.Url.StandardizedUrl.ToString();
            if(url.Contains(".."))
                urlSchemeTask.DidFailWithError(new NSError());
            else {
                url = url.Substring("resource:".Length);
                while (url.StartsWith("/"))
                    url = url.Substring(1);
                url = "file://" + Path.Combine(NSBundle.MainBundle.BundlePath, "Contents/Resources/htdocs/") + url;
                try
                {
                    using (WebClient client = new WebClient())
                    {
                        //client.OpenRead(url);
                        var mime = new Mime().Lookup(url);
                        if (mime == "application/xhtml+xml")
                            mime = "text/html";
                        //Console.WriteLine(mime);
                        //else if (mime == "application/javascript")
                        //    mime = "text/javascript";
                        var result = client.DownloadData(url);
                        var resp = new NSUrlResponse(urlSchemeTask.Request.Url, mime, result.Length, "UTF-8");
                        urlSchemeTask.DidReceiveResponse(resp);
                        urlSchemeTask.DidReceiveData(NSData.FromArray(result));
                        urlSchemeTask.DidFinish();
                    }
                }
                catch (Exception ex)
                {
                    urlSchemeTask.DidFailWithError(new NSError());
                }
            }
        }

        public void StopUrlSchemeTask(WKWebView webView, IWKUrlSchemeTask urlSchemeTask)
        {
            urlSchemeTask.DidFailWithError(new NSError());
        }
    }
}
