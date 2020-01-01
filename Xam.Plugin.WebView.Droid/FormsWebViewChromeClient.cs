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
using Xamarin.Forms.Platform.Android;

namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewChromeClient : WebChromeClient
    {
        private IValueCallback mUploadMessage;
        private static int FILECHOOSER_RESULTCODE = 1;
        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewChromeClient(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        /* 
         * below lines for fixing issue #66: Not support HTML Input File
         * reference this workaround: https://forums.xamarin.com/discussion/62284/input-type-file-doesnt-work-on-xamarin-forms-webview-androidhome.firefoxchina.cn
         */
        private void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == FILECHOOSER_RESULTCODE)
            {
                if (null == mUploadMessage)
                    return;
                mUploadMessage.OnReceiveValue(WebChromeClient.FileChooserParams.ParseResult((int)resultCode, data));
                mUploadMessage = null;
            }
        }
        
        [Android.Runtime.Register("onShowFileChooser", "(Landroid/webkit/WebView;Landroid/webkit/ValueCallback;Landroid/webkit/WebChromeClient$FileChooserParams;)Z", "GetOnShowFileChooser_Landroid_webkit_WebView_Landroid_webkit_ValueCallback_Landroid_webkit_WebChromeClient_FileChooserParams_Handler")]
        public override bool OnShowFileChooser(Android.Webkit.WebView webView, IValueCallback filePathCallback, FileChooserParams fileChooserParams)
        {
            var appActivity = Xamarin.Forms.Forms.Context as IMainActivityWithStarting;
            mUploadMessage = filePathCallback;
            Intent chooserIntent = fileChooserParams.CreateIntent();
            appActivity.StartActivity(chooserIntent, FILECHOOSER_RESULTCODE, OnActivityResult);
            //return base.OnShowFileChooser (webView, filePathCallback, fileChooserParams);
            return true;
        }
    }
}