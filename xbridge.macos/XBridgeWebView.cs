using System;
using AppKit;
using CoreGraphics;
using Foundation;
using WebKit;

namespace xbridge.macos
{
    public class XBridgeWebView : WKWebView
    {
        public XBridgeWebView(CGRect frame, WKWebViewConfiguration configuration) : base(frame, configuration)
        {
        }
        protected internal XBridgeWebView(IntPtr handle) : base(handle)
        {
        }
        protected XBridgeWebView(NSObjectFlag t) : base(t)
        {
        }
        public XBridgeWebView(NSCoder coder) : base(coder)
        {
        }

        public override bool AcceptsFirstMouse(NSEvent theEvent)
        {
            base.AcceptsFirstMouse(theEvent);
            return true;
        }

        public override NSDragOperation DraggingEntered(NSDraggingInfo sender)
        {
            return base.DraggingEntered(sender);
        }

        public override void DraggingExited(NSDraggingInfo sender)
        {
            base.DraggingExited(sender);
        }

        public override void DraggingEnded(NSDraggingInfo sender)
        {
            base.DraggingEnded(sender);
        }

        public override NSDragOperation DraggingUpdated(NSDraggingInfo sender)
        {
            return base.DraggingUpdated(sender);
        }


    }
}
