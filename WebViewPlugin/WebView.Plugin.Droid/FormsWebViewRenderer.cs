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

[assembly: ExportRenderer(typeof(FormsWebView), typeof(Xam.Plugin.Droid.FormsWebViewRenderer))]
namespace Xam.Plugin.Droid
{
    public partial class FormsWebViewRenderer : ViewRenderer<FormsWebView, Android.Webkit.WebView>
    {

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;

        private WebViewStringDataSettings StringDataSettings { get; set; } = new WebViewStringDataSettings();
        private FormsWebViewClient WebViewClient { get; set; }
        private FormsWebViewChromeClient ChromeClient { get; set; }

        public static string BaseUrl { get; set; } = "file:///android_asset/";

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
            webView.SetWebViewClient((WebViewClient = new FormsWebViewClient(element, this)));
            webView.SetWebChromeClient((ChromeClient = new FormsWebViewChromeClient(this)));

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
                InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));
        }

        void OnInjectJavascriptRequest(FormsWebView sender, string js)
        {
            if (Element != null && (sender.Equals(Element)))
                InjectJS(js);
        }

        void OnStackNavigationRequested(FormsWebView sender, bool forward)
        {
            if (Element != null && (sender.Equals(Element)))
            {
                if (forward)
                    Control.GoForward();
                else
                    Control.GoBack();
            }
        }

        void SetupElement(FormsWebView element)
        {
            Device.BeginInvokeOnMainThread(() => Control.AddJavascriptInterface(new FormsWebViewJSBridge(this), "bridge"));
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

            if (this != null && Control != null)
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
            if (Element != null && sender.Equals(Element))
            {
                switch (contentType)
                {
                    case WebViewContentType.Internet:
                        Control.LoadUrl(uri);
                        break;
                    case WebViewContentType.StringData:
                        Control.LoadDataWithBaseURL(GetCorrectBaseUrl(sender), uri, StringDataSettings.MimeType, StringDataSettings.EncodingType, StringDataSettings.HistoryUri);
                        break;
                    case WebViewContentType.LocalFile:
                        Control.LoadUrl(Path.Combine(GetCorrectBaseUrl(sender), uri));
                        break;
                }
            }
        }

        string GetCorrectBaseUrl(FormsWebView sender)
        {
            if (sender != null)
                return sender.BaseUrl != null ? sender.BaseUrl : BaseUrl;

            return BaseUrl;
        }

        internal void InjectJS(string script)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (Control != null)
                    Control.LoadUrl(string.Format("javascript: {0}", script));
            });
        }

        internal void OnScriptNotify(string script)
        {
            if (Element != null)
                Element.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, script));
        }
    }
}