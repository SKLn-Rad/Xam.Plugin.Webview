using System;
using AppKit;
using Foundation;
using Xam.Plugin.WebView.Abstractions;
using Xam.Plugin.WebView.MacOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace SampleApp.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow _window;
        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var rect = new CoreGraphics.CGRect(200, 1000, 1024, 768);
            _window = new NSWindow(rect, style, NSBackingStore.Buffered, false);
            _window.Title = "Xam.Plugin.WebView.FormsWebView";
            _window.TitleVisibility = NSWindowTitleVisibility.Visible;
        }

        public override NSWindow MainWindow => _window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            FormsWebViewRenderer.Initialize();
            Forms.Init();
            LoadApplication(new App());
            base.DidFinishLaunching(notification);
        }
    }
}
