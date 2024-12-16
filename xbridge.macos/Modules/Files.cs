using System;
using System.Threading;
using System.Threading.Tasks;
using AppKit;

namespace xbridge.macos.Modules
{
    public class Files : xbridge.Modules.Files
    {
        public Files(XBridge bridge) : base(bridge)
        {

        }

        public Task<object> Pick()
        {
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            var result = new Task(() => {
                var dlg = NSOpenPanel.OpenPanel;
                dlg.CanChooseFiles = true;
                dlg.CanChooseDirectories = false;
                //dlg.AllowedFileTypes = new string[] { "txt", "html", "md", "css" };

                if (dlg.RunModal() == 1)
                {
                    // Nab the first file
                    var url = dlg.Urls[0];

                    if (url != null)
                    {
                        tcs.SetResult(url.Path);
                        return;
                    }
                }
                tcs.SetResult(null);
            });
            result.RunSynchronously();
            return tcs.Task;
            
        }

        public string SaveAs()
        {
            var dlg = new NSSavePanel();

            if (dlg.RunModal() == 1)
            {
                // Nab the first file
                var url = dlg.Url;

                if (url != null)
                {
                    return url.Path;
                }
            }
            return null;

        }

    }
}
