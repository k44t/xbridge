using System;
using System.Reflection;
using AppKit;
using SuperSocket.SocketBase.Config;
using xbridge.macos.Modules;
using xbridge.websocket;

namespace xbridge.macos.sample
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            NSApplication.Main(args);
        }
    }
}
