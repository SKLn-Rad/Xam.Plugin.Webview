using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.UWP
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WebView>
    {
        
        public static event EventHandler<WebView> OnControlChanged;

        public static string BaseUrl { get; set; } = "ms-appx:///";
        LocalFileStreamResolver _resolver;

        public static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
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
            element.OnBackRequested += OnBackRequested;
            element.OnForwardRequested += OnForwardRequested;
            element.OnRefreshRequested += OnRefreshRequested;

            SetSource();
        }

        void DestroyOldElement(FormsWebView element)
        {
            element.PropertyChanged -= OnWebViewPropertyChanged;
            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequestAsync;
            element.OnBackRequested -= OnBackRequested;
            element.OnForwardRequested -= OnForwardRequested;
            element.OnRefreshRequested -= OnRefreshRequested;

            element.Dispose();
        }

        void SetupControl()
        {
            var control = new WebView();
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

        void OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            if (Element == null) return;

            Element.Navigating = true;
            var handler = Element.HandleNavigationStartRequest(args.Uri != null ? args.Uri.AbsoluteUri : Element.Source);
            args.Cancel = handler.Cancel;
        }

        void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (Element == null) return;

            if (!args.IsSuccess)
                Element.HandleNavigationError((int) args.WebErrorStatus);

            Element.CanGoBack = Control.CanGoBack;
            Element.CanGoForward = Control.CanGoForward;

            Element.Navigating = false;
            Element.HandleNavigationCompleted();
        }

        async void OnDOMContentLoaded(WebView sender, WebViewDOMContentLoadedEventArgs args)
        {
            if (Element == null) return;
            
            // Add Injection Function
            await Control.InvokeScriptAsync("eval", new[] { FormsWebView.InjectedFunction });

            // Add Global Callbacks
            foreach (var callback in FormsWebView.GlobalRegisteredCallbacks)
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(callback.Key) });

            // Add Local Callbacks
            foreach (var callback in Element.LocalRegisteredCallbacks)
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(callback.Key) });

            Element.HandleContentLoaded();
        }

        async void OnCallbackAdded(object sender, string e)
        {
            if (Element == null) return;

            if (sender == null || sender.Equals(Element))
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(e) });
        }

        void OnScriptNotify(object sender, NotifyEventArgs e)
        {
            if (Element == null) return;
            Element.HandleScriptReceived(e.Value);
        }

        async Task<string> OnJavascriptInjectionRequestAsync(string js)
        {
            if (Control == null) return string.Empty;
            var result = await Control.InvokeScriptAsync("eval", new[] { js });
            return result;
        }

        void SetSource()
        {
            if (Element == null || Control == null || Element.Source == null) return;
            
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
            foreach (var header in FormsWebView.GlobalRegisteredHeaders)
            {
                if (!requestMsg.Headers.ContainsKey(header.Key))
                    requestMsg.Headers.Add(header.Key, header.Value);
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
