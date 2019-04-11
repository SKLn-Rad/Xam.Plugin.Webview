﻿using System;
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

        [Export("webView:decidePolicyForNavigationAction:decisionHandler:")]
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
			if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
			if (renderer.Element == null) return;
            
            var response = renderer.Element.HandleNavigationStartRequest(navigationAction.Request.Url.ToString());
            var url = navigationAction.Request.Url.ToString();

            if (url == "about:blank")
                decisionHandler(WKNavigationActionPolicy.Allow);
            else
            { 
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

        [Export("webView:didReceiveAuthenticationChallenge:completionHandler:")]
        public override void DidReceiveAuthenticationChallenge(WKWebView webView, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;
            if (challenge == null || challenge.ProtectionSpace == null || challenge.ProtectionSpace.AuthenticationMethod == null) return;

            if (challenge.ProtectionSpace.AuthenticationMethod == "NSURLAuthenticationMethodServerTrust")
            {
                if (renderer.Element.IgnoreSSLErrors)
                {
                    using (var cred = NSUrlCredential.FromTrust(challenge.ProtectionSpace.ServerSecTrust))
                    {
                        completionHandler.Invoke(NSUrlSessionAuthChallengeDisposition.UseCredential, cred);
                    }
                }
                else
                {
                    completionHandler.Invoke(NSUrlSessionAuthChallengeDisposition.PerformDefaultHandling, null);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(renderer.Element.Username) && !string.IsNullOrEmpty(renderer.Element.Password))
                {
                    var crendential = new NSUrlCredential(renderer.Element.Username, renderer.Element.Password, NSUrlCredentialPersistence.ForSession);

                    completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, crendential);
                }
            }
        }
    }
}
