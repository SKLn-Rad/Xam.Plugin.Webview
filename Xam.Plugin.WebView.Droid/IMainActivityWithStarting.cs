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

namespace Xam.Plugin.WebView.Droid
{
    public interface IMainActivityWithStarting
    {
        void StartActivity(Intent intent, int requestCode, Action<int, Result, Intent> resultCallback);
    }
}