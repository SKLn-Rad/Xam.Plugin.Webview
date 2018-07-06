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
using System.Collections.Concurrent;

namespace SampleApp.Droid
{
    [Activity(Label = "SampleApp", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IFormsWebViewMainActivity
    {
        readonly ConcurrentDictionary<int, Action<Result, Intent>> _activityResultCallbacks = new ConcurrentDictionary<int, Action<Result, Intent>>();

        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            FormsWebViewRenderer.Initialize();

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }


        public void RegisterActivityResultCallback(int requestCode, Action<Result, Intent> callback)
        {
            _activityResultCallbacks.TryAdd(requestCode, callback);
        }

        public void UnregisterActivityResultCallback(int requestCode)
        {
            Action<Result, Intent> callback;
            _activityResultCallbacks.TryRemove(requestCode, out callback);

            callback = null;
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            Action<Result, Intent> callback;
            if (_activityResultCallbacks.TryGetValue(requestCode, out callback))
                callback(resultCode, data);
        }
    }
}

