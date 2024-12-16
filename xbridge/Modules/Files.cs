using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace xbridge.Modules
{
    public class Files
    {
        protected XBridge bridge;
        public Files(XBridge bridge)
        {
            this.bridge = bridge;
        }


        public async Task<object> List(string path)
        {
            if (!await bridge.GetModule<Core>().GetPermission("read-file"))
                throw new Exception("no permission given by user");
            path = Path.GetFullPath(path);
            //var x = new string[] { path + "/..", path + "/." };
            var y = Directory.GetFileSystemEntries(path);

            for(var i = y.Length - 1; i >= 0; i--)
            {
                var e = y[i];
                var parts = e.Split('/');
                if ((File.GetAttributes(e) & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    y[i] = parts[parts.Length - 1] + "/";
                }
                else
                {
                    y[i] = parts[parts.Length - 1];
                }
            }

            return y;
        }


        public async Task<object> IsDir(string path)
        {
            if (!await bridge.GetModule<Core>().GetPermission("read-file"))
                throw new Exception("no permission given by user");
            return (File.GetAttributes(path) & FileAttributes.Directory)
                 == FileAttributes.Directory;
        }

        public void MakeDir(string path)
        {
            Directory.CreateDirectory(path);
        }

        public async Task<object> Read(string path)
        {
            if (!await bridge.GetModule<Core>().GetPermission("read-file"))
                throw new Exception("no permission given by user");
            String result = File.ReadAllText(path);
            return result;
        }

        public async Task<object> Exists(string path)
        {
            if (!await bridge.GetModule<Core>().GetPermission("read-file"))
                throw new Exception("no permission given by user");
            var result = Directory.Exists(path) || File.Exists(path);
            return result;
        }

        public async Task<object> Write(string path, string text)
        {
            if (!await bridge.GetModule<Core>().GetPermission("write-file"))
                throw new Exception("no permission given by user");
            File.WriteAllText(path, text);
            return null;
        }

        public async Task<object> IsLink(string path)
        {
            if (!await bridge.GetModule<Core>().GetPermission("read-file"))
                throw new Exception("no permission given by user");
            return (File.GetAttributes(path) & FileAttributes.ReparsePoint)
                 == FileAttributes.ReparsePoint;
        }

        //the directory where your documents and downloads are, on iOS this would be the iCloud root directory
        public virtual string SharedDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        // the directiory where the app may store data and configuration
        public virtual string DataDir()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
    }
}
