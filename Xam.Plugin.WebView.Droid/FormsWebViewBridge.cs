using Android.Webkit;
using Java.Interop;
using System;

namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewBridge : Java.Lang.Object
    {

        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewBridge(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleScriptReceived(data);
        }
    }
}