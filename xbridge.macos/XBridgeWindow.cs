using System;

using Foundation;
using AppKit;
using WebKit;
using CoreGraphics;
using xbridge;
using xbridge.Modules;
using xbridge.macos;

namespace xbridge.macos
{
    public partial class XBridgeWindow : NSWindow
    {
        public WebView WebView;

        public XBridgeWindow(CGRect contentRect, NSWindowStyle aStyle, NSBackingStore bufferingType, bool deferCreation) : base(contentRect, aStyle, bufferingType, deferCreation)
        {
            this.WillEnterFullScreen += Handle_WillEnterFullScreen;
            this.WillExitFullScreen += Handle_WillExitFullScreen;
            this.TitlebarAppearsTransparent = true;
            this.TitleVisibility = NSWindowTitleVisibility.Hidden;
            this._isFull = false;

        }

        void Handle_WillExitFullScreen(object sender, EventArgs e)
        {
            this.StyleMask &= ~NSWindowStyle.FullScreenWindow;
            this._isFull = false;
            this.Toolbar = _placeholder;
        }


        void Handle_WillEnterFullScreen(object sender, EventArgs e)
        {
            this.StyleMask |= NSWindowStyle.FullScreenWindow;
            this._isFull = true;
            this.Toolbar = null;
        }

        private NSToolbar _placeholder;


        private bool _BigTitle;
        private bool _isFull;

        public bool BigTitle {
            get
            {
                return _BigTitle;
            }
            set
            {
                if(value)
                {
                    this._placeholder = new NSToolbar();
                    this._placeholder.ShowsBaselineSeparator = false;
                    if (!_isFull)
                        this.Toolbar = this._placeholder;

                }
                else
                {
                    if (this.Toolbar == this._placeholder)
                        this.Toolbar = null;
                    this._placeholder = null;

                }
                _BigTitle = value;
            }
        }



        void Handle_DidEnterFullScreen(object sender, EventArgs e)
        {
        }


        public override bool CanBecomeKeyWindow => true;

        public override bool CanBecomeMainWindow => true;

        public XBridge Bridge { get; internal set; }
    }
}
