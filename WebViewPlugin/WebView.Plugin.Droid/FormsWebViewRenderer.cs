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

        public string BaseUrl { get; set; } = "file:///android_asset/";

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
            WebViewControlDelegate.OnInjectJavascriptRequest += OnInjectJavascriptRequest;
            WebViewControlDelegate.OnActionAdded += OnActionAdded;

            var webView = new Android.Webkit.WebView(Forms.Context);
            webView.SetWebViewClient((WebViewClient = new FormsWebViewClient(element, this)));
            webView.SetWebChromeClient((ChromeClient = new FormsWebViewChromeClient(this)));

            // Defaults
            webView.Settings.JavaScriptEnabled = true;

            OnControlChanging?.Invoke(this, element, webView);
            SetNativeControl(webView);
            OnControlChanged?.Invoke(this, element, webView);
        }

        private void OnActionAdded(FormsWebView sender, string key)
        {
            InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));
        }

        private void OnInjectJavascriptRequest(FormsWebView sender, string js)
        {
            if (sender == Element)
                InjectJS(js);
        }

        private void SetupElement(FormsWebView element)
        {
            Control.AddJavascriptInterface(new FormsWebViewJSBridge(this), "bridge");

            if (element.Source != null)
                OnUserNavigationRequested(element, element.Source, element.ContentType);
        }

        private void DestroyElement(FormsWebView element)
        {
            if (this != null && Control != null)
                Control.RemoveJavascriptInterface("bridge");
        }

        private void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType)
        {
            if (sender == Element)
            {
                switch (contentType)
                {
                    case WebViewContentType.Internet:
                        Control.LoadUrl(uri);
                        break;
                    case WebViewContentType.StringData:
                        Control.LoadDataWithBaseURL(BaseUrl, uri, StringDataSettings.MimeType, StringDataSettings.EncodingType, StringDataSettings.HistoryUri);
                        break;
                    case WebViewContentType.LocalFile:
                        LoadLocalFile(uri);
                        break;
                }
            }
        }

        private void LoadLocalFile(string uri)
        {
            if (BaseUrl == null)
                throw new Exception("Base URL was not set, could not load local content");

            Control.LoadUrl(Path.Combine(BaseUrl, uri));
        }

        internal void InjectJS(string script)
        {
            if (Control != null)
                Control.LoadUrl(string.Format("javascript: {0}", script));
        }

        internal void OnScriptNotify(string script)
        {
            Element.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, script));
        }
    }
}