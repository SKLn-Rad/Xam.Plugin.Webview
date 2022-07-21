
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;

namespace Xam.Plugin.WebView.Droid
{
    public class WebViewEx : Android.Webkit.WebView
    {
        public bool Disposed { get; set; }
        protected WebViewEx(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WebViewEx(Context context) : base(context)
        {
        }

        public WebViewEx(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public WebViewEx(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public WebViewEx(Context context, IAttributeSet attrs, int defStyleAttr, bool privateBrowsing) : base(context, attrs, defStyleAttr, privateBrowsing)
        {
        }

        public WebViewEx(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        public override bool OnKeyPreIme(Keycode keyCode, KeyEvent e)
        {
            var inputMethodManager = (InputMethodManager)Context.GetSystemService(Context.InputMethodService);

            if (keyCode != Keycode.Back ||
                !inputMethodManager.IsAcceptingText)
            {
                return base.OnKeyPreIme(keyCode, e);
            }

            inputMethodManager.HideSoftInputFromWindow(WindowToken, HideSoftInputFlags.None);

            var activity = GetActivity();
            if (activity == null)
            {
                return false;
            }

            activity.Window.DecorView.ClearFocus();

            return true;
        }

        private Activity GetActivity()
        {
            var context = Context;
            while (context is ContextWrapper)
            {
                if (context is Activity)
                {
                    return (Activity)context;
                }

                context = ((ContextWrapper)context).BaseContext;
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }
}
