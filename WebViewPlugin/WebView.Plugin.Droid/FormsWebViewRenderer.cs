using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.IO;
using Xam.Plugin.Abstractions;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using Xam.Plugin.Droid.Extras;
using Xam.Plugin.Abstractions.Events.Outbound;
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
            OnControlChanged?.Invoke(this, element, webView);
        }

        void OnActionAdded(FormsWebView sender, string key, bool isGlobal)
        {
            if (isGlobal || (Element != null && Element.Equals(sender)))
                InjectJavascript(WebViewControlDelegate.GenerateFunctionScript(key));
        }

        void OnInjectJavascriptRequest(FormsWebView sender, string js)
        {
            if (Element != null && (sender.Equals(Element)))
                InjectJavascript(js);
        }

        void OnStackNavigationRequested(FormsWebView sender, bool forward)
        {
            if (Element == null || (!sender.Equals(Element))) return;

            if (forward)
                Control.GoForward();
            else
                Control.GoBack();
        }

        void SetupElement(FormsWebView element)
        {
            Device.BeginInvokeOnMainThread(() => Control.AddJavascriptInterface(new FormsWebViewJsBridge(this), "bridge"));
            element.PropertyChanged += OnWebViewPropertyChanged;

            WebViewControlDelegate.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            WebViewControlDelegate.OnInjectJavascriptRequest += OnInjectJavascriptRequest;
            WebViewControlDelegate.OnStackNavigationRequested += OnStackNavigationRequested;
            WebViewControlDelegate.OnActionAdded += OnActionAdded;

            SetWebViewBackgroundColor(element.BackgroundColor);

            if (element.Source != null)
                OnUserNavigationRequested(element, element.Source, element.ContentType);
        }

        void DestroyElement(FormsWebView element)
        {
            element.PropertyChanged -= OnWebViewPropertyChanged;

            WebViewControlDelegate.OnNavigationRequestedFromUser -= OnUserNavigationRequested;
            WebViewControlDelegate.OnInjectJavascriptRequest -= OnInjectJavascriptRequest;
            WebViewControlDelegate.OnStackNavigationRequested -= OnStackNavigationRequested;
            WebViewControlDelegate.OnActionAdded -= OnActionAdded;

            if (Control != null)
                Device.BeginInvokeOnMainThread(() => Control.RemoveJavascriptInterface("bridge"));
        }

        void OnWebViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(FormsWebView.BackgroundColor)))
                SetWebViewBackgroundColor(((FormsWebView)sender).BackgroundColor);
        }

        void SetWebViewBackgroundColor(Color backgroundColor)
        {
            if (Control != null)
                Device.BeginInvokeOnMainThread(() => Control.SetBackgroundColor(backgroundColor.ToAndroid()));
        }

        void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType)
        {
            if (Element == null || !sender.Equals(Element)) return;

            switch (contentType)
            {
                case WebViewContentType.Internet:
                    Control.LoadUrl(uri, Element.RequestHeaders);
                    break;
                case WebViewContentType.StringData:
                    Control.LoadDataWithBaseURL(GetCorrectBaseUrl(sender), uri, StringDataSettings.MimeType, StringDataSettings.EncodingType, StringDataSettings.HistoryUri);
                    break;
                case WebViewContentType.LocalFile:
                    Control.LoadUrl(Path.Combine(GetCorrectBaseUrl(sender), uri));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(contentType), contentType, null);
            }
        }

        string GetCorrectBaseUrl(FormsWebView sender)
        {
            if (sender != null)
                return sender.BaseUrl ?? BaseUrl;

            return BaseUrl;
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