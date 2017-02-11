using Foundation;
using System;
using WebKit;
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

        [Export("webView:didFinishNavigation:")]
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);
            
            if (webView.Url.AbsoluteUrl != null)
                Element.SetValue(Element.UriProperty, webView.Url.AbsoluteUrl.ToString());

            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, webView.Url.AbsoluteUrl.ToString(), true));
        }
    }
}
