using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
namespace xbridge.macos.Modules
{
    public class Clipboard
    {
        public Clipboard(XBridge bridge)
        {

        }

        public async Task<String> Get(){
            string clipboardText = await Xamarin.Essentials.Clipboard.GetTextAsync();
            return clipboardText;
        }

        public void Set(String value){
            Xamarin.Essentials.Clipboard.SetText(value);
        }
    }
}
