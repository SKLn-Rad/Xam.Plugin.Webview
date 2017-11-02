using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Webkit;
using Xam.Plugin.WebView.Droid;

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

            FormsWebViewRenderer.OnUrlLoading = ShouldOverrideUrlLoading;
            FormsWebViewRenderer.Initialize();

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        public bool ShouldOverrideUrlLoading(WebView webView, string url)
        {
            if (url.StartsWith("mailto:"))
            {
                webView.StopLoading();
                webView.GoBack();

                url = url.Replace("mailto:", "");

                Intent email = new Intent(Intent.ActionSendto);
                email.SetData(Android.Net.Uri.Parse("mailto:"));
                email.PutExtra(Intent.ExtraEmail, new String[] { url.Split('?')[0] });
                if (email.ResolveActivity(PackageManager) != null)
                    StartActivity(email);

                return true;
            }

            return false;
        }
    }
}

