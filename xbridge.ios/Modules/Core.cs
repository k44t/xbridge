using System;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json.Linq;

namespace xbridge.ios.Modules
{
    public class Core: xbridge.Modules.Core
	{
        public Core(XBridge bridge) : base(bridge)
        {
        }

        public override string AppID()
        {
            return NSBundle.MainBundle.BundleIdentifier;
        }

        public async override Task<bool> GetPermission(string v)
        {
            return true;
        }

        public override Stream OpenPackagedFile(string name)
        {
            throw new NotImplementedException();
        }

        public override string Os()
        {
            var s = NSProcessInfo.ProcessInfo.OperatingSystemVersionString;
            var version = s.Split(' ')[1];
            return "ios-" + version;
        }

        public override void Stop()
        {
            System.Environment.Exit(0);
        }
    }
}
