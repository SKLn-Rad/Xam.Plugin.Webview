using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewChromeClient : WebChromeClient
    {
        private static int FILECHOOSER_RESULTCODE = 1;
        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewChromeClient(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        // For Android < 5.0
        [Java.Interop.Export]
        public void openFileChooser(IValueCallback filePathCallback, string acceptType, string capture)
        {
            Intent chooserIntent = new Intent(Intent.ActionGetContent);
            chooserIntent.AddCategory(Intent.CategoryOpenable);
            chooserIntent.SetType("*/*");
            RegisterCustomFileUploadActivity(filePathCallback, chooserIntent);
        }

        // For Android > 5.0
        [Android.Runtime.Register("onShowFileChooser", "(Landroid/webkit/WebView;Landroid/webkit/ValueCallback;Landroid/webkit/WebChromeClient$FileChooserParams;)Z", "GetOnShowFileChooser_Landroid_webkit_WebView_Landroid_webkit_ValueCallback_Landroid_webkit_WebChromeClient_FileChooserParams_Handler")]
        public override bool OnShowFileChooser(global::Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            base.OnShowFileChooser(webView, filePathCallback, fileChooserParams);

            var chooserIntent = fileChooserParams.CreateIntent();
            RegisterCustomFileUploadActivity(filePathCallback, chooserIntent, fileChooserParams.Title);

            return true;
        }

        private void RegisterCustomFileUploadActivity(IValueCallback filePathCallback, Intent chooserIntent, string title = "File Chooser")
        {
            if (Forms.Context is IFormsWebViewMainActivity)
            {
                var appActivity = Forms.Context as IFormsWebViewMainActivity;

                Action<Result, Intent> callback = (resultCode, intentData) =>
                {
                    if (filePathCallback == null)
                        return;

                    var result = FileChooserParams.ParseResult((int)resultCode, intentData);
                    filePathCallback.OnReceiveValue(result);

                    appActivity.UnregisterActivityResultCallback(FILECHOOSER_RESULTCODE);
                };

                appActivity.RegisterActivityResultCallback(FILECHOOSER_RESULTCODE, callback);

                ((FormsAppCompatActivity)Forms.Context).StartActivityForResult(Intent.CreateChooser(chooserIntent, title), FILECHOOSER_RESULTCODE);
            }
        }
    }
}