using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Util;
using Android.Content.Res;
using xbridge.android.Modules;
using Android.Graphics;
using Android.Support.V7.App;
#if __ANDROID_21__
using Android.Support.V4.App;
#endif

namespace xbridge.android
{
    public abstract class XBridgeMainActivity : AppCompatActivity
#if __ANDROID_21__
    , ActivityCompat.IOnRequestPermissionsResultCallback
#endif
    {
        protected XBridgeAndroidAdapter adapter;
        protected XBridge xbridge;



        protected override void OnPause()
        {
            xbridge.GetModule<xbridge.Modules.Events>().Trigger("deactivate", null);
            base.OnPause();
        }
        protected override void OnStop()
        {
            xbridge.GetModule<xbridge.Modules.Events>().Trigger("hide", null);
            base.OnStop();
        }
        private bool first = true;

        public abstract WebView MainWebView { get; protected set; }

        protected override void OnPostResume()
        {
            if (!first)
            {
                xbridge.GetModule<xbridge.Modules.Events>().Trigger("activate", null);
            }
            first = false;
            base.OnPostResume();
        }

        protected override void OnRestart()
        {
            xbridge.GetModule<xbridge.Modules.Events>().Trigger("show", null);
            base.OnRestart();
        }

        protected override void OnDestroy()
        {
            xbridge.GetModule<xbridge.Modules.Events>().Trigger("stop", null);
            base.OnDestroy();
        }

#if __ANDROID_21__
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if(grantResults.Length != 1)
            {
                xbridge.GetModule<Core>().HandlePermissionResult(requestCode, false);
            }else if(grantResults[0] == Permission.Granted)
            {
                xbridge.GetModule<Core>().HandlePermissionResult(requestCode, true);
            }else
            {
                xbridge.GetModule<Core>().HandlePermissionResult(requestCode, true);
            }
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
#endif


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            xbridge.GetModule<Core>().HandleActivityResult(requestCode, resultCode, data);
            base.OnActivityResult(requestCode, resultCode, data);
        }


        public override void OnBackPressed()
        {
            if (MainWebView.CanGoBack())
            {
                MainWebView.GoBack();
            }
            else {
                base.OnBackPressed();
            }
        }
    }
}
