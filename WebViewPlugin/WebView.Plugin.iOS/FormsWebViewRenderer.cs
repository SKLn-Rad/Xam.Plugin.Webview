using System;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using System.IO;
using WebView.Plugin.Abstractions;
using WebView.Plugin.iOS.Extras;
using WebView.Plugin.Abstractions.Events.Outbound;
using WebView.Plugin.Abstractions.Inbound;
using static WebView.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using WebView.Plugin.Abstractions.Events.Inbound;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(WebView.Plugin.iOS.FormsWebViewRenderer))]
namespace WebView.Plugin.iOS
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WKWebView>, IWKScriptMessageHandler
    {

        internal WebViewEventAbstraction _eventAbstraction;

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;

        public WKUserContentController UserController;
        public WKWebViewConfiguration WebViewConfiguration;
        private FormsWKNavigationDelegate NavigationDelegate;

        public string UriBase { get; set; }

        public new static void Init()
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
            WebViewControlDelegate.OnInjectJavascriptRequest += OnInjectJavascriptRequested;

            _eventAbstraction = new WebViewEventAbstraction() { Source = new WebViewEventStub() };

            UserController = new WKUserContentController();
            UserController.AddScriptMessageHandler(this, "invokeAction");

            NavigationDelegate = new FormsWKNavigationDelegate(this, element);
            WebViewConfiguration = new WKWebViewConfiguration { UserContentController = UserController };
            
            var webView = new WKWebView(Frame, WebViewConfiguration);
            webView.NavigationDelegate = NavigationDelegate;
            
            UriBase = NSBundle.MainBundle.ResourcePath;

            OnControlChanging?.Invoke(this, Element, Control);
            SetNativeControl(webView);
            OnControlChanged?.Invoke(this, Element, Control);
        }

        private void OnInjectJavascriptRequested(FormsWebView sender, string js)
        {
            if (Element == sender)
                InjectJS(js);
        }

        private void DestroyElement(FormsWebView element)
        {
        }

        private void SetupElement(FormsWebView element)
        {
        }

        internal void InjectJS(string js)
        {
            if (Control != null)
                InvokeOnMainThread(async () => await Control.EvaluateJavaScriptAsync(new NSString(js)));
        }

        private string OnUserRequestUri(FormsWebView sender)
        {
            if (sender == Element && Control != null)
                return Control.Url.AbsoluteUrl.ToString();
            return "";
        }

        private void OnUserNavigationRequested(FormsWebView sender, string uri, Abstractions.Enumerations.WebViewContentType contentType, string baseUri)
        {
            if (Element == sender)
            {
                switch (contentType)
                {
                    case Abstractions.Enumerations.WebViewContentType.Internet:
                        Control.LoadRequest(new NSUrlRequest(new NSUrl(uri)));
                        break;
                    case Abstractions.Enumerations.WebViewContentType.LocalFile:
                        Control.LoadFileUrl(new NSUrl("file://" + Path.Combine(UriBase, uri)), new NSUrl("file://" + UriBase));
                        break;
                    case Abstractions.Enumerations.WebViewContentType.StringData:
                        Control.LoadHtmlString(new NSString(uri), baseUri == null ? null : new NSUrl(baseUri));
                        break;
                }
            }
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            _eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, message.Body.ToString()));
        }
    }
}
