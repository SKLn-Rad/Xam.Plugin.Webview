using System;
using Android.Webkit;

namespace Xam.Plugin.WebView.Droid
{
    public interface IWebViewChromeClientActivity
    {
        IValueCallback UploadMessage { get; set; }
    }
}
