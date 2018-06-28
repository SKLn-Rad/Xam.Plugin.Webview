using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Xam.Plugin.WebView.Abstractions;
using Xam.Plugin.WebView.Abstractions.Enumerations;
using Xam.Plugin.WebView.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.WebView.UWP
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Windows.UI.Xaml.Controls.WebView>
    {

        public static event EventHandler<Windows.UI.Xaml.Controls.WebView> OnControlChanged;
        private CookieCollection cookieCollection = new CookieCollection();
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
            element.OnGetCookieValueRequested += OnGetCookieRequest;
            element.OnGetAllCookiesRequested += OnGetAllCookieRequest;
            element.OnSetCookieValueRequested += OnSetCookieRequest;
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
            element.OnGetAllCookiesRequested -= OnGetAllCookieRequest;
            element.OnGetCookieValueRequested -= OnGetCookieRequest;
            element.OnSetCookieValueRequested -= OnSetCookieRequest;
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


            /* Setting cookies */


            var filter = new HttpBaseProtocolFilter();
            HttpClient client = new HttpClient(filter);
            HttpCookieCollection httpCookieCollection = filter.CookieManager.GetCookies(navigationEventArgs.Uri);
            //httpCookieCollection.
            // Use this, while it comes from an instance, its shared across everything.
            foreach (var cookie in httpCookieCollection)
            {
                cookieCollection.Add(new Cookie
                {
                    Domain = cookie.Domain,
                    Name = cookie.Name,
                    Value = cookie.Value,

                });
            }

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
            var cookie = string.Empty;
            foreach (Cookie currentCookie in cookieCollection.GetCookies(new Uri(Element.Source)))
            {
                cookie += currentCookie.Name + "=" + currentCookie.Value + "; ";
            }

            if(cookie.Length > 2) {
                cookie = cookie.Remove(cookie.Length - 2);
            }
            // This clears all tmp. data. Not only cookies
            //await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
            return cookie;
        }

        private async Task<string> OnGetAllCookieRequest() {
            if (Control == null || Element == null) return string.Empty;
        }

        private async Task<string> OnGetCookieRequest(string cookieName)
        {
            if (Control == null || Element == null) return;

            var cookie = string.Empty;
            foreach(Cookie currentCookie in cookieCollection.GetCookies(new Uri(Element.Source))) {
                if (currentCookie.Name == cookieName)
                {
                    cookie = currentCookie.Value;
                    break;
                }
            }
            // This clears all tmp. data. Not only cookies
            //await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
            return cookie;
        }

        private async Task<string> OnSetCookieRequest(string cookieName, string cookieValue)
        {
            if (Control == null) return;

            // This clears all tmp. data. Not only cookies
            //await Windows.UI.Xaml.Controls.WebView.ClearTemporaryWebDataAsync();
            throw new NotImplementedException();
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
    }
}
