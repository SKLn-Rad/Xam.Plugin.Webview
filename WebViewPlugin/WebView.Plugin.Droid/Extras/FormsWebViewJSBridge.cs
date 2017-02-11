using System;
using Java.Interop;
using Android.Webkit;

namespace Xam.Plugin.Droid.Extras
{
    public class FormsWebViewJSBridge : Java.Lang.Object
    {
        readonly WeakReference<FormsWebViewRenderer> Renderer;

        public FormsWebViewJSBridge(FormsWebViewRenderer renderer)
        {
            Renderer = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        [JavascriptInterface]
        [Export("invokeAction")]
        public void InvokeAction(string data)
        {
            FormsWebViewRenderer renderer;
            if (Renderer != null && Renderer.TryGetTarget(out renderer))
                renderer.OnScriptNotify(data);
        }
    }
}