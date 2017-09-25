using System;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Shared;
using Xam.Plugin.Shared.Resolvers;
using Xam.Plugin.Abstractions.Events.Inbound;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using Windows.Web;
using Windows.UI;
using Windows.Web.Http;
using Xamarin.Forms;
using WebView.Plugin.Abstractions.Events.Inbound;

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#else
using Xamarin.Forms.Platform.WinRT;
#endif

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.Shared
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Windows.UI.Xaml.Controls.WebView>
    {

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;
        private LocalFileStreamResolver _resolver;

        public static string BaseUrl { get; set; } = "ms-appx:///";

        public static void Init()
        {
            //var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
                SetupControl(e.NewElement);

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);
        }

        void SetupControl(FormsWebView element)
        {
            var control = new Windows.UI.Xaml.Controls.WebView();
            
            OnControlChanging?.Invoke(this, Element, control);

            _resolver = new LocalFileStreamResolver(this);
            SetNativeControl(control);

            Control.NavigationFailed += OnNavigationFailed;
            Control.NavigationStarting += OnNavigating;
            Control.NavigationCompleted += OnNavigated;
            Control.DOMContentLoaded += OnContentLoaded;
            Control.ScriptNotify += OnScriptNotify;

            OnControlChanged?.Invoke(this, Element, control);
        }

        async void InjectJavascript(string js)
        {
            await Control.InvokeScriptAsync("eval", new[] { js });
        }

        void OnStackNavigationRequested(bool forward)
        {
            if (forward)
                Control.GoForward();
            else
                Control.GoBack();
        }

        void SetupElement(FormsWebView element)
        {
            element.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            element.OnInjectJavascriptRequest += InjectJavascript;
            element.OnStackNavigationRequested += OnStackNavigationRequested;
            element.OnLocalActionAdded += OnActionAdded;
            element.PropertyChanged += OnWebViewElementPropertyChanged;

            if (element.EnableGlobalCallbacks)
                { FormsWebView.OnGlobalActionAdded += OnActionAdded; }
            
            if (element.Source != null)
                OnUserNavigationRequested(element.Source, element.ContentType);

            SetWebViewBackgroundColor(element.BackgroundColor);
        }

        void DestroyElement(FormsWebView element)
        {
            if (element == null) return;

            element.Destroy();
            element.PropertyChanged -= OnWebViewElementPropertyChanged;
        }

        void OnWebViewElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var webView = sender as FormsWebView;

            if (e.PropertyName.Equals(nameof(FormsWebView.BackgroundColor)))
                SetWebViewBackgroundColor(webView.BackgroundColor);
        }

        void SetWebViewBackgroundColor(Xamarin.Forms.Color backgroundColor)
        {
            if (Control != null)
                Control.DefaultBackgroundColor = ToWindowsColor(backgroundColor);
        }

        private Windows.UI.Color ToWindowsColor(Xamarin.Forms.Color color)
        {
            // Make colour safe for Windows
            if (color.A == -1 || color.R == -1 || color.G == -1 || color.B == -1)
                color = Xamarin.Forms.Color.Transparent;

            return Windows.UI.Color.FromArgb(Convert.ToByte(color.A * 255), Convert.ToByte(color.R * 255), Convert.ToByte(color.G * 255), Convert.ToByte(color.B * 255));
        }

        async void OnActionAdded(string key)
        {
            await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(key) });
        }

        void OnNavigationFailed(object sender, Windows.UI.Xaml.Controls.WebViewNavigationFailedEventArgs e)
        {
            Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Element, (int) e.WebErrorStatus));
        }

        void OnNavigated(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            Element.SetValue(FormsWebView.SourceProperty, uri);
            
            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, uri, args.IsSuccess)); 
        }

        void OnNavigating(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            var nrd = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, uri));

            args.Cancel = nrd?.Cancel ?? false;
        }

        async void OnContentLoaded(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewDOMContentLoadedEventArgs args)
        {
            await Control.InvokeScriptAsync("eval", new[] { FormsWebView.InjectedFunction });

            foreach (var key in Element.GetLocalCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(key) });

            foreach (var key in FormsWebView.GetGlobalCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { FormsWebView.GenerateFunctionScript(key) });

            Element.InvokeEvent(WebViewEventType.NavigationStackUpdate, new NavigationStackUpdateDelegate(Element, Control.CanGoBack, Control.CanGoForward));
            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, args.Uri != null ? args.Uri.AbsoluteUri : ""));
        }

        void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            Element?.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, e.Value));
        }

        void OnUserNavigationRequested(string uri, WebViewContentType contentType)
        {
            switch (contentType)
            {
                case WebViewContentType.Internet:
                    NavigateWithHttpRequest(new Uri(uri));
                    break;
                case WebViewContentType.StringData:
                    LoadStringData(uri);
                    break;
                case WebViewContentType.LocalFile:
                    LoadLocalFile(uri);
                    break;
            }
        }

        void NavigateWithHttpRequest(Uri uri)
        {
            if (Element == null || Control == null) return;

            var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);
            foreach (var header in Element.RequestHeaders)
                requestMsg.Headers.Add(header.Key, header.Value);

            Control.NavigateWithHttpRequestMessage(requestMsg);
        }

        void LoadStringData(string uri)
        {
            Control.NavigateToString(uri);
        }

        void LoadLocalFile(string uri)
        {
            Control.NavigateToLocalStreamUri(Control.BuildLocalStreamUri("/", uri), _resolver);
        }

        internal string GetCorrectBaseUrl()
        {
            if (Element != null)
                return Element.BaseUrl ?? BaseUrl;
            return BaseUrl;
        }
    }
}
