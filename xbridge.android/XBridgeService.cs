using System;
using Android.App;
using Android.Content;
using Android.OS;
using xbridge.android.Modules;

namespace xbridge.android
{
    internal class ServiceBinder : Binder
    {
        internal XBridgeAndroidService Service;

        public ServiceBinder(XBridgeAndroidService srv)
        {
            this.Service = srv;
        }
    }

    class Connection : Java.Lang.Object, IServiceConnection
    {

        private ServiceHandler serviceHandler;

        public Connection(ServiceHandler serviceHandler)
        {
            this.serviceHandler = serviceHandler;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = service as ServiceBinder;
            serviceHandler.Service = binder.Service;
            serviceHandler.TriggerStart();
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            serviceHandler.TriggerStop();
        }
    }
    public interface ISimpleAndroidService
    {
        XBridgeAndroidService Service { get; set; }
    }
    public class XBridgeAndroidService : Service
    {
        private ServiceBinder binder;

        public XBridgeAndroidService()
        {
            this.binder = new ServiceBinder(this);
        }
        public override IBinder OnBind(Intent intent)
        {
            return binder;
        }

        public override void OnTaskRemoved(Intent rootIntent)
        {
            base.OnTaskRemoved(rootIntent);
        }
    }

    public class ServiceHandler
    {
        internal ISimpleService srv;
        internal Activity context;
        private Type serviceType;
        internal IServiceConnection connection;

        internal XBridgeAndroidService Service { get; set; }

        public ServiceHandler(Type service, Activity context, ISimpleService v)
        {
            this.serviceType = service;
            this.context = context;
            this.connection = new Connection(this);
            this.srv = v;

            Intent intent = new Intent(context, service);
            context.BindService(intent, connection, Bind.AutoCreate);
            context.StartService(intent);
        }


        internal void TriggerStart()
        {
            if (srv is ISimpleAndroidService)
                ((ISimpleAndroidService)srv).Service = this.Service;
            srv.Start();
        }

        internal void TriggerStop()
        {
            srv.Stop();
        }

        public void Stop()
        {
            Intent intent = new Intent(context, this.serviceType);
            context.UnbindService(connection);
            context.StopService(intent);
        }

        internal static ServiceHandler Start(Type service, Activity context, ISimpleService srv)
        {
            return new ServiceHandler(service, context, srv);
        }
    }
}
