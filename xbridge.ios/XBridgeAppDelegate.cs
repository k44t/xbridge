using System;
using Foundation;
using UIKit;
using xbridge;
using xbridge.Modules;
using WebKit;

// how to get the instance?
namespace xbridge.ios
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    public abstract class XBridgeAppDelegate : UIApplicationDelegate
    {
        protected XBridge bridge;

        // class-level declarations

        public XBridgeAppDelegate()
        {
            //this.bridge = XBridgeIOSAdapter.GetInstance().Bridge;
        }

        public override UIWindow Window
        {
            get;
            set;
        }

        public abstract void Start();

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Start();
            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
            bridge.GetModule<Events>()?.Trigger("resignActivation", null);
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
            bridge.GetModule<Events>()?.Trigger("didEnterBackground", null);
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
            bridge.GetModule<Events>()?.Trigger("willEnterForeground", null);
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
            bridge.GetModule<Events>()?.Trigger("activated", null);
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
            bridge.GetModule<Events>()?.Trigger("willTerminate", null);
        }
    }
}

