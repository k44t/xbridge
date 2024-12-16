using System;
using Newtonsoft.Json.Linq;

namespace xbridge
{
    public class XBridgeSharedObject: IXBridgeSharedObject
    {
        protected readonly XBridge Bridge;

        public XBridgeSharedObject(XBridge xbridge)
        {
            this.Bridge = xbridge;
        }
        public int ID { get; set; } = 0;
        public int TypeID { get; set; } = 0;


        protected void _Trigger(string v)
        {
            Bridge.GetModule<xbridge.Modules.Events>().TriggerOnSharedObject(this, v, null);
        }
        protected void _Trigger(string v, JObject data)
        {
            Bridge.GetModule<xbridge.Modules.Events>().TriggerOnSharedObject(this, v, data);
        }

        public virtual void Destroy()
        {

        }
    }
}
