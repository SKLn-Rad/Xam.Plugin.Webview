﻿//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Threading.Tasks;
//using Foundation;
//using UIKit;
//using Xam.Plugin.WebView.Abstractions;
//using Xam.Plugin.WebView.Abstractions.Delegates;
//using Xam.Plugin.WebView.Abstractions.Enumerations;
//using Xam.Plugin.WebView.iOS;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.iOS;

//[assembly: Xamarin.Forms.ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewOldRenderer))]
//namespace Xam.Plugin.WebView.iOS
//{
//    public class FormsWebViewOldRenderer : ViewRenderer<FormsWebView, UIWebView>
//    {
//        public FormsWebViewOldRenderer()
//        {

//        }

//        public static event EventHandler<UIWebView> OnControlChanged;

//        public static string BaseUrl { get; set; } = NSBundle.MainBundle.BundlePath;

//        private Dictionary<string, int> _cookieDomains = new Dictionary<string, int>();

//        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
//        {
//            base.OnElementChanged(e);

//            if (Control == null && Element != null)
//                SetupControl();

//            if (e.NewElement != null)
//                SetupElement(e.NewElement);

//            if (e.OldElement != null)
//                DestroyElement(e.OldElement);
//        }

//        void OnCallbackAdded(object sender, string e)
//        {
//            if (Element == null || string.IsNullOrWhiteSpace(e)) return;

//            if ((sender == null && Element.EnableGlobalCallbacks) || sender != null)
//                OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(e));
//        }

//        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
//        {
//            switch (e.PropertyName) {
//                case "Source":
//                    SetSource();
//                    break;
//            }
//        }

//        private Task OnClearCookiesRequest()
//        {
//            if (Control == null) Task.FromResult<object>(null);

//            foreach (var domain in _cookieDomains) {
//                var domainUrl = NSUrl.FromString(domain.Key);
//                foreach (var cookie in NSHttpCookieStorage.SharedStorage.CookiesForUrl(domainUrl)) {
//                    NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
//                    _cookieDomains[domain.Key] = domain.Value - 1;
//                }
//            }
//#if DEBUG
//            OnPrintCookiesRequested();
//#endif
//            return Task.FromResult<object>(null);
//        }

//        private Task OnPrintCookiesRequested(IEnumerable<string> urls = null)
//        {
//#if DEBUG
//            NSHttpCookie[] cookies;

//            System.Diagnostics.Debug.WriteLine("*** NSHttpCookieStorage.SharedStorage.Cookies ***");
//            cookies = NSHttpCookieStorage.SharedStorage.Cookies;
//            if (cookies != null) {
//                foreach (var nsCookie2 in cookies) {
//                    System.Diagnostics.Debug.WriteLine($"Domain={nsCookie2.Domain}; Name={nsCookie2.Name}; Value={nsCookie2.Value};");
//                }
//            }
//#endif
//            return Task.FromResult<object>(null);
//        }

//        private Task OnAddCookieRequested(System.Net.Cookie cookie)
//        {
//            if (Control == null || cookie == null || String.IsNullOrEmpty(cookie.Domain) || String.IsNullOrEmpty(cookie.Name))
//                return Task.FromResult<object>(null);

//            var nsCookie = new NSHttpCookie(cookie);
//            NSHttpCookieStorage.SharedStorage.SetCookie(nsCookie);

//            if (!_cookieDomains.ContainsKey(cookie.Domain)) {
//                _cookieDomains[cookie.Domain] = 0;
//            }
//            _cookieDomains[cookie.Domain] = _cookieDomains[cookie.Domain] + 1;
//#if DEBUG
//            OnPrintCookiesRequested();
//#endif
//            return Task.FromResult<object>(null);
//        }

//        void OnRefreshRequested(object sender, EventArgs e)
//        {
//            if (Control == null) return;
//            Control.Reload();
//        }

//        void OnForwardRequested(object sender, EventArgs e)
//        {
//            if (Control == null || Element == null) return;

//            if (Control.CanGoForward)
//                Control.GoForward();
//        }

//        void OnBackRequested(object sender, EventArgs e)
//        {
//            if (Control == null || Element == null) return;

//            if (Control.CanGoBack)
//                Control.GoBack();
//        }

//        internal Task<string> OnJavascriptInjectionRequest(string js)
//        {
//            if (Control == null || Element == null)
//                return Task.FromResult(string.Empty);

//            var response = string.Empty;
//            try {
//                response = Control.EvaluateJavascript(js);
//            } catch (Exception exc) {
//                /* The Webview might not be ready... */
//                System.Diagnostics.Debug.WriteLine(exc.Message);
//            }
//            return Task.FromResult(response);
//        }

//        void SetupElement(FormsWebView element)
//        {
//            element.PropertyChanged += OnPropertyChanged;
//            element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequest;
//            element.OnClearCookiesRequested += OnClearCookiesRequest;
//            element.OnAddCookieRequested += OnAddCookieRequested;
//            element.OnBackRequested += OnBackRequested;
//            element.OnForwardRequested += OnForwardRequested;
//            element.OnRefreshRequested += OnRefreshRequested;
//            element.OnPrintCookiesRequested += OnPrintCookiesRequested;

