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
using Xam.Plugin.Abstractions;
using Xam.Plugin.Droid;
using Xamarin.Forms.Platform.Android.AppCompat;
using Android.Webkit;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.Droid
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Android.Webkit.WebView>
    {

        public static event EventHandler<Android.Webkit.WebView> OnControlChanging;
        public static event EventHandler<Android.Webkit.WebView> OnControlChanged;

        public static void Init()
        {
            var dt = DateTime.Now;
        }

    }
}