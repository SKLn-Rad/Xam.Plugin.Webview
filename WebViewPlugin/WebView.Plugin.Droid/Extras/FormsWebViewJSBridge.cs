using System;
using Java.Interop;
using Android.Webkit;

namespace Xam.Plugin.Droid.Extras
{
    public class FormsWebViewJsBridge : Java.Lang.Object
    {
        readonly WeakReference<FormsWebViewRenderer> Renderer;

        public FormsWebViewJsBridge(FormsWebViewRenderer renderer)
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