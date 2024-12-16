using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace xbridge.Modules
{
    public class Free
    {
        private XBridge Bridge;
        private string Salt;

        public Free(XBridge bridge)
        {
            this.Bridge = bridge;
        }

        public bool IsEnabled()
        {

            var dir = Bridge.GetModule<Files>().DataDir();
            try
            {
                var li = File.ReadAllText(dir + "/free.txt");
                return Verify(li);
            }
            catch
            {
                return false;
            }
        }

        private bool Verify(string data)
        {
            //var dataAsBytes = Convert.FromBase64String(data);
            //var str = System.Text.Encoding.UTF8.GetString(dataAsBytes);
            var msg = data.Substring(0, data.Length - 4);
            var sumStart = data.Substring(data.Length - 4);
            var toSum = Salt + msg;
            var bytes = System.Text.Encoding.UTF8.GetBytes(toSum);
            var pro = new SHA256CryptoServiceProvider();
            var hash = pro.ComputeHash(bytes);
            var hashHex = ByteArrayToString(hash);
            var hash4 = hashHex.Substring(0, 4);
            return hash4 == sumStart;
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public void Enable(string data)
        {
            if (!Verify(data))
            {
                throw new Exception("verification failed");
            }
            var dir = Bridge.GetModule<Files>().DataDir();
            File.WriteAllText(dir + "/free.txt", data, System.Text.Encoding.UTF8);
            
        }

        internal void Init(string v)
        {
            this.Salt = v;
        }
    }
}
