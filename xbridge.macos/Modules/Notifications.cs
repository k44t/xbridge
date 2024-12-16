using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AppKit;
using Foundation;
using Newtonsoft.Json.Linq;
using WebKit;
using xbridge.macos.sample;

namespace xbridge.macos.Modules
{
    public class Notifications
    {
        private XBridge bridge;

        public Notifications(XBridge xbridge)
        {
            this.bridge = xbridge;
            NSUserNotificationCenter.DefaultUserNotificationCenter.DidActivateNotification += DefaultUserNotificationCenter_DidActivateNotification;
        }

        IndexedTasks<object> tasks = new IndexedTasks<object>();

        public void Show(String id, String title, String description, string defaultAction, object more, String close)
        {
            var notification = new NSUserNotification();

            // Add text and sound to the notification
            notification.Title = title;
            notification.InformativeText = description;
            notification.SoundName = NSUserNotification.NSUserNotificationDefaultSoundName;

            notification.HasActionButton = defaultAction != null;
            if(defaultAction != null)
                notification.ActionButtonTitle = defaultAction;
            if (more is String)
            {
                notification.HasReplyButton = more is String;
                notification.ResponsePlaceholder = (string)more;
            }
            if (close != null)
                notification.OtherButtonTitle = close;
            if(more != null && more.GetType().IsArray)
            {
                var arr = (object[]) more;
                var actions = new NSUserNotificationAction[arr.Length];
                for (var i = arr.Length - 1; i >= 0; i--)
                {
                    var elem = (object[])arr[i];
                    actions[i] = NSUserNotificationAction.GetAction((string)elem[0], (string)elem[1]);
                }
                notification.AdditionalActions = actions;

            }
            notification.Identifier = id;
            NSUserNotificationCenter.DefaultUserNotificationCenter.DeliverNotification(notification);
        }

        void DefaultUserNotificationCenter_DidActivateNotification(object sender, UNCDidActivateNotificationEventArgs e)
        {
            var events = bridge.GetModule<xbridge.Modules.Events>();
            if (events != null && e.Notification.Identifier != null)
            {
                var evt = new JObject();
                evt["type"] = "notification";
                evt["notification"] = e.Notification.Identifier;
                if (e.Notification.ActivationType == NSUserNotificationActivationType.Replied)
                {
                    if (e.Notification.Response != null)
                    {
                        evt["action"] = "reply";
                        evt["response"] = e.Notification.Response.Value;
                    }
                }
                else if (e.Notification.ActivationType == NSUserNotificationActivationType.ActionButtonClicked)
                {
                    evt["action"] = "default";
                }
                else if (e.Notification.ActivationType == NSUserNotificationActivationType.AdditionalActionClicked)
                {

                    evt["action"] = e.Notification.AdditionalActivationAction.Identifier;
                }
                else if (e.Notification.ActivationType == NSUserNotificationActivationType.ContentsClicked)
                {
                    evt["action"] = null;
                }

                events.Trigger((string)evt["type"], evt);
            }
        }
    }
}