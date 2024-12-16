using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace xbridge.Modules
{
    public class TestObject: XBridgeSharedObject
    {
        public TestObject(XBridge xbridge) : base(xbridge)
        {
        }

        public String Test(string who)
        {
            return "Hello " + who + "!";
        }
    }
    public class SharedObjects
    {
        private XBridge bridge;

        public SharedObjects(XBridge bridge)
        {
            this.bridge = bridge;
        }

        public TestObject Test()
        {
            return new TestObject(bridge);
        }

        public void DestroyAll() {
            bridge.DestroyAllSharedObjects();
        }
    }

}
