using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.IO;
using Xam.Plugin.Abstractions;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using Xam.Plugin.Droid.Extras;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Abstractions.Events.Inbound;
using Android.OS;
using Android.Webkit;
using Java.Lang;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(Xam.Plugin.Droid.FormsWebViewRenderer))]
namespace Xam.Plugin.Droid
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Android.Webkit.WebView>
    {

        public static event WebViewControlChangedDelegate OnControlChanging;

        public static event WebViewControlChangedDelegate OnControlChanged;

        WebViewStringDataSettings StringDataSettings { get; set; } = new WebViewStringDataSettings();

        FormsWebViewClient WebViewClient { get; set; }

        FormsWebViewChromeClient ChromeClient { get; set; }

        public static string BaseUrl { get; set; } = "file:///android_asset/";

        public static bool IgnoreSslGlobally { get; set; } = false;

        public static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            if (Control == null)
                SetupControl(e.NewElement);

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);

            base.OnElementChanged(e);
        }

        void SetupControl(FormsWebView element)
        {
            var webView = new Android.Webkit.WebView(Forms.Context);
            webView.SetWebViewClient(WebViewClient = new FormsWebViewClient(element, this));
            webView.SetWebChromeClient(ChromeClient = new FormsWebViewChromeClient(this));

            // https://github.com/SKLn-Rad/Xam.Plugin.Webview/issues/11
            webView.LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent);

            // Defaults
            webView.Settings.JavaScriptEnabled = true;
            webView.Settings.DomStorageEnabled = true;

            OnControlChanging?.Invoke(this, element, webView);

            SetNativeControl(webView);
            Control.AddJavascriptInterface(new FormsWebViewJsBridge(this), "bridge");

            OnControlChanged?.Invoke(this, element, webView);
        }

        void OnActionAdded(string key)
        {
            InjectJavascript(FormsWebView.GenerateFunctionScript(key));
        }

        void OnGlobalActionAdded(string key)
        {
            InjectJavascript(FormsWebView.GenerateFunctionScript(key));
        }

        void OnInjectJavascriptRequest(string js)
        {
            InjectJavascript(js);
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
            element.PropertyChanged += OnWebViewPropertyChanged;

            element.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            element.OnInjectJavascriptRequest += OnInjectJavascriptRequest;
            element.OnStackNavigationRequested += OnStackNavigationRequested;
            element.OnLocalActionAdded += OnActionAdded;

            if (element.EnableGlobalCallbacks)
                { FormsWebView.OnGlobalActionAdded += OnActionAdded; }

            SetWebViewBackgroundColor(element.BackgroundColor);

            if (element.Source != null)
                element.PerformNavigation(element.Source, element.ContentType);
        }

        void DestroyElement(FormsWebView element)
        {
            element.Destroy();
            element.PropertyChanged -= OnWebViewPropertyChanged;

            element.OnNavigationRequestedFromUser -= OnUserNavigationRequested;
            element.OnInjectJavascriptRequest -= OnInjectJavascriptRequest;
            element.OnStackNavigationRequested -= OnStackNavigationRequested;
            element.OnLocalActionAdded -= OnActionAdded;
        }

        void OnWebViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(FormsWebView.BackgroundColor)))
                SetWebViewBackgroundColor(((FormsWebView)sender).BackgroundColor);
        }

        void SetWebViewBackgroundColor(Color backgroundColor)
        {
            Device.BeginInvokeOnMainThread(() => Control?.SetBackgroundColor(backgroundColor.ToAndroid()));
        }

        void OnUserNavigationRequested(string uri, WebViewContentType contentType)
        {
            if (Element == null) return;

            switch (contentType)
            {
                case WebViewContentType.Internet:
                    Control.LoadUrl(uri, Element.RequestHeaders);
                    break;
                case WebViewContentType.StringData:
                    HandleStringDataRequest(uri);
                    break;
                case WebViewContentType.LocalFile:
                    Control.LoadUrl(Path.Combine(Element.BaseUrl ?? BaseUrl, uri));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
            }
        }

        void HandleStringDataRequest(string uri)
        {
            if (Element == null) return;

            // String data doesn't allow cancellation, this should allow that
            var combinedUri = Path.Combine(Element.BaseUrl ?? BaseUrl, uri);
            var request = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, combinedUri));

            if (!request.Cancel)
                Control.LoadDataWithBaseURL(Element.BaseUrl ?? BaseUrl, uri, StringDataSettings.MimeType, StringDataSettings.EncodingType, StringDataSettings.HistoryUri);
        }

        internal void InjectJavascript(string script)
        {
            Device.BeginInvokeOnMainThread(() => Control?.LoadUrl($"javascript: {script}"));
        }

        /// <summary>
        /// Less than or equal to API 18
        /// </summary>
        /// <param name="script">The response object</param>
        internal void OnScriptNotify(string script)
        {
            Element?.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, script));
        }
    }
}