using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using WebKit;
using ObjCRuntime;
using WebView.Plugin.Abstractions;
using WebView.Plugin.Abstractions.Events.Inbound;
using WebView.Plugin.Abstractions.Events.Outbound;

namespace WebView.Plugin.iOS.Extras
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
            NavigationRequestedDelegate res = (NavigationRequestedDelegate) Renderer._eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, navigationAction.Request.Url.ToString()));

            if (res.Cancel)
                decisionHandler(WKNavigationActionPolicy.Cancel);
            else
                decisionHandler(WKNavigationActionPolicy.Allow);
        }

        [Export("webView:didFinishNavigation:")]
        public override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);
            Renderer._eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, webView.Url.AbsoluteUrl.ToString(), true));
        }
    }
}
