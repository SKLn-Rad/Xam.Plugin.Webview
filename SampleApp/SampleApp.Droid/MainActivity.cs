using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xam.Plugin.Droid;

namespace SampleApp.Droid
{
    [Activity(Label = "SampleApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FormsWebViewRenderer.Init();
            FormsWebViewRenderer.OnControlChanging += (sender, element, control) =>
            {
                var webView = control as Android.Webkit.WebView;
                webView.SetLayerType(LayerType.Software, null);
                webView.Settings.LoadWithOverviewMode = true;
                webView.Settings.UseWideViewPort = true;
            };

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }
    }
}

