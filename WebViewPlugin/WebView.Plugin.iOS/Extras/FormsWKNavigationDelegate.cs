using Foundation;
using System;
using WebKit;
using WebView.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;

namespace Xam.Plugin.iOS.Extras
{
    public class FormsWKNavigationDelegate : WKNavigationDelegate
    {

        private FormsWebViewRenderer Renderer;
        private FormsWebView Element;

        public FormsWKNavigationDelegate(FormsWebViewRenderer renderer, FormsWebView element)
        {
            Renderer = renderer;
            Element = element;
        }

        [Export("webView:didStartProvisionalNavigation:")]
        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {

        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            NavigationRequestedDelegate res = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, navigationAction.Request.Url.ToString()));

            if (res.Cancel)
                decisionHandler(WKNavigationActionPolicy.Cancel);
            else
                decisionHandler(WKNavigationActionPolicy.Allow);
        }

        [Export("webView:didCommitNavigation:")]
        public override void DidCommitNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (webView.Url.AbsoluteUrl != null)
                Element.SetValue(FormsWebView.SourceProperty, webView.Url.AbsoluteUrl.ToString());

            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, webView.Url.AbsoluteUrl.ToString(), true));
        }

        [Export("webView:didFinishNavigation:")]
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);
            foreach (var key in Element.GetAllCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, webView.Url.AbsoluteUrl.ToString()));
        }
    }
}
