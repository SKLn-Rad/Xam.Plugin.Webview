using Foundation;
using System;
using WebKit;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using ObjCRuntime;
using WebView.Plugin.Abstractions.Events.Inbound;

namespace Xam.Plugin.iOS.Extras
{
    public class FormsWKNavigationDelegate : WKNavigationDelegate
    {

        private readonly FormsWebViewRenderer Renderer;
        private readonly FormsWebView Element;

        public FormsWKNavigationDelegate(FormsWebViewRenderer renderer, FormsWebView element)
        {
            Renderer = renderer;
            Element = element;
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            var res = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, navigationAction.Request.Url.ToString()));
            decisionHandler(res?.Cancel ?? false ? WKNavigationActionPolicy.Cancel : WKNavigationActionPolicy.Allow);
        }

        [Export("webView:decidePolicyForNavigationResponse:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
        {
            if (!(navigationResponse.Response is NSHttpUrlResponse)) return;

            var sta = ((NSHttpUrlResponse)navigationResponse.Response).StatusCode;

            if (sta >= 400)
                Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Element, (int) sta));
            else
                decisionHandler(WKNavigationResponsePolicy.Allow);
        }

        [Export("webView:didCommitNavigation:")]
        public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (webView.Url.AbsoluteUrl != null)
                Element.SetValue(FormsWebView.SourceProperty, webView.Url.AbsoluteUrl.ToString());

            if (webView.Url.AbsoluteUrl != null)
                Element.InvokeEvent(WebViewEventType.NavigationComplete,
                    new NavigationCompletedDelegate(Element, webView.Url.AbsoluteUrl.ToString()));
        }

        [Export("webView:didFinishNavigation:")]
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);

            foreach (var key in Element.GetLocalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            foreach (var key in Element.GetGlobalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.NavigationStackUpdate, new NavigationStackUpdateDelegate(Element, Renderer.Control.CanGoBack, Renderer.Control.CanGoForward));
            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, webView.Url.AbsoluteUrl.ToString()));
        }
    }
}
