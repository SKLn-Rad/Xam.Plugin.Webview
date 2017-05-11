using Foundation;
using System;
using WebKit;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using ObjCRuntime;

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

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            NavigationRequestedDelegate res = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, navigationAction.Request.Url.ToString()));

            if (res.Cancel)
                decisionHandler(WKNavigationActionPolicy.Cancel);
            else
                decisionHandler(WKNavigationActionPolicy.Allow);
        }

        [Export("webView:decidePolicyForNavigationResponse:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
        {
            if (navigationResponse.Response is NSHttpUrlResponse)
            {
                var sta = ((NSHttpUrlResponse)navigationResponse.Response).StatusCode;
                if (sta >= 400)
                    Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Element, (int) sta));
            }
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

            foreach (var key in Element.GetLocalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            foreach (var key in Element.GetGlobalCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, webView.Url.AbsoluteUrl.ToString()));
        }
    }
}
