using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Foundation;
using Newtonsoft.Json.Linq;
using UserNotifications;

namespace xbridge.ios.Modules
{
    public class NotificationReceiver
    {

    }

    public class Notification : XBridgeSharedObject
    {
        private Notifications notifications;

        public UNMutableNotificationContent IosContent { get; }

        private string title = "title";
        private string description = "description";
        private string icon = "icon";
        private JObject[] actions;
        private string bigIcon;
        private string color;
        private string visiblity;

        public Notification(Notifications n): base(n.Bridge)
        {
            this.notifications = n;
            this.IosContent = new UNMutableNotificationContent();
            IosContent.Badge = 1;
            IosContent.Body = description;
            IosContent.Title = title;
        }

        public string Title(string v)
        {
            if (v == null)
                return this.title;
            this.title = v;
            IosContent.Title = title;
            return null;
        }

        public JObject[] Actions(JObject[] actions)
        {
            if (actions == null)
                return this.actions;
            this.actions = actions;

            if (this.actions != null)
            {
                for (int i = 0; i < this.actions.Length; i++)
                {
                 
                }
            }
            return null;
        }

        public string Content(string v)
        {
            if (v == null)
                return this.description;
            this.description = v;
            IosContent.Body = v;
            return null;
        }

        public string Icon(string v)
        {
            if (v == null)
                return this.icon;
            this.icon = v;
            if (icon == null || icon == "")
            {

            }
            else
            {
            }
            return null;
        }

        public string Color(string v)
        {
            if (v == null)
                return this.color;
            this.color = v;

            if (this.color != null && this.color != "")
            {
                if (this.color.StartsWith("#", StringComparison.Ordinal))
                {
                    var clr = this.color.Substring(1);
                    var R = clr.Substring(0, 2);
                    var G = clr.Substring(2, 2);
                    var B = clr.Substring(4, 2);

                }
            }
            return null;
        }
        public string Visibility(string v)
        {
            if (v == null)
                return this.visiblity;
            this.visiblity = v;

            if (this.visiblity == "public")
            {

            }
            return null;
        }
        /*
        public string BigIcon(string v)
        {
            if (v == null)
                return this.bigIcon;
            this.bigIcon = v;
            return null;
        }
        */
        public async Task<object> Show()
        {
            if(false) UNUserNotificationCenter.Current.GetDeliveredNotifications((UNNotification[] obj) =>
            {
                foreach(var notif in obj)
                {
                    if((notif.Request.Content.UserInfo["custom-payload-id"].ToString()) == this.ID.ToString())
                    {

                    }
                }
            });
            var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
            var request = UNNotificationRequest.FromIdentifier(this.ID.ToString(), IosContent, trigger);
            await UNUserNotificationCenter.Current.AddNotificationRequestAsync(request);
            return null;

        }

        public void Update()
        {
            this.Show();
        }

        public void Remove()
        {
            try
            {

            }catch(Exception)
            {

            }
        }

    }
    public class Notifications
    {
        internal XBridge Bridge;

        public Notifications(XBridge bridge)
        {
            this.Bridge = bridge;
            // Request notification permissions from the user
        }

        public Task<object> Init()
        {
            TaskCompletionSource<object> src = new TaskCompletionSource<object>();
            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {
                if (!approved)
                    src.SetException(new NSErrorException(err));
                else
                    src.SetResult(true);
            });
            return src.Task;
        }

        public Notification Create()
        {
            return new Notification(this);
        }

        public void RemoveAll() {

        }
    }


}
