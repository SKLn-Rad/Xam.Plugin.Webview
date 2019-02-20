using Android.Content;
using Android.OS;
using Android.Webkit;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xam.Plugin.WebView.Abstractions;
using Xam.Plugin.WebView.Abstractions.Enumerations;
using Xam.Plugin.WebView.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Android.Webkit.WebView>
    {

        public static string MimeType = "text/html";

        public static string EncodingType = "UTF-8";

        public static string HistoryUri = "";

        public static string BaseUrl { get; set; } = "file:///android_asset/";

        public static bool IgnoreSSLGlobally { get; set; }

        public static event EventHandler<Android.Webkit.WebView> OnControlChanged;

        JavascriptValueCallback _callback;

        public FormsWebViewRenderer(Context context) : base(context)
        {
        }

        public static void Initialize()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && Element != null)
                SetupControl();

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);

            if (Element.UseWideViewPort) {
                Control.Settings.LoadWithOverviewMode = true;
                Control.Settings.UseWideViewPort = true;
            }
        }
        void SetupElement(FormsWebView element)
        {
            element.PropertyChanged += OnPropertyChanged;
            element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequest;
            element.OnClearCookiesRequested += OnClearCookiesRequest;
            element.OnAddCookieRequested += OnAddCookieRequested;
            element.OnBackRequested += OnBackRequested;
            element.OnForwardRequested += OnForwardRequested;
            element.OnRefreshRequested += OnRefreshRequested;
            element.OnPrintCookiesRequested += OnPrintCookiesRequested;

            SetSource();
        }


        void DestroyElement(FormsWebView element)
        {
            element.PropertyChanged -= OnPropertyChanged;
            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequest;
            element.OnClearCookiesRequested -= OnClearCookiesRequest;
            element.OnAddCookieRequested -= OnAddCookieRequested;
            element.OnBackRequested -= OnBackRequested;
            element.OnForwardRequested -= OnForwardRequested;
            element.OnRefreshRequested -= OnRefreshRequested;
            element.OnPrintCookiesRequested -= OnPrintCookiesRequested;

            element.Dispose();
        }

        void SetupControl()
        {
            var webView = new Android.Webkit.WebView(Forms.Context);
            _callback = new JavascriptValueCallback(this);

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
            if (Control == null) return;

            if (Control.CanGoForward())
                Control.GoForward();
        }

        void OnBackRequested(object sender, EventArgs e)
        {
            if (Control == null) return;

            if (Control.CanGoBack())
                Control.GoBack();
        }

        void OnRefreshRequested(object sender, EventArgs e)
        {
            if (Control == null) return;

            Control.Reload();
        }

        void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "Source":
                    SetSource();
                    break;
            }
        }

        private Task OnClearCookiesRequest()
        {
            if (Control == null) return Task.CompletedTask;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1) {
                CookieManager.Instance.RemoveAllCookies(null);
                CookieManager.Instance.Flush();
            } else {
#pragma warning disable CS0618
                //CookieSyncManager cookieSyncMngr = CookieSyncManager.createInstance(context);
                CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                cookieSyncMngr.StartSync();
                CookieManager cookieManager = CookieManager.Instance;
                cookieManager.RemoveAllCookie();
                cookieManager.RemoveSessionCookie();
                cookieSyncMngr.StopSync();
                cookieSyncMngr.Sync();
#pragma warning restore CS0618
            }
            return Task.CompletedTask;
        }

        private Task OnAddCookieRequested(System.Net.Cookie cookie)
        {
            if (Control == null || cookie == null || String.IsNullOrEmpty(cookie.Domain) || String.IsNullOrEmpty(cookie.Name))
                return Task.CompletedTask;

            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1) {
                SetCookie(cookie);
                CookieManager.Instance.Flush();
            } else {
#pragma warning disable CS0618
                CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                cookieSyncMngr.StartSync();
                SetCookie(cookie);
                cookieSyncMngr.StopSync();
                cookieSyncMngr.Sync();
#pragma warning restore CS0618
            }
            return Task.CompletedTask;
        }

        private Task OnPrintCookiesRequested(IEnumerable<string> urls = null)
        {
            if (Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1) {
                CookieManager.Instance.Flush();
            } else {
#pragma warning disable CS0618
                CookieSyncManager cookieSyncMngr = CookieSyncManager.CreateInstance(Context);
                cookieSyncMngr.Sync();
#pragma warning restore CS0618
            }
            if (!CookieManager.Instance.HasCookies)
                return Task.CompletedTask;
            if (urls == null)
                System.Diagnostics.Debug.WriteLine("Android must be given the cookie urls to iterate over");
            foreach (var url in urls) {
                var cookie = CookieManager.Instance.GetCookie(url);
                System.Diagnostics.Debug.WriteLine($"Cookie for {url}: {cookie}");
            }
            return Task.CompletedTask;
        }

        private void SetCookie(System.Net.Cookie cookie)
        {
            var cookieDomain = cookie.Domain;
            var url = $"{cookieDomain}";
            var cookieString = $"{cookie.ToString()}; Domain={cookieDomain}; Path={cookie.Path}";

            CookieManager cookieManager = CookieManager.Instance;
            CookieManager.Instance.SetAcceptCookie(true);
            CookieManager.Instance.SetCookie(url, cookieString);
        }

        internal async Task<string> OnJavascriptInjectionRequest(string js)
        {
            if (Element == null || Control == null) return string.Empty;

            // fire!
            _callback.Reset();

            var response = string.Empty;

            Device.BeginInvokeOnMainThread(() => Control.EvaluateJavascript(js, _callback));

            // wait!
            await Task.Run(() => {
                while (_callback.Value == null) { }

                // Get the string and strip off the quotes
                if (_callback.Value is Java.Lang.String) {
                    // Unescape that damn Unicode Java bull.
                    response = Regex.Replace(_callback.Value.ToString(), @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)));
                    response = Regex.Unescape(response);

                    if (response.Equals("\"null\""))
                        response = null;

                    else if (response.StartsWith("\"") && response.EndsWith("\""))
                        response = response.Substring(1, response.Length - 2);
                }

            });

            // return
            return response;
        }

        internal void SetSource()
        {
            if (Element == null || Control == null || string.IsNullOrWhiteSpace(Element.Source)) return;

            switch (Element.ContentType) {
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
            if (Element == null || Control == null || Element.Source == null) return;

            // Check cancellation
            var handler = Element.HandleNavigationStartRequest(Element.Source);
            if (handler.Cancel) return;

            // Load
            Control.LoadDataWithBaseURL(Element.BaseUrl ?? BaseUrl, Element.Source, MimeType, EncodingType, HistoryUri);
        }

        void LoadFromFile()
        {
            if (Element == null || Control == null || Element.Source == null) return;

            Control.LoadUrl(Path.Combine(Element.BaseUrl ?? BaseUrl, Element.Source));
        }

        void LoadFromInternet()
        {
            if (Element == null || Control == null || Element.Source == null) return;

            var headers = new Dictionary<string, string>();

            // Add Local Headers
            foreach (var header in Element.LocalRegisteredHeaders) {
                if (!headers.ContainsKey(header.Key))
                    headers.Add(header.Key, header.Value);
            }

            // Add Global Headers
            if (Element.EnableGlobalHeaders) {
                foreach (var header in FormsWebView.GlobalRegisteredHeaders) {
                    if (!headers.ContainsKey(header.Key))
                        headers.Add(header.Key, header.Value);
                }
            }

            Control.LoadUrl(Element.Source, headers);
        }
    }
}