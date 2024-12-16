using System;
using Android.Content;
using Android.OS;
namespace xbridge.android.Modules
{
    public class Vibration
    {
        Vibrator v;
        public Vibration(XBridge bridge)
        {
            v = (Vibrator)(bridge.Adapter as XBridgeAndroidAdapter).Activity.GetSystemService(Context.VibratorService);
        }

        public void Vibrate(Int64 milliseconds) {

            // Vibrate for 500 milliseconds
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                v.Vibrate(VibrationEffect.CreateOneShot(milliseconds, VibrationEffect.DefaultAmplitude));
            }
            else
            {
                //deprecated in API 26 
                v.Vibrate(milliseconds);
            }
        }
    }
}
