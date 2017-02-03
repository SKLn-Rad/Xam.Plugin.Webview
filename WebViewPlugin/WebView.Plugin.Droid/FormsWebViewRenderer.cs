using System;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using WebView.Plugin.Abstractions;
using WebView.Plugin.Droid.Extras;
using static WebView.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using WebView.Plugin.Abstractions.Enumerations;
using WebView.Plugin.Abstractions.Events.Outbound;
using WebView.Plugin.Abstractions.Inbound;
using System.IO;
using WebView.Plugin.Abstractions.Events.Inbound;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(WebView.Plugin.Droid.FormsWebViewRenderer))]
namespace WebView.Plugin.Droid
{
    public partial class FormsWebViewRenderer : ViewRenderer<FormsWebView, Android.Webkit.WebView>
    {

        internal WebViewEventAbstraction _eventAbstraction;

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;

        public WebViewStringDataSettings StringDataSettings { get; set; } = new WebViewStringDataSettings();
        public FormsWebViewClient WebViewClient { get; set; }
        public FormsWebViewChromeClient ChromeClient { get; set; }

        public static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                SetupControl(e.NewElement);

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);
        }

        private void SetupControl(FormsWebView element)
        {
            WebViewControlDelegate.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            WebViewControlDelegate.ObtainUri += OnUserRequestUri;
            WebViewControlDelegate.OnInjectJavascriptRequest += OnInjectJavascriptRequest;

            _eventAbstraction = new WebViewEventAbstraction() { Source = new WebViewEventStub() };

            var webView = new Android.Webkit.WebView(Forms.Context);
            webView.SetWebViewClient((WebViewClient = new FormsWebViewClient(element, this)));
            webView.SetWebChromeClient((ChromeClient = new FormsWebViewChromeClient(this)));

            // Defaults
            webView.Settings.JavaScriptEnabled = true;

            OnControlChanging?.Invoke(this, element, webView);
            SetNativeControl(webView);
            OnControlChanged?.Invoke(this, element, webView);
        }

        private void OnInjectJavascriptRequest(FormsWebView sender, string js)
        {
            if (sender == Element)
                InjectJS(js);
        }

        private string OnUserRequestUri(FormsWebView sender)
        {
            if (sender == Element && Control != null)
                return Control.Url.ToString();
            return "";
        }

        private void SetupElement(FormsWebView element)
        {
            Control.AddJavascriptInterface(new FormsWebViewJSBridge(this), "bridge");
        }

        private void DestroyElement(FormsWebView element)
        {
            Control.RemoveJavascriptInterface("bridge");
        }

        private void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType, string baseUri = "")
        {
            if (sender == Element)
            {
                switch (contentType)
                {
                    case WebViewContentType.Internet:
                        Control.LoadUrl(uri);
                        break;
                    case WebViewContentType.StringData:
                        Control.LoadDataWithBaseURL(baseUri, uri, StringDataSettings.MimeType, StringDataSettings.EncodingType, StringDataSettings.HistoryUri);
                        break;
                    case WebViewContentType.LocalFile:
                        Control.LoadUrl(Path.Combine("file:///android_asset/", uri));
                        break;
                }
            }
        }

        internal void InjectJS(string script)
        {
            if (Control != null)
                Control.LoadUrl(string.Format("javascript: {0}", script));
        }

        internal void OnScriptNotify(string script)
        {
            _eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, script));
        }
    }
}