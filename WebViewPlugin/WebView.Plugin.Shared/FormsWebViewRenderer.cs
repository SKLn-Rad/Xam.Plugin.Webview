using System;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Shared;
using Xam.Plugin.Shared.Resolvers;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
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
            var dt = DateTime.Now;
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
            WebViewControlDelegate.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            WebViewControlDelegate.OnInjectJavascriptRequest += InjectJavascript;
            WebViewControlDelegate.OnStackNavigationRequested += OnStackNavigationRequested;
            WebViewControlDelegate.OnActionAdded += OnActionAdded;

            var control = new Windows.UI.Xaml.Controls.WebView();
            OnControlChanging?.Invoke(this, Element, control);

            _resolver = new LocalFileStreamResolver(this);
            SetNativeControl(control);

            OnControlChanged?.Invoke(this, Element, control);
        }

        async void InjectJavascript(FormsWebView sender, string js)
        {
            if (Element != null && sender != null && Control != null && Element == sender)
                await Control.InvokeScriptAsync("eval", new[] { js });
        }

        void OnStackNavigationRequested(FormsWebView sender, bool forward)
        {
            if (sender == null || Control == null || Element == null || !sender.Equals(Element)) return;

            if (forward)
                Control.GoForward();
            else
                Control.GoBack();
        }

        void SetupElement(FormsWebView element)
        {
            element.PropertyChanged += OnWebViewElementPropertyChanged;

            Control.NavigationFailed += OnNavigationFailed;
            Control.NavigationStarting += OnNavigating;
            Control.NavigationCompleted += OnNavigated;
            Control.DOMContentLoaded += OnContentLoaded;
            Control.ScriptNotify += OnScriptNotify;

            if (element.Source != null)
                OnUserNavigationRequested(element, element.Source, element.ContentType);

            SetWebViewBackgroundColor(element.BackgroundColor);
        }

        void DestroyElement(FormsWebView element)
        {
            element.PropertyChanged -= OnWebViewElementPropertyChanged;

            if (Control == null) return;

            Control.NavigationFailed -= OnNavigationFailed;
            Control.NavigationStarting -= OnNavigating;
            Control.NavigationCompleted -= OnNavigated;
            Control.DOMContentLoaded -= OnContentLoaded;
            Control.ScriptNotify -= OnScriptNotify;
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

        async void OnActionAdded(FormsWebView sender, string key, bool isGlobal)
        {
            if (isGlobal || Element.Equals(sender))
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });
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
            await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.InjectedFunction });

            foreach (var key in Element.GetLocalCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });

            foreach (var key in Element.GetGlobalCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });

            Element.InvokeEvent(WebViewEventType.NavigationStackUpdate, new NavigationStackUpdateDelegate(Element, Control.CanGoBack, Control.CanGoForward));
            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, args.Uri != null ? args.Uri.AbsoluteUri : ""));
        }

        void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            Element?.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, e.Value));
        }

        void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType)
        {
            if (sender != Element) return;

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
