

using System.IO;
using System.Net;
using Foundation;

namespace xbridge.ios.Modules
{
    public class Resources: xbridge.apple.Modules.Resources
    {
        private XBridge bridge;

        public Resources(XBridge xbridge): base(xbridge)
        {
            this.bridge = xbridge;
            prefix = "file://" + NSBundle.MainBundle.BundlePath + "/htdocs/";
        }
    }
}