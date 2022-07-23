using System;
using Foundation;
using WebKit;
using UIKit;
using Xam.Plugin.WebView.Abstractions;
using Xamarin.Forms;

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

            if ((!string.IsNullOrWhiteSpace(renderer.Element.Username))
                && (!string.IsNullOrWhiteSpace(renderer.Element.Password)))
            {
                if (challenge.PreviousFailureCount > 5) //cancel autorization in case of 5 failing requests
                {
                    completionHandler(NSUrlSessionAuthChallengeDisposition.CancelAuthenticationChallenge, null);
                    return;
                }
                completionHandler(NSUrlSessionAuthChallengeDisposition.UseCredential, new NSUrlCredential(renderer.Element.Username, renderer.Element.Password, NSUrlCredentialPersistence.ForSession));
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

            System.Console.WriteLine("DecidePolicy" + navigationAction.Request.Url?.Host);
            System.Console.WriteLine("DecidePolicy" + renderer.Element.BaseUrl);
            System.Console.WriteLine($"DecidePolicy {navigationAction.Request.Url.Host} {navigationAction.Request is NSMutableUrlRequest}");


#if DEBUG
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0)) {
                webView.Configuration.WebsiteDataStore.HttpCookieStore.GetAllCookies((NSHttpCookie[] obj) => {
                    System.Diagnostics.Debug.WriteLine("*** DecidePolicy webView.Configuration.WebsiteDataStore");
                    for (var i = 0; i < obj.Length; i++) {
                        var nsCookie = obj[i];
                        var domain = nsCookie.Domain;
                        System.Diagnostics.Debug.WriteLine($"Domain={nsCookie.Domain}; Name={nsCookie.Name}; Value={nsCookie.Value};");
                    }
                });
            }
#endif
            if (!UIDevice.CurrentDevice.CheckSystemVersion(11, 0)) {
                var headers = navigationAction.Request.Headers as NSMutableDictionary;
                var cookieDictionary = NSHttpCookie.RequestHeaderFieldsWithCookies(NSHttpCookieStorage.SharedStorage.Cookies);
                foreach (var item in cookieDictionary) {
                    headers.SetValueForKey(item.Value, new NSString(item.Key.ToString()));
                }
            }

            // If navigation target frame is null, this can mean that the link contains target="_blank". Start loadrequest to perform the navigation
            if (navigationAction.TargetFrame == null)
            {
                webView.LoadRequest(navigationAction.Request);
                decisionHandler(WKNavigationActionPolicy.Cancel);
                return;
            }
            // If the navigation event originates from another frame than main (iframe?) it's not a navigation event we care about
            if (!navigationAction.TargetFrame.MainFrame)
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
                return;
            }

            var response = renderer.Element.HandleNavigationStartRequest(navigationAction.Request.Url.ToString());


            var url = navigationAction.Request.Url.ToString();



            if (url == "about:blank")
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
            }
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
                    //_headerIsSet = false;
                    decisionHandler(WKNavigationActionPolicy.Allow);
                    renderer.Element.Navigating = true;
                }
            }
        }

        public override void DecidePolicy(WKWebView webView, WKNavigationResponse navigationResponse, Action<WKNavigationResponsePolicy> decisionHandler)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            System.Console.WriteLine("DecidePolicy Response" + renderer.Element.BaseUrl);

            if (navigationResponse.Response is NSHttpUrlResponse)
            {
                var code = ((NSHttpUrlResponse)navigationResponse.Response).StatusCode;
                if (code >= 400)
                {
                    renderer.Element.Navigating = false;
                    renderer.Element.HandleNavigationError((int)code);
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

        [Foundation.Export("webView:didStartProvisionalNavigation:")]
        [ObjCRuntime.BindingImpl(ObjCRuntime.BindingImplOptions.GeneratedCode | ObjCRuntime.BindingImplOptions.Optimizable)]
        public override void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;
            Device.BeginInvokeOnMainThread(() =>
            {
                renderer.Element.CurrentUrl = webView.Url.ToString();
            });
        }
    }
}
