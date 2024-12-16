using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Content;
using Android.App;

namespace xbridge.android.Modules
{



    public class ActivityResult
    {
        public Android.App.Result Code { get; set; }
        public Intent Data { get; set; }
    }
    public class Core : xbridge.Modules.Core
    {
        public Core(XBridge bridge) : base(bridge)
        {
            Adapter = (xbridge.android.XBridgeAndroidAdapter)bridge.Adapter;
        }


        internal void HandlePermissionResult(int requestCode, bool v)
        {
            permissionIndex.Finish(requestCode, v);
        }
        Dictionary<int, TaskCompletionSource<bool>> requests = new Dictionary<int, TaskCompletionSource<bool>>();

        internal Android.Net.Uri _ResourceUri(string url)
        {
            if (url.StartsWith("resource:", StringComparison.Ordinal))
                url = "file:///android_asset/" + url.Substring(9);
            else if (!url.StartsWith("file:///android_asset/", StringComparison.Ordinal))
                throw new Exception("not a resource url");
            return Android.Net.Uri.Parse(url);
        }
       

        static Dictionary<string, string> permissions = new Dictionary<string, string>()
        {
            { "read-file", Manifest.Permission.ReadExternalStorage},
            { "write-file", Manifest.Permission.WriteExternalStorage}
        };

        public override async Task<bool> GetPermission(string v)
        {
            if (!permissions.ContainsKey(v))
                throw new Exception("permission not implemented: " + v);
            if (ContextCompat.CheckSelfPermission(Adapter.Context, permissions[v]) == (int)Permission.Granted)
            {
                return true;
            }
            else
            {
                var awaitable = permissionIndex.Create();
                ActivityCompat.RequestPermissions(Adapter.Activity, new String[] { permissions[v] }, awaitable.ID);
                return (bool) (await awaitable.Task);
            }



        }

        public Task<ActivityResult> GetActivityResult(Intent intent) {
            var awaitable = activityIndex.Create();
            Adapter.Activity.StartActivityForResult(intent, awaitable.ID);
            return awaitable.Task;
        }

        private bool KeepingScreenOn = false;
        public async Task<bool> KeepScreenOn(bool? keep)
        {
            if (keep != null)
            {
                KeepingScreenOn = (bool)keep;
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                Adapter.Activity.RunOnUiThread(() => {
                    if (KeepingScreenOn)
                        Adapter.Activity.Window.AddFlags(Android.Views.WindowManagerFlags.KeepScreenOn);
                    else
                        Adapter.Activity.Window.ClearFlags(Android.Views.WindowManagerFlags.KeepScreenOn);
                    tcs.SetResult(KeepingScreenOn);
                });
                return await tcs.Task;
            }
            return KeepingScreenOn;

        }

        internal void HandleActivityResult(int requestCode, Android.App.Result resultCode, Intent data)
        {
                activityIndex.Finish(requestCode, new ActivityResult { Code = resultCode, Data = data });

        }

        private XBridgeAndroidAdapter Adapter;
        private IndexedTasks<bool> permissionIndex = new IndexedTasks<bool>(65535);
        private IndexedTasks<ActivityResult> activityIndex = new IndexedTasks<ActivityResult>(65535);

        public override Stream OpenPackagedFile(string name)
        {
            throw new NotImplementedException();
        }

        public override void Terminate() {

            Stop();
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }

        public override string Os()
        {
            return "android";
        }

        public override void Stop()
        {
            Adapter.Activity.Finish();
        }

        public override string AppID()
        {
            return Adapter.Activity.PackageName;
        }
    }
}
