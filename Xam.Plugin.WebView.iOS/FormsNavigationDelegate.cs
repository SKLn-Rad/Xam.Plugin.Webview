using System;
using Foundation;
using WebKit;
using Xam.Plugin.WebView.Abstractions;
using UIKit;
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
                //else
                //{
                //    var request = navigationAction.Request.Copy();
                //    System.Console.WriteLine(navigationAction.Request.Url?.Host);
                //    System.Console.WriteLine(renderer.Element.BaseUrl);
                //    bool _headerIsSet = false;
                //    // check if the header is set and if not, create a muteable copy of the original request
                //    if (/*navigationAction.Request.Url.Host == "192.168.1.51" &&*/ request is NSMutableUrlRequest mutableRequest)
                //    {
                //        // set the headers of the new request to the created dict
                //        if (renderer.Element.EnableGlobalHeaders)
                //        {
                //            var keys = new object[FormsWebView.GlobalRegisteredHeaders.Count];
                //            var values = new object[FormsWebView.GlobalRegisteredHeaders.Count];
                //            int index = 0;
                //            foreach (var header in FormsWebView.GlobalRegisteredHeaders)
                //            {
                //                keys[index] = header.Key;
                //                values[index] = header.Value;

                //                //if (!mutableRequest.Headers.ContainsKey(new NSString(header.Key)))
                //                //    mutableRequest.Headers.SetValueForKey(new NSString(header.Value), new NSString(header.Key));
                //            }
                //            var headerDict = NSDictionary.FromObjectsAndKeys(values, keys);
                //            mutableRequest.Headers = headerDict;

                //            _headerIsSet = true;
                //        }

                //        if (_headerIsSet)
                //        {
                //            // attempt to load the newly created request
                //            webView.LoadRequest(mutableRequest);
                //            // abort the old one
                //            decisionHandler(WKNavigationActionPolicy.Cancel);
                //            // exit this whole method
                //            return;
                //        }
                //        else
                //        {
                //            _headerIsSet = false;
                //            decisionHandler(WKNavigationActionPolicy.Allow);
                //            renderer.Element.Navigating = true;
                //        }
                //    }
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
        public virtual void DidStartProvisionalNavigation(WKWebView webView, WKNavigation navigation)
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