//            SetSource();
//        }

//        void DestroyElement(FormsWebView element)
//        {
//            element.PropertyChanged -= OnPropertyChanged;
//            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequest;
//            element.OnClearCookiesRequested -= OnClearCookiesRequest;
//            element.OnAddCookieRequested -= OnAddCookieRequested;
//            element.OnBackRequested -= OnBackRequested;
//            element.OnForwardRequested -= OnForwardRequested;
//            element.OnRefreshRequested -= OnRefreshRequested;
//            element.OnPrintCookiesRequested -= OnPrintCookiesRequested;

//            element.Dispose();
//        }

//        void SetupControl()
//        {
//            var uiWebView = new UIWebView(Frame);
//            uiWebView.BackgroundColor = UIColor.Black;

//            FormsWebView.CallbackAdded += OnCallbackAdded;

//            uiWebView.LoadError += UiWebView_LoadError;
//            uiWebView.LoadStarted += UiWebView_LoadStarted;
//            uiWebView.LoadFinished += UiWebView_LoadFinished;
//            uiWebView.ShouldStartLoad += UiWebView_ShouldStartLoad;

//            if(uiWebView.ScrollView != null) {
//                uiWebView.ScrollView.Bounces = false;
//            }

//            uiWebView.ContentMode = UIViewContentMode.ScaleAspectFit;
//            uiWebView.AutoresizingMask = UIViewAutoresizing.All;
//            uiWebView.ScalesPageToFit = true;

//            SetNativeControl(uiWebView);
//            OnControlChanged?.Invoke(this, uiWebView);
//        }

//        public override SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
//        {
//            return new SizeRequest(Size.Zero, Size.Zero);
//        }

//        void SetSource()
//        {
//            if (Element == null || Control == null || string.IsNullOrWhiteSpace(Element.Source)) return;

//            switch (Element.ContentType) {
//                case WebViewContentType.Internet:
//                    LoadInternetContent();
//                    break;

//                case WebViewContentType.LocalFile:
//                    LoadLocalFile();
//                    break;

//                case WebViewContentType.StringData:
//                    LoadStringData();
//                    break;
//            }
//        }

//        void LoadStringData()
//        {
//            if (Control == null || Element == null) return;

//            var nsBaseUri = new NSUrl($"file://{Element.BaseUrl ?? BaseUrl}");
//            Control.LoadHtmlString(Element.Source, nsBaseUri);
//        }

//        void LoadLocalFile()
//        {
//            if (Control == null || Element == null) return;
//            // TODO: Not finished
//            var path = Path.Combine(Element.BaseUrl ?? BaseUrl, Element.Source);
//            var nsFileUri = new NSUrl($"file://{path}");
//            var nsBaseUri = new NSUrl($"file://{Element.BaseUrl ?? BaseUrl}");

//            var url = new NSUrl(Element.Source);
//            var request = new NSMutableUrlRequest(url);

//            Control.LoadRequest(request);
//        }

//        void LoadInternetContent()
//        {
//            if (Control == null || Element == null) return;

//            var headers = new NSMutableDictionary();

//            foreach (var header in Element.LocalRegisteredHeaders) {
//                var key = new NSString(header.Key);
//                if (!headers.ContainsKey(key))
//                    headers.Add(key, new NSString(header.Value));
//            }

//            if (Element.EnableGlobalHeaders) {
//                foreach (var header in FormsWebView.GlobalRegisteredHeaders) {
//                    var key = new NSString(header.Key);
//                    if (!headers.ContainsKey(key))
//                        headers.Add(key, new NSString(header.Value));
//                }
//            }

//            /*
//                var cookieDictionary = NSHttpCookie.RequestHeaderFieldsWithCookies(NSHttpCookieStorage.SharedStorage.Cookies);
//                foreach (var item in cookieDictionary) {
//                    headers.SetValueForKey(item.Value, new NSString(item.Key.ToString()));
//                }
//                */

//            var url = new NSUrl(Element.Source);
//            var request = new NSMutableUrlRequest(url) {
//                Headers = headers
//            };

//            Control.LoadRequest(request);
//        }

//        void UiWebView_LoadError(object sender, UIWebErrorArgs e)
//        {
//            if (this.Element == null) return;
//            this.Element.HandleNavigationError(Convert.ToInt32(e.Error.Code));
//        }

//        bool UiWebView_ShouldStartLoad(UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType)
//        {
//            if (this.Element == null) return false;

//            var dh = this.Element.HandleNavigationStartRequest(request.Url.ToString());
//            if (dh.Cancel) {
//                this.Control.StopLoading();
//                return false;
//            }
//            return true;
//        }

//        void UiWebView_LoadStarted(object sender, EventArgs e)
//        {
//            if (this.Element == null) return;
//        }

//        void UiWebView_LoadFinished(object sender, EventArgs e)
//        {
//            if (this.Element == null) return; 

//            this.Element.HandleNavigationCompleted(this.Control.Request.Url.ToString());
//            this.Element.HandleContentLoaded();
//        }
//    }
//}
