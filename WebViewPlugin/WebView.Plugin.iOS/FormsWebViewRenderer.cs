using System;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using System.IO;
using Xam.Plugin.Abstractions;
using Xam.Plugin.iOS.Extras;
using Xam.Plugin.Abstractions.Events.Outbound;
using Xam.Plugin.Abstractions.Events.Inbound;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using Xam.Plugin.iOS;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.iOS
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WKWebView>, IWKScriptMessageHandler
    {

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
            WebViewControlDelegate.OnInjectJavascriptRequest += OnInjectJavascriptRequested;

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
            if (element.Uri != null)
                OnUserNavigationRequested(element, element.Uri, element.ContentType, element.BasePath);
        }

        internal void InjectJS(string js)
        {
            if (Control != null)
                InvokeOnMainThread(async () => await Control.EvaluateJavaScriptAsync(new NSString(js)));
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
            Element.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, message.Body.ToString()));
        }
    }
}
