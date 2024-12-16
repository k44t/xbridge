using System;
using AppKit;
using Foundation;

namespace xbridge.macos
{
    public class XBridgeDNDView: NSView
    {
        public XBridgeDNDView()
        {
        }

        [Export("draggingEntered:")]
        public NSDragOperation DraggingEntered(NSDraggingInfo sender)
        {

            return NSDragOperation.All;
        }
    }
    
}
