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
    public interface IFormsWebViewMainActivity
    {
        void RegisterActivityResultCallback(int requestCode, Action<Result, Intent> callback);
        void UnregisterActivityResultCallback(int requestCode);
    }
}