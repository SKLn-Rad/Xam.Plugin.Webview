using Android.OS;
using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xam.Plugin.WebView.Abstractions;
using Xam.Plugin.WebView.Abstractions.Delegates;
using Xam.Plugin.WebView.Abstractions.Enumerations;
using Xam.Plugin.WebView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WebViewEx>
    {

        public static string MimeType = "text/html";

        public static string EncodingType = "UTF-8";

        public static string HistoryUri = "";

        public static string BaseUrl { get; set; } = "file:///android_asset/";

        public static bool IgnoreSSLGlobally { get; set; }

        public static event EventHandler<Android.Webkit.WebView> OnControlChanged;

        public static void Initialize()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if ((Control == null || Control.Disposed) && Element != null)
                SetupControl();

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);

            if (Element.UseWideViewPort)
            {
                Control.Settings.LoadWithOverviewMode = true;
                Control.Settings.UseWideViewPort = true;
            }
        }
        void SetupElement(FormsWebView element)
        {
            element.PropertyChanged += OnPropertyChanged;
            element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequest;
            element.OnGetCookieRequestedAsync += OnGetCookieRequestAsync;
            element.OnGetAllCookiesRequestedAsync += OnGetAllCookieRequestAsync;
            element.OnSetCookieRequestedAsync += OnSetCookieRequestAsync;
            element.OnClearCookiesRequested += OnClearCookiesRequest;
            element.OnBackRequested += OnBackRequested;
            element.OnForwardRequested += OnForwardRequested;
            element.OnRefreshRequested += OnRefreshRequested;
            element.OnNavigationStarted += SetCurrentUrl;
            element.OnNavigationCompleted += OnNavigationCompleted;

            SetSource();
        }

        void DestroyElement(FormsWebView element)
        {
            element.PropertyChanged -= OnPropertyChanged;
            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequest;
            element.OnClearCookiesRequested -= OnClearCookiesRequest;
            element.OnGetAllCookiesRequestedAsync -= OnGetAllCookieRequestAsync;
            element.OnGetCookieRequestedAsync -= OnGetCookieRequestAsync;
            element.OnSetCookieRequestedAsync -= OnSetCookieRequestAsync;
            element.OnBackRequested -= OnBackRequested;
            element.OnForwardRequested -= OnForwardRequested;
            element.OnRefreshRequested -= OnRefreshRequested;
            element.OnNavigationStarted -= SetCurrentUrl;
            element.OnNavigationCompleted -= OnNavigationCompleted;

            element.Dispose();
        }

        void SetupControl()
        {
            var webView = new WebViewEx(Forms.Context);

            // https://github.com/SKLn-Rad/Xam.Plugin.WebView.Webview/issues/11
            webView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

            // Defaults
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.DomStorageEnabled = true;
            webView.AddJavascriptInterface(new FormsWebViewBridge(this), "bridge");
            webView.SetWebViewClient(new FormsWebViewClient(this));
            webView.SetWebChromeClient(new FormsWebViewChromeClient(this));
            webView.SetBackgroundColor(Android.Graphics.Color.Transparent);

            FormsWebView.CallbackAdded += OnCallbackAdded;
            SetNativeControl(webView);
            OnControlChanged?.Invoke(this, webView);
        }

        async void OnCallbackAdded(object sender, string e)
        {
            if (Element == null || string.IsNullOrWhiteSpace(e)) return;

            if ((sender == null && Element.EnableGlobalCallbacks) || sender != null)
                await OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(e));
        }

        void OnForwardRequested(object sender, EventArgs e)
        {
            if (Control == null || Control.Disposed) return;

            if (Control.CanGoForward())
                Control.GoForward();
        }

        void OnBackRequested(object sender, EventArgs e)
        {
            if (Control == null || Control.Disposed) return;

            if (Control.CanGoBack())
                Control.GoBack();
        }

        void OnRefreshRequested(object sender, EventArgs e)
        {
            if (Control == null || Control.Disposed) return;

            Control.Reload();
        }

        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Source":
                    SetSource();
                    break;
            }
        }

        private async Task OnClearCookiesRequest()
        {
            if (Control == null || Control.Disposed) return;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
            {
                CookieManager.Instance.RemoveAllCookies(null);
                CookieManager.Instance.Flush();
            }
            else
            {
                //CookieSyncManager cookieSyncMngr = CookieSyncManager.createInstance(context);
                CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                cookieSyncMngr.StartSync();
                CookieManager cookieManager = CookieManager.Instance;
                cookieManager.RemoveAllCookie();
                cookieManager.RemoveSessionCookie();
                cookieSyncMngr.StopSync();
                cookieSyncMngr.Sync();
            }
        }


        private async Task<string> OnGetAllCookieRequestAsync()
        {
            if (Control == null || Element == null || Control.Disposed) return string.Empty;
            var cookies = string.Empty;

            if (Control != null && Element != null && !Control.Disposed)
            {
                string url = string.Empty;
                try
                {
                    url = Control.Url;
                }
                catch (Exception e)
                {
                    url = Element.BaseUrl;
                }
                if (string.IsNullOrEmpty(url))
                    return string.Empty;

                if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                {
                    CookieManager.Instance.Flush();
                    cookies = CookieManager.Instance.GetCookie(url);
                }
                else
                {
                    CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                    cookieSyncMngr.StartSync();
                    CookieManager cookieManager = CookieManager.Instance;
                    cookies = cookieManager.GetCookie(url);
                }
            }

            return cookies;
        }

        private async Task<string> OnSetCookieRequestAsync(Cookie cookie)
        {
            if (Control != null && Element != null && !Control.Disposed)
            {
                var url = new Uri(Control.Url).Host;
                if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                {

                    CookieManager.Instance.SetCookie(url, cookie.ToString());
                    CookieManager.Instance.Flush();
                }
                else
                {
                    CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                    cookieSyncMngr.StartSync();
                    CookieManager cookieManager = CookieManager.Instance;
                    cookieManager.SetCookie(url, cookie.ToString());
                    cookieManager.Flush();
                }
            }

            var toReturn = await OnGetCookieRequestAsync(cookie.Name);

            return toReturn;
        }



        private async Task<string> OnGetCookieRequestAsync(string key)
        {

            var cookie = default(string);

            if (Control != null && Element != null && !Control.Disposed)
            {
                string url = string.Empty;
                try
                {
                    url = Control.Url;
                }
                catch (Exception e)
                {
                    url = Element.BaseUrl;
                }
                string cookieCollectionString;
                string[] cookieCollection;

                try
                {
                    if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                    {
                        CookieManager.Instance.Flush();
                        cookieCollectionString = CookieManager.Instance.GetCookie(url);

                    }
                    else
                    {
                        CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                        cookieSyncMngr.StartSync();
                        CookieManager cookieManager = CookieManager.Instance;
                        cookieCollectionString = cookieManager.GetCookie(url);
                    }
                    cookieCollection = cookieCollectionString.Split(new string[] { "; " }, StringSplitOptions.None);

                    foreach (var c in cookieCollection)
                    {
                        var keyValue = c.Split(new[] { '=' }, 2);
                        if (keyValue.Length > 1 && keyValue[0] == key)
                        {
                            cookie = keyValue[1];
                            break;
                        }
                    }
                }
                catch (Exception e) { }
            }


            return cookie;
        }


        internal async Task<string> OnJavascriptInjectionRequest(string js)
        {
            if (Element == null || Control == null || Control.Disposed) return string.Empty;

            var callback = new JavascriptValueCallback(this);

            var response = string.Empty;

            Device.BeginInvokeOnMainThread(() => {
                if (Control == null || Control.Disposed)
                    return;
                Control.EvaluateJavascript(js, callback);
            });

            // wait!
            await Task.Run(() =>
            {
                while (callback.Value == null) { }

                using (callback)
                {
                    // Get the string and strip off the quotes
                    if (callback.Value is Java.Lang.String)
                    {
                        // Unescape that damn Unicode Java bull.
                        response = Regex.Replace(
                            callback.Value.ToString(),
                            @"\\[Uu]([0-9A-Fa-f]{4})",
                            m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
                        response = Regex.Unescape(response);

                        if (response.Equals("\"null\""))
                            response = null;

                        else if (response.StartsWith("\"") && response.EndsWith("\""))
                            response = response.Substring(1, response.Length - 2);
                    }
                }
            });

            // return
            return response;
        }

        internal void SetSource()
        {
            if (Element == null || Control == null || Control.Disposed || string.IsNullOrWhiteSpace(Element.Source)) return;

            switch (Element.ContentType)
            {
                case WebViewContentType.Internet:
                    LoadFromInternet();
                    break;

                case WebViewContentType.LocalFile:
                    LoadFromFile();
                    break;

                case WebViewContentType.StringData:
                    LoadFromString();
                    break;
            }
        }

        void LoadFromString()
        {
            if (Element == null || Control == null || Control.Disposed || Element.Source == null) return;

            // Check cancellation
            var handler = Element.HandleNavigationStartRequest(Element.Source);
            if (handler.Cancel) return;

            // Load
            Control.LoadDataWithBaseURL(Element.BaseUrl ?? BaseUrl, Element.Source, MimeType, EncodingType, HistoryUri);
        }

        void LoadFromFile()
        {
            if (Element == null || Control == null || Control.Disposed || Element.Source == null) return;

            Control.LoadUrl(Path.Combine(Element.BaseUrl ?? BaseUrl, Element.Source));
        }

        void LoadFromInternet()
        {
            if (Element == null || Control == null || Control.Disposed || Element.Source == null) return;

            var headers = new Dictionary<string, string>();

            // Add Local Headers
            foreach (var header in Element.LocalRegisteredHeaders)
            {
                if (!headers.ContainsKey(header.Key))
                    headers.Add(header.Key, header.Value);
            }

            // Add Global Headers
            if (Element.EnableGlobalHeaders)
            {
                foreach (var header in FormsWebView.GlobalRegisteredHeaders)
                {
                    if (!headers.ContainsKey(header.Key))
                        headers.Add(header.Key, header.Value);
                }
            }

            Control.LoadUrl(Element.Source, headers);
        }


        private void SetCurrentUrl(object sender, DecisionHandlerDelegate e)
        {
            if (Element == null || Control == null || Control.Disposed) return;

            Device.BeginInvokeOnMainThread(() =>
            {
                if (Element == null || Control == null || Control.Disposed) return;

                Element.CurrentUrl = Control.Url;
            });
        }

        private void OnNavigationCompleted(object sender, string e)
        {
            if (Element == null || Control == null || Control.Disposed) return;

            Device.BeginInvokeOnMainThread(() =>
            {
                if (Element == null || Control == null || Control.Disposed) return;

                Element.CurrentUrl = Control.Url;
            });
        }
    }
}