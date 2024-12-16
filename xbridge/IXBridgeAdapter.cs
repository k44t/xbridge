using System;
using System.IO;
using System.Threading.Tasks;

namespace xbridge
{
    public interface IXBridgeAdapter
    {
        XBridge Bridge { set; }
        void ExecJS(string command);
        void SetLocation(string url);
    }
}
