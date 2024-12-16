using System;
using System.Security.Cryptography;

namespace xbridge.Modules
{
    public class RSA
    {
        private readonly XBridge Bridge;
        private string Key;

        public RSA(XBridge bridge)
        {
            this.Bridge = bridge;
        }

        public void PublicKey(string xmlKey)
        {
            this.Key = xmlKey;
        }

        public bool Verify(string message, string signature, string alg, string pubKey)
        {
            if (pubKey == null)
                pubKey = Key;
            if (pubKey == null)
                throw new Exception("no public key given to verify signature");
            if (alg == null)
                alg = "SHA256";
            var enc = System.Text.Encoding.UTF8;
            var bytes = enc.GetBytes(message);
            var sigB = Convert.FromBase64String(signature);


            var pro = new RSACryptoServiceProvider();
            pro.FromXmlString(pubKey);

            //Adding public key to RSACryptoServiceProvider object.

            //pro.FromXmlString(pub);

            //Reading the Signature to verify.

            //Reading the Signed File for Verification.


            //FileStream Verifyfile = new FileStream(txtVerifyFile.Text, FileMode.Open, FileAccess.Read);

            //BinaryReader VerifyFileReader = new BinaryReader(Verifyfile);

            //byte[] VerifyFileData = VerifyFileReader.ReadBytes((int)Verifyfile.Length);

            //Comparing.
            //pro.VerifyData()
            return pro.VerifyData(bytes, alg, sigB);
        }
    }
}
