using System;
using AppKit;
using Newtonsoft.Json.Linq;
using WebKit;
using xbridge.macos.sample;
using System.Collections.Generic;
using CoreGraphics;

namespace xbridge.macos.Modules
{
    enum DraggingState
    {
        Enabled,
        Disabled,
        Running
    }

    public class Window
    {
        private WKWebView view;
        private NSWindow window;
        private XBridge bridge;
        private XBridgeMacosAdapter adapter;

        public List<string> CustomStyles = new List<string>();
        private DraggingState dragging = DraggingState.Disabled;
        private CGPoint point;

        public Window(XBridge xbridge)
        {
            this.bridge = xbridge;
            this.adapter = (XBridgeMacosAdapter)xbridge.Adapter;
            this.view = adapter.View;
            this.window = adapter.View.Window;
            window.DidMove += Window_DidMove;
        }

        void Window_DidMove(object sender, EventArgs e)
        {
            var evts = bridge.GetModule<xbridge.Modules.Events>();
            if (evts != null)
                evts.Trigger("window.move", null);
        }


        public object Title(string title)
        {
            if (title == null)
                return this.view.Window.Title;
            this.view.Window.Title = title;
            return null;
        }
        public void Alert(string msg)
        {
            var a = new NSAlert();
            a.AddButton("OK");
            a.MessageText = msg;
            a.RunModal();
        }
        public bool Confirm(string msg)
        {
            var a = new NSAlert();
            a.AddButton("OK");
            a.AddButton("Cancel");
            a.MessageText = msg;
            return a.RunModal() == 1;
        }
        // does not work...
        public void Activate()
        {
            view.Window.MakeKeyAndOrderFront(adapter.AppDelegate);
        }
        public JObject Rect(JObject rect)
        {
            var frame = view.Window.Frame;
            if (rect == null)
            {
                rect = new JObject()
                {
                    { "l", (float)frame.X },
                    { "t", (float)(NSScreen.Screens[0].Frame.Height - frame.Height - frame.Y )},
                    { "w", (float)frame.Width },
                    { "h", (float)frame.Height }
                };
                return rect;
            }
            // set...

            frame.Width = (nfloat)rect.GetValue("w").ToObject<float>();
            frame.Height = (nfloat)rect.GetValue("h").ToObject<float>();

            frame.X = ((nfloat)rect.GetValue("l").ToObject<float>());
            frame.Y = NSScreen.Screens[0].Frame.Height - frame.Height - ((nfloat)rect.GetValue("t").ToObject<float>());
            //bridge.Log("x: " + frame.X);
            //bridge.Log("y: " + frame.Y);
            view.Window.SetFrame(frame, true);
            return null;
        }


        public void StartDrag(double x, double y)
        {
            var frame = view.Window.Frame;
            //x += frame.X;
            //y += frame.Y;
            var viewFrame = view.Frame;
            //x += viewFrame.X;
            //y += viewFrame.Y - viewFrame.Height;
            x = frame.X + viewFrame.X + x;
            y = frame.Y + viewFrame.Y + (viewFrame.Height - y);
            var point = new CGPoint(x, y);
            var e = NSEvent.MouseEvent(NSEventType.LeftMouseDown, point, 0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), 0, view.Window.GraphicsContext, 0, 1, 0);
            view.Window.PerformWindowDrag(e);
        }

        internal void _AddCustomStyle(string v)
        {
            CustomStyles.Add(v);
        }

        public string[] Styles()
        {
            var list = new List<string>();
            if(view.Window.TitlebarAppearsTransparent && view.Window.StyleMask == NSWindowStyle.FullSizeContentView)
            {
                list.Add("full-size");
            }
            list.AddRange(CustomStyles);
            return list.ToArray();
        }
    }
}
