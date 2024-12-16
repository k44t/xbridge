

using System;
using System.IO;
using System.Net;
using xbridge.android;

namespace xbridge.android.Modules
{
    public class Resources
    {
        private XBridge bridge;

        public Resources(XBridge xbridge)
        {
            this.bridge = xbridge;

        }

        public Stream _Open(string path)
        {
            if (path.StartsWith("/", System.StringComparison.Ordinal))
                path = path.Substring(1);
            return (this.bridge.Adapter as XBridgeAndroidAdapter).Activity.Assets.Open(path);
        }

        public Android.Content.Res.AssetFileDescriptor _FileDescriptor(string path)
        {
            if (path.StartsWith("/", System.StringComparison.Ordinal))
                path = path.Substring(1);
            return (this.bridge.Adapter as XBridgeAndroidAdapter).Activity.Assets.OpenFd(path);
        }

        public string[] List(string path)
        {
            if (path.StartsWith("/", System.StringComparison.Ordinal))
                path = path.Substring(1);
            if (path.EndsWith("/", System.StringComparison.Ordinal))
                path = path.Substring(0, path.Length - 1);
            var assets = (this.bridge.Adapter as XBridgeAndroidAdapter).Activity.Assets;
            var result =  assets.List(path);
            for(var i = result.Length - 1; i >= 0; i--)
            {
                try {
                    assets.Open(path + "/" + result[i]).Close();
                } catch (Exception) {
                    result[i] = result[i] + "/";
                }
            }
            return result;
        }

    }
}