using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using Android.Content.Res;
using Android;
using Android.Content.PM;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using xbridge.Modules;
using Java.Interop;
using System.Net;
using Android.Support.V7.App;
#if false
using MimeSharp;
#endif

namespace xbridge.android
{

    public class Handler : Java.Lang.Object
    {
        private XBridge bridge;
        private WebView view;

        public Handler(WebView view, XBridge bridge)
        {
            this.view = view;
            this.bridge = bridge;
        }

        [JavascriptInterface]
        [Export("exec")]
        public void Exec(String msg)
        {
            // the command execution runs on the view thread...
            //view.Post(() =>
            //{
            //Task.Run(() => bridge.HandleDecodedURL(msg));
            //});
            bridge.HandleDecodedURL(msg);
        }
    }
    public class XBridgeAndroidAdapter : WebViewClient, IXBridgeAdapter
    {
        private class MyWebChromeClient : WebChromeClient
        {

            override public bool OnCreateWindow(WebView view, bool dialog, bool userGesture, Android.OS.Message resultMsg)
            {
                WebView.HitTestResult result = view.GetHitTestResult();
                String data = result.Extra;
                Context context = view.Context;
                var uri = Android.Net.Uri.Parse(data);
                var browserIntent = new Intent(Intent.ActionView, uri);
                context.StartActivity(browserIntent);
                return false;
            }
        }


        private WebView view = null;

        internal static XBridgeAndroidAdapter Get(XBridge xbridge)
        {
            return (XBridgeAndroidAdapter)xbridge.Adapter;
        }

        private AssetManager assets;
        public AppCompatActivity Activity;
        private Handler handler;
        private XBridge bridge;


        public Context Context { get { return view.Context; } }

        public XBridge Bridge
        {
            get
            {
                return bridge;
            }
            set
            {
                bridge = value;
                handler = new Handler(view, bridge);
                view.AddJavascriptInterface(handler, "__wkxbridge__");
            }
        }

        public XBridgeAndroidAdapter(AppCompatActivity activity, WebView view, AssetManager assets)
        {
            this.view = view;
            this.assets = assets;
            this.Activity = activity;
            view.Settings.JavaScriptEnabled = true;
            view.Settings.SetSupportMultipleWindows(true);
            view.Settings.MediaPlaybackRequiresUserGesture = false;
            view.Settings.AllowUniversalAccessFromFileURLs = true;

            // Use subclassed WebViewClient to intercept hybrid native calls
            view.SetWebViewClient(this);

            view.SetWebChromeClient(new MyWebChromeClient());

        }


        public string LocalRoot
        {
            get
            {
                return "file:///android_asset/";
            }
        }

        public string Host { get; set; } = "localhost";

        public Stream OpenPackagedFile(string name)
        {
            return assets.Open(name);
        }

        public void ExecJS(string command)
        {
            //view.LoadUrl("javascript:" + command);
            //view.LoadUrl("file:///android_asset/style.css");
            Activity.RunOnUiThread(() =>
            {
                view.EvaluateJavascript(command, null);
            });


        }

        public void SetLocation(string url)
        {
            view.LoadUrl(url);
        }
#if false
        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            try
            {

                var url = request.Url.ToString();
                if (url.StartsWith("https://" + this.Host + "/", StringComparison.Ordinal))
                {
                    if (url.Contains(".."))
                        throw new Exception("parent path not allowed");

                    url = url.Substring(("https://" + this.Host + "/").Length);
                    //url = "file:///android_asset/" + url;
                    var mime = new Mime().Lookup(url);
                    if (mime == "application/xhtml+xml")
                        mime = "text/html";

                    var stream = this.Activity.Assets.Open(url);
                    var headers = new Dictionary<string, string>();
                    /*
                    headers["Accept-Ranges"] = "bytes";
                    headers["Access-Control-Allow-Origin"] = "*";
                    headers["Last-Modified"] = "Mon, 14 Jan 2019 13:42:16 GMT";
                    headers["Content-Type"] = "text/html; charset=UTF-8";
                    headers["ETag"] = "W/\"a208-1684c99378d\"";
                    */
                    return new WebResourceResponse(mime, "UTF-8", 200, "OK", headers, stream);
                    /*
                    using (WebClient client = new WebClient())
                    {
                        client.OpenRead(url);
                        //Console.WriteLine(mime);
                        //else if (mime == "application/javascript")
                        //    mime = "text/javascript";
                        var result = client.DownloadData(url);
                        var stream = new MemoryStream(result);
                        return new WebResourceResponse(mime, "UTF-8", stream);
                    }*/
                }
            }
            catch (Exception ex)
            {
                var headers = new Dictionary<string, string>();
                return new WebResourceResponse("text/html", "UTF-8", 404, "NOT FOUND", headers, new MemoryStream(new byte[] { }));
            }
            return base.ShouldInterceptRequest(view, request);
        }
#endif


        internal AssetFileDescriptor GetAssetFileDescriptor(string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal))
                path = path.Substring(1);
            return Activity.Assets.OpenFd(path);
        }
    }


}
