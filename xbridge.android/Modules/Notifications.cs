using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Newtonsoft.Json.Linq;
using xbridge.android;

namespace xbridge.android.Modules
{
    public class NotificationReceiver
    {

    }

    public class Notification : XBridgeSharedObject
    {
        private XBridgeAndroidAdapter adapter;
        private Notifications notifications;
        private string title = "title";
        private string description = "description";
        private string icon = "icon";
        private Android.App.Notification notification;
        private JObject[] actions;
        private string bigIcon;
        private string color;
        private string visiblity;
        private NotificationCompat.Builder b;
        private PendingIntent actionIntent;

        public Notification(XBridge xbridge, Notifications n): base(xbridge)
        {
            this.adapter = (xbridge.android.XBridgeAndroidAdapter)xbridge.Adapter;
            this.notifications = n;


            this.b = new NotificationCompat.Builder(adapter.Activity, "notifications");

            var pkgName = adapter.Activity.PackageName;
            var intent = adapter.Activity.PackageManager.GetLaunchIntentForPackage(pkgName);
            var contentIntent = PendingIntent.GetActivity(adapter.Activity, this.ID, intent, PendingIntentFlags.UpdateCurrent);
            this.actionIntent = PendingIntent.GetActivity(adapter.Activity, this.ID, intent, PendingIntentFlags.UpdateCurrent);
            b.SetContentIntent(contentIntent);
            b.SetDeleteIntent(contentIntent);
            b.SetOngoing(false);
            b.SetAutoCancel(true);
            b.SetOnlyAlertOnce(true);
        }


        public override void Destroy()
        {
            if (this.notification != null)
            {
                notification.Dispose();
            }
            b.Dispose();
        }

        public string Title(string v)
        {
            if (v == null)
                return this.title;
            this.title = v;
            b.SetContentTitle(v);
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
                    var a = actions[i];
                    string iconName = a.Property("icon").Value.ToObject<string>();
                    var nameProp = a.Property("name");
                    string name = null;
                    if (nameProp != null)
                    {
                        name = nameProp.Value.ToObject<string>();
                    }
                    var iconId = notifications.resources[iconName];
                    b.AddAction(new NotificationCompat.Action(iconId, name, actionIntent));
                }
            }
            return null;
        }

        public string Content(string v)
        {
            if (v == null)
                return this.description;
            this.description = v;
            b.SetContentText(description.Replace('\n', ' '));
            return null;
        }

        public string Icon(string v)
        {
            if (v == null)
                return this.icon;
            this.icon = v;
            if (icon == null || icon == "")
                b.SetSmallIcon(0);
            else
                b.SetSmallIcon(notifications.resources[icon]);
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

                    b.SetColor(int.Parse(clr, System.Globalization.NumberStyles.HexNumber));
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
                b.SetVisibility(NotificationCompat.VisibilityPublic);
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

        public Android.App.Notification _GetNotification()
        {
            return this.notification;
        }

        public void Show()
        {


            //b.SetStyle(new NotificationCompat.BigTextStyle().BigText(description));
            /*
            if (this.bigIcon != "" && this.bigIcon != null)
            {
                /*
                if (this.bigIcon.StartsWith("data:image/png;base64,"))
                {
                    var result = Base64.Decode(this.bigIcon.Substring("data:image/png;base64,".Length), Base64Flags.UrlSafe);
                    var bitmap2 = BitmapFactory.DecodeByteArray(result, 0, result.Length);
                    var bitmap3 = Bitmap.CreateScaledBitmap(bitmap2, 144, 143, false);
                    b.SetLargeIcon(bitmap3);
                }
                var bitmap = BitmapFactory.DecodeResource(adapter.Context.Resources,
                  notifications.resources[this.bigIcon]);
                b.SetLargeIcon(bitmap);
            }*/
            if (this.notification != null)
            {
                notification.Dispose();
            }


            notification = b.Build();
            //notification.ContentView.
            notifications.manager.Notify((int)this.ID, notification);
        }

        public void Update()
        {
            this.Show();
        }

        public void Remove()
        {
            notifications.manager.Cancel((int)this.ID);
        }

    }
    public class Notifications
    {
        internal NotificationManager manager;
        private XBridge bridge;

        public Notifications(XBridge bridge)
        {
            this.bridge = bridge;
            manager = ((Android.App.NotificationManager)((XBridgeAndroidAdapter) bridge.Adapter).Activity.GetSystemService(Context.NotificationService));
            CreateNotificationChannel("notifications", "Notifications");
        }
        private void CreateNotificationChannel(string channelId, string channelName)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) { 
                var chan = new NotificationChannel(channelId,
                    channelName, NotificationManager.ImportanceNone);
                chan.LightColor = 1;
                chan.LockscreenVisibility = NotificationVisibility.Public;
                var service = ((XBridgeAndroidAdapter)bridge.Adapter).Activity.GetSystemService(Context.NotificationService) as NotificationManager;
                service.CreateNotificationChannel(chan);
            }
        }

        public Notification Create()
        {
            return new Notification(bridge, this);
        }

        internal Dictionary<string, int> resources = new Dictionary<string, int>();
        internal void RegisterResource(string name, int id)
        {
            this.resources[name] = id;
        }

        public void RemoveAll() {
            manager.CancelAll();
        }
    }


}