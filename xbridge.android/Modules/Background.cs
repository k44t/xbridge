using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Java.IO;
using NLog;
using xbridge.Modules;

namespace xbridge.android.Modules
{

    class BSBinder : Binder
    {
        internal AndroidBackgroundService Service;

        public BSBinder(AndroidBackgroundService bS)
        {
            this.Service = bS;
        }
    }

    class BSActionConnection : Java.Lang.Object, IServiceConnection
    {
        Action<AndroidBackgroundService> Action;
        public BSActionConnection(Action<AndroidBackgroundService> a)
        {
            this.Action = a;
        }
        public void OnServiceConnected(ComponentName name, IBinder binder)
        {
            Action.Invoke((binder as BSBinder).Service);
        }

        public void OnServiceDisconnected(ComponentName name)
        {

        }
    }

    [Service]
    class AndroidBackgroundService : Service
    {
        private PowerManager.WakeLock wakeLock;

        public override IBinder OnBind(Intent intent)
        {
            return new BSBinder(this);
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            if (Background != null)
                Background.Trigger("taskRemoved");
        }

        internal Background Background;
    }



    public class Background
    {
        private Logger log;
        private XBridge bridge;
        private Activity context;
        private PowerManager.WakeLock wakeLock;
        bool enabled = false;
        private AndroidBackgroundService Service;
        private BSActionConnection Connection;

        public Background(XBridge bridge)
        {
            log = LogManager.GetCurrentClassLogger();
            this.bridge = bridge;
            this.context = (bridge.Adapter as XBridgeAndroidAdapter).Activity;
        }

        // returns false if the service is already running, returns true if it started and bound the serivce successfully. throws an exception if anything goes wrong.
        public Task<object> Enable(Notification notification)
        {
            var tcs = new TaskCompletionSource<object>();
            try
            {

            lock (this)
            {
                if (!enabled)
                {
                    enabled = true;
                    var intent = new Intent(context, typeof(AndroidBackgroundService));
                    this.Connection = new BSActionConnection((AndroidBackgroundService Srv) =>
                    {
                        try
                        {

                            if (enabled)
                            {
                                Srv.StartForeground(notification.ID, notification._GetNotification());
                                Service = Srv;
                                Service.Background = this;
                                if (wakeLock == null)
                                {
                                    PowerManager pm = (PowerManager)
                                            Srv.GetSystemService(Context.PowerService);

                                    wakeLock = pm.NewWakeLock(
                                            WakeLockFlags.Partial, "Background");
                                    wakeLock.Acquire();
                                    tcs.SetResult(true);
                                    return;
                                }
                            }
                        }catch(Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                    });
                    context.BindService(
                        intent,
                        this.Connection,
                        Bind.AutoCreate);
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        context.StartForegroundService(intent);
                    }
                    else
                    {
                        context.StartService(intent);
                    }

                }
                else
                {
                    tcs.SetResult(false);
                }
                }
            }catch(Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }



        public void Disable()
        {
            lock (this)
                if (enabled)
                {
                    enabled = false;
                    var intent = new Intent(context, typeof(AndroidBackgroundService));
                    try
                    {
                        context.UnbindService(Connection);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                    try
                    {
                        if (Service != null)
                        {
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                            {
                                Service.StopForeground(StopForegroundFlags.Detach);
                            }
                            Service = null;
                        }
                        context.StopService(intent);
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                    }
                    if (wakeLock != null)
                    {
                        wakeLock.Release();
                        wakeLock = null;
                    }
                }
        }

        internal void Trigger(string v)
        {
            bridge.GetModule<Events>().Trigger("background." + v, null);
        }
    }
}
