using System;
using System.Threading.Tasks;
using xbridge;
namespace xbridge.websocket.Modules
{


    public class Clipboard
    {
        public Clipboard(XBridge bridge)
        {
        }

        public string Get()
        {
            string text = "pbpaste".Bash();
            return text;
        }

        public void Set(String value)
        {
            $"echo \"{value}\" | pbcopy".Bash();
        }

    }
}
