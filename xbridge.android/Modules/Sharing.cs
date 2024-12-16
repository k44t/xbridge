using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Java.IO;
using xbridge.Util;

namespace xbridge.android.Modules
{
    public class Sharing
    {
        private XBridge Bridge;
        private AppCompatActivity Activity;
        private string Package;

        public Sharing(XBridge bridge)
        {
            this.Bridge = bridge;
            this.Activity = (bridge.Adapter as XBridgeAndroidAdapter).Activity;
            this.Package = bridge.GetModule<Core>().AppID();
        }

        // file and title (for the dialog) are required
        public async Task<object> ShareFile(string file, string title, string subject, string text)
        {
            Intent intentShareFile = new Intent(Intent.ActionSend);
            var f = new File(file);
            if (f.Exists())
            {
                intentShareFile.SetType(MimeTypes.GetMimeType(file));
                var shareUri = FileProvider.GetUriForFile(Activity, Package + ".fileprovider", f);
                intentShareFile.PutExtra(Intent.ExtraStream, shareUri);

                if(subject != null)
                    intentShareFile.PutExtra(Intent.ExtraSubject,
                                    subject);
                if(text != null)
                    intentShareFile.PutExtra(Intent.ExtraText, text);

                var r = await Bridge.GetModule<Core>().GetActivityResult(Intent.CreateChooser(intentShareFile, subject));
                if (r.Code == Android.App.Result.Canceled)
                    return false;
                return true;
            }
            throw new System.IO.FileNotFoundException(file);
        }


        public Task<object> ReadString(string uri, string encoding)
        {
            return Task.Run(() =>
            {
                if (encoding == null)
                    encoding = "utf-8";
                var u = Android.Net.Uri.Parse(uri);
                var stream = Activity.ContentResolver.OpenInputStream(u);
                var reader = new System.IO.StreamReader(stream, System.Text.Encoding.GetEncoding(encoding));
                var result = reader.ReadToEnd();
                return result as object;
            });
        }

        public async Task<object> ReceiveFile(string mime, string title)
        {
            if (mime == null)
                mime = "*/*";
            if (title == null)
                title = "Open file";

            Intent i = new Intent(Intent.ActionGetContent);
            i.SetType(mime);
            i.AddCategory(Intent.CategoryOpenable);

            // special intent for Samsung file manager
            Intent sIntent = new Intent("com.sec.android.app.myfiles.PICK_DATA");
            // if you want any file type, you can skip next line 
            sIntent.PutExtra("CONTENT_TYPE", mime);

            sIntent.AddCategory(Intent.CategoryDefault);

            Intent chooserIntent;
            if (Activity.PackageManager.ResolveActivity(sIntent, 0) != null)
            {
                // it is device with Samsung file manager
                chooserIntent = Intent.CreateChooser(sIntent, title);
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, new Intent[] { i });
            }
            else
            {
                chooserIntent = Intent.CreateChooser(i, title);
            }

            var result = await Bridge.GetModule<Core>().GetActivityResult(chooserIntent);
            if (result.Code == 0)
                // user no select dang
                return null;
            var file = result.Data.DataString;
            return file;
        }

    }
}
