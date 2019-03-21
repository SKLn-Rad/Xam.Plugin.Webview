using System;
using Foundation;
using WebKit;
using Xam.Plugin.WebView.Abstractions;
using UIKit;

namespace Xam.Plugin.WebView.iOS
{
    public class FormsNavigationDelegate : WKNavigationDelegate
    {

        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsNavigationDelegate(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public bool AttemptOpenCustomUrlScheme(NSUrl url)
        {
            var app = UIApplication.SharedApplication;

            if (app.CanOpenUrl(url))
                return app.OpenUrl(url);

            return false;
        }

        public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) 
            {
                completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
                return;
            }
            if (renderer?.Element == null) {
                completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
                return;
            }

            if ((!string.IsNullOrWhiteSpace(renderer.Element.UserName))
                && (!string.IsNullOrWhiteSpace(renderer.Element.Password)))
            {
                if (challenge.PreviousFailureCount > 5) //cancel autorization in case of 5 failing requests
                {
                    completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
                    return;
                }
                completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, new NSUrlCredential(renderer.Element.UserName, renderer.Element.Password, NSUrlCredentialPersistence.ForSession));
            }
            else
            {
                completionHandler(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null); 
            }
        }

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
			if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
			if (renderer.Element == null) return;
            
            var response = renderer.Element.HandleNavigationStartRequest(navigationAction.Request.Url.ToString());
            
            if (response.Cancel || response.OffloadOntoDevice)
            {
                if (response.OffloadOntoDevice)
                    AttemptOpenCustomUrlScheme(navigationAction.Request.Url);

                decisionHandler(WKNavigationActionPolicy.Cancel);
            }

            else
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
                renderer.Element.Navigating = true;
            }
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
        {
			if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
			if (renderer.Element == null) return;

            if (navigationResponse.Response is NSHttpUrlResponse) {
                var code = ((NSHttpUrlResponse)navigationResponse.Response).StatusCode;
                if (code >= 400) {
                    renderer.Element.Navigating = false;
                    renderer.Element.HandleNavigationError((int) code);
                    decisionHandler(WKNavigationResponsePolicy.Cancel);
                    return;
                }
            }

            decisionHandler(WKNavigationResponsePolicy.Allow);
        }

        [Export("webView:didFinishNavigation:")]
        public async override void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
			if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
			if (renderer.Element == null) return;

            renderer.Element.HandleNavigationCompleted(webView.Url.ToString());
            await renderer.OnJavascriptInjectionRequest(FormsWebView.InjectedFunction);

            if (renderer.Element.EnableGlobalCallbacks)
                foreach (var function in FormsWebView.GlobalRegisteredCallbacks)
    				await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(function.Key));

            foreach (var function in renderer.Element.LocalRegisteredCallbacks)
                await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(function.Key));

            renderer.Element.CanGoBack = webView.CanGoBack;
            renderer.Element.CanGoForward = webView.CanGoForward;
            renderer.Element.Navigating = false;
            renderer.Element.HandleContentLoaded();
        }
    }
}
