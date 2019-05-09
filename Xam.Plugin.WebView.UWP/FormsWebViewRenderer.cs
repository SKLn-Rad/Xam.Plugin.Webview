using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Security.Cryptography.Certificates;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Xam.Plugin.WebView.Abstractions;
using Xam.Plugin.WebView.Abstractions.Delegates;
using Xam.Plugin.WebView.Abstractions.Enumerations;
using Xam.Plugin.WebView.UWP;
using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpMethod = Windows.Web.Http.HttpMethod;
using HttpRequestMessage = Windows.Web.Http.HttpRequestMessage;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.WebView.UWP
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Windows.UI.Xaml.Controls.WebView>
    {

        public static event EventHandler<Windows.UI.Xaml.Controls.WebView> OnControlChanged;
       
        public static string BaseUrl { get; set; } = "ms-appx:///";
        LocalFileStreamResolver _resolver;

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
                SetupNewElement(e.NewElement);

            if (e.OldElement != null)
                DestroyOldElement(e.OldElement);
        }

        void SetupNewElement(FormsWebView element)
        {
            element.PropertyChanged += OnWebViewPropertyChanged;
            element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequestAsync;
            element.OnClearCookiesRequested += OnClearCookiesRequest;
            element.OnGetAllCookiesRequestedAsync += OnGetAllCookieRequestAsync;
            element.OnGetCookieRequestedAsync += OnGetCookieRequestAsync;
            element.OnSetCookieRequestedAsync += OnSetCookieRequestAsync;
            element.OnBackRequested += OnBackRequested;
            element.OnForwardRequested += OnForwardRequested;
            element.OnRefreshRequested += OnRefreshRequested;

            SetSource();
        }

        void DestroyOldElement(FormsWebView element)
        {
            element.PropertyChanged -= OnWebViewPropertyChanged;
            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequestAsync;
            element.OnClearCookiesRequested -= OnClearCookiesRequest;
            element.OnBackRequested -= OnBackRequested;
            element.OnGetAllCookiesRequestedAsync -= OnGetAllCookieRequestAsync;
            element.OnGetCookieRequestedAsync -= OnGetCookieRequestAsync;
            element.OnSetCookieRequestedAsync -= OnSetCookieRequestAsync;
            element.OnForwardRequested -= OnForwardRequested;
            element.OnRefreshRequested -= OnRefreshRequested;

            element.Dispose();
        }

        void SetupControl()
        {
            var control = new Windows.UI.Xaml.Controls.WebView();
            _resolver = new LocalFileStreamResolver(this);

            SetNativeControl(control);

            FormsWebView.CallbackAdded += OnCallbackAdded;
            Control.NavigationStarting += OnNavigationStarting;
            Control.NavigationCompleted += OnNavigationCompleted;
            Control.DOMContentLoaded += OnDOMContentLoaded;
            Control.ScriptNotify += OnScriptNotify;
            Control.LoadCompleted += SetCurrentUrl;
            Control.DefaultBackgroundColor = Windows.UI.Colors.Transparent;

            OnControlChanged?.Invoke(this, control);
        }

        void OnRefreshRequested(object sender, EventArgs e)
        {
            SetSource();
        }

        void OnForwardRequested(object sender, EventArgs e)
        {
            if (Control == null) return;

            if (Control.CanGoForward)
                Control.GoForward();
        }

        void OnBackRequested(object sender, EventArgs e)
        {
            if (Control == null) return;

            if (Control.CanGoBack)
                Control.GoBack();
        }

        void OnWebViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Source":
                    SetSource();
                    break;
            }
        }

        void OnNavigationStarting(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (Element == null) return;

            Element.Navigating = true;
            var handler = Element.HandleNavigationStartRequest(args.Uri != null ? args.Uri.AbsoluteUri : Element.Source);
            args.Cancel = handler.Cancel;
        }

        void OnNavigationCompleted(Windows.UI.Xaml.Controls.WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (Element == null) return;

            if (!args.IsSuccess)
                Element.HandleNavigationError((int)args.WebErrorStatus);

            Element.CanGoBack = Control.CanGoBack;
            Element.CanGoForward = Control.CanGoForward;

            Element.Navigating = false;
            Element.HandleNavigationCompleted(args.Uri.ToString());
        }

        async void OnDOMContentLoaded(Windows.UI.Xaml.Controls.WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (Element == null) return;

            // Add Injection Function
            await Control.InvokeScriptAsync("eval", new[] { FormsWebView.InjectedFunction });

            // Add Global Callbacks
            if (Element.EnableGlobalCallbacks)
                foreach (var callback in FormsWebView.GlobalRegisteredCallbacks)
                    await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(callback.Key) });

            // Add Local Callbacks
            foreach (var callback in Element.LocalRegisteredCallbacks)
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(callback.Key) });

            Element.HandleContentLoaded();
        }

        async void OnCallbackAdded(object sender, string e)
        {
            if (Element == null || string.IsNullOrWhiteSpace(e)) return;

            if ((sender == null && Element.EnableGlobalCallbacks) || sender != null)
                await OnJavascriptInjectionRequestAsync(FormsWebView.GenerateFunctionScript(e));
        }

        void OnScriptNotify(object sender, NotifyEventArgs e)
        {
            if (Element == null) return;
            Element.HandleScriptReceived(e.Value);
        }

        private async Task OnClearCookiesRequest()
        {
            if (Control == null) return;

   
            // This clears all tmp. data. Not only cookies
            await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
        }

        private async Task<string> OnGetAllCookieRequestAsync() {
            if (Control == null || Element == null) return string.Empty;
            var domain = (new Uri(Element.Source)).Host;
            var cookie = string.Empty;
            var url = new Uri(Element.Source);

            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            var cookieManager = filter.CookieManager;
            HttpCookieCollection cookieCollection = cookieManager.GetCookies(url);
            
            foreach (HttpCookie currentCookie in cookieCollection)
            {
                cookie += currentCookie.Name + "=" + currentCookie.Value + "; ";
            }

            if (cookie.Length > 2)
            {
                cookie = cookie.Remove(cookie.Length - 2);
            }
            return cookie;
        }

        private async Task<string> OnGetCookieRequestAsync(string key)
        {
            if (Control == null || Element == null) return string.Empty;
            var url = new Uri(Element.Source);
            var domain = (new Uri(Element.Source)).Host;
            var cookie = string.Empty;

            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            var cookieManager = filter.CookieManager;
            HttpCookieCollection cookieCollection = cookieManager.GetCookies(url);

            foreach (HttpCookie currentCookie in cookieCollection)
            {
                if (key == currentCookie.Name)
                {
                    cookie = currentCookie.Value;
                    break;
                }
            }
            
            return cookie;
        }

        private async Task<string> OnSetCookieRequestAsync(Cookie cookie)
        {
            if (Control == null || Element == null) return string.Empty;
            var url = new Uri(Element.Source);
            var newCookie = new HttpCookie(cookie.Name, cookie.Domain, cookie.Path);
            newCookie.Value = cookie.Value;
            newCookie.HttpOnly = cookie.HttpOnly;
            newCookie.Secure = cookie.Secure;
            newCookie.Expires = cookie.Expires;

            List<HttpCookie> cookieCollection = new List<HttpCookie>();
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            HttpClient httpClient;
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Untrusted);
            filter.IgnorableServerCertificateErrors.Add(ChainValidationResult.Expired);
            foreach (HttpCookie knownCookie in cookieCollection)
            {
                filter.CookieManager.SetCookie(knownCookie);
            }

            filter.CookieManager.SetCookie(newCookie);
            httpClient = new HttpClient(filter);

            return await OnGetCookieRequestAsync(cookie.Name);
           
        }

        async Task<string> OnJavascriptInjectionRequestAsync(string js)
        {
            if (Control == null) return string.Empty;
            var result = await Control.InvokeScriptAsync("eval", new[] { js });
            return result;
        }

        void SetSource()
        {
            if (Element == null || Control == null || string.IsNullOrWhiteSpace(Element.Source)) return;

            switch (Element.ContentType)
            {
                case WebViewContentType.Internet:
                    NavigateWithHttpRequest(new Uri(Element.Source));
                    break;
                case WebViewContentType.StringData:
                    LoadStringData(Element.Source);
                    break;
                case WebViewContentType.LocalFile:
                    LoadLocalFile(Element.Source);
                    break;
            }
        }

        void NavigateWithHttpRequest(Uri uri)
        {
            if (Element == null || Control == null) return;

            var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);

            // Add Local Headers
            foreach (var header in Element.LocalRegisteredHeaders)
            {
                if (!requestMsg.Headers.ContainsKey(header.Key))
                    requestMsg.Headers.Add(header.Key, header.Value);
            }

            // Add Global Headers
            if (Element.EnableGlobalHeaders)
            {
                foreach (var header in FormsWebView.GlobalRegisteredHeaders)
                {
                    if (!requestMsg.Headers.ContainsKey(header.Key))
                        requestMsg.Headers.Add(header.Key, header.Value);
                }
            }

            // Navigate
            Control.NavigateWithHttpRequestMessage(requestMsg);
        }

        void LoadLocalFile(string source)
        {
            Control.NavigateToLocalStreamUri(Control.BuildLocalStreamUri("/", source), _resolver);
        }

        void LoadStringData(string source)
        {
            Control.NavigateToString(source);
        }

        internal string GetBaseUrl()
        {
            return Element?.BaseUrl ?? BaseUrl;
        }

        Windows.UI.Color ToWindowsColor(Xamarin.Forms.Color color)
        {
            // Make colour safe for Windows
            if (color.A == -1 || color.R == -1 || color.G == -1 || color.B == -1)
                color = Xamarin.Forms.Color.Transparent;

            return Windows.UI.Color.FromArgb(Convert.ToByte(color.A * 255), Convert.ToByte(color.R * 255), Convert.ToByte(color.G * 255), Convert.ToByte(color.B * 255));
        }
        private void SetCurrentUrl(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Element.CurrentUrl = e.Uri.ToString();
            });
        }


    }
}

