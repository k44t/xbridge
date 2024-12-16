using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace xbridge.android.Controllers
{
    public class Files: xbridge.Modules.Files
    {

        public Files(XBridge bridge) : base(bridge)
        {

        }



        public override string DataDir()
        {
            return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/";
        }

        public override string SharedDir()
        {
            return Android.OS.Environment.ExternalStorageDirectory.ToString() + "/";
        }

    }
}
