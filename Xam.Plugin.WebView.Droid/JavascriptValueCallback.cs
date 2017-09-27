using System;
using Android.Webkit;

namespace Xam.Plugin.WebView.Droid
{
    public class JavascriptValueCallback : Java.Lang.Object, IValueCallback
    {

        public Java.Lang.Object Value { get; private set; }

        readonly WeakReference<FormsWebViewRenderer> Reference;

        public JavascriptValueCallback(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public void OnReceiveValue(Java.Lang.Object value)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            Value = value;
        }

        public void Reset()
        {
            Value = null;
        }
    }
}