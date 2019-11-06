using System;
using Android.App;
using Android.Content;
using Android.Webkit;

namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewChromeClient : WebChromeClient
    {
        readonly WeakReference<FormsWebViewRenderer> Reference;

        public static int FILECHOOSER_RESULTCODE = 42;

        Activity _activity;

        public FormsWebViewChromeClient(FormsWebViewRenderer renderer, Activity activity)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
            _activity = activity;
        }

        public override bool OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            ((IWebViewChromeClientActivity)_activity).UploadMessage = filePathCallback;
            Intent i = new Intent(Intent.ActionGetContent);
            i.AddCategory(Intent.CategoryOpenable);
            i.SetType("image/*");
            _activity.StartActivityForResult(Intent.CreateChooser(i, "File Chooser"), FILECHOOSER_RESULTCODE);

            return true;
        }
    }
}