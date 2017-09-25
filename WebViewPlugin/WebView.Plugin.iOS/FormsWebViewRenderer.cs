using System;
using WebKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Foundation;
using System.IO;
using System.Linq;
using Xam.Plugin.Abstractions;
using Xam.Plugin.iOS.Extras;
using Xam.Plugin.Abstractions.Events.Inbound;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using Xam.Plugin.iOS;
using UIKit;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.iOS
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WKWebView>, IWKScriptMessageHandler, IWKUIDelegate
    {

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;

        private WKUserContentController UserController;
        private WKWebViewConfiguration WebViewConfiguration;
        private FormsWKNavigationDelegate NavigationDelegate;

        public static string BaseUrl { get; set; } = NSBundle.MainBundle.BundlePath;

        public new static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                SetupControl(e.NewElement);

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);
        }

        void SetupControl(FormsWebView element)
        {
            if (element.EnableGlobalCallbacks)
                FormsWebView.OnGlobalActionAdded += OnActionAdded;

            UserController = new WKUserContentController();
            UserController.AddScriptMessageHandler(this, "invokeAction");

            NavigationDelegate = new FormsWKNavigationDelegate(this, element);
            WebViewConfiguration = new WKWebViewConfiguration { UserContentController = UserController };

            var webView = new WKWebView(Frame, WebViewConfiguration)
            {
                Opaque = false,
                UIDelegate = this,
                NavigationDelegate = NavigationDelegate
            };

            OnControlChanging?.Invoke(this, Element, Control);
            SetNativeControl(webView);
            OnControlChanged?.Invoke(this, Element, Control);
        }

        void OnActionAdded(string key)
        {
            InjectJS(FormsWebView.GenerateFunctionScript(key));
        }

        void OnInjectJavascriptRequested(string js)
        {
            InjectJS(js);
        }

        void OnStackNavigationRequested(bool forward)
        {
            if (forward)
                Control.GoForward();
            else
                Control.GoBack();
        }

        void DestroyElement(FormsWebView element)
        {
            element.Destroy();

            element.PropertyChanged -= OnWebViewPropertyChanged;
            element.OnNavigationRequestedFromUser -= OnUserNavigationRequested;
            element.OnInjectJavascriptRequest -= OnInjectJavascriptRequested;
            element.OnStackNavigationRequested -= OnStackNavigationRequested;
            element.OnLocalActionAdded -= OnActionAdded;
        }

        void SetupElement(FormsWebView element)
        {
            element.PropertyChanged += OnWebViewPropertyChanged;
            element.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            element.OnInjectJavascriptRequest += OnInjectJavascriptRequested;
            element.OnStackNavigationRequested += OnStackNavigationRequested;
            element.OnLocalActionAdded += OnActionAdded;

            SetWebViewBackgroundColor(element.BackgroundColor);

            if (element.Source != null)
                OnUserNavigationRequested(element.Source, element.ContentType);
        }

        void OnWebViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(FormsWebView.BackgroundColor)))
                SetWebViewBackgroundColor(((FormsWebView)sender).BackgroundColor);
        }

        void SetWebViewBackgroundColor(Color backgroundColor)
        {
            if (Control != null)
            {
                InvokeOnMainThread(() =>
                {
                    Control.BackgroundColor = backgroundColor.ToUIColor();
                    Element.BackgroundColor = backgroundColor;
                });
            }
        }

        internal void InjectJS(string js)
        {
            if (Control != null)
                InvokeOnMainThread(async () => await Control.EvaluateJavaScriptAsync(new NSString(js)));
        }

        void OnUserNavigationRequested(string uri, Abstractions.Enumerations.WebViewContentType contentType)
        {
            switch (contentType)
            {
                case Abstractions.Enumerations.WebViewContentType.Internet:
                    CommitNsUrlRequest(new NSUrl(uri));
                    break;
                case Abstractions.Enumerations.WebViewContentType.LocalFile:
                    LoadLocalContent(uri);
                    break;
                case Abstractions.Enumerations.WebViewContentType.StringData:
                    Control.LoadHtmlString(uri, new NSUrl(string.Concat("file://", Element.BaseUrl ?? BaseUrl, "/")));
                    break;
            }
        }

        void CommitNsUrlRequest(NSUrl nSUrl)
        {
            var headers = new NSMutableDictionary();
            foreach (var header in Element.RequestHeaders)
                headers.Add(new NSString(header.Key), new NSString(header.Value));

            var request = new NSMutableUrlRequest(nSUrl) { Headers = headers };

            Control.LoadRequest(request);
        }

        void LoadLocalContent(string uri)
        {
            var path = Path.Combine(Element.BaseUrl ?? BaseUrl, uri);
            Control.LoadFileUrl(new NSUrl(string.Concat("file://", path)), new NSUrl(string.Concat("file://", Element.BaseUrl ?? BaseUrl)));
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            Element?.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, message.Body.ToString()));
        }

        /**
         * UI Delegate methods from: https://developer.xamarin.com/recipes/ios/content_controls/web_view/handle_javascript_alerts/
         */

        [Foundation.Export("webView:runJavaScriptAlertPanelWithMessage:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptAlertPanel(WebKit.WKWebView webView, string message, WKFrameInfo frame, Action completionHandler)
        {
            var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);

            completionHandler();
        }

        [Export("webView:runJavaScriptConfirmPanelWithMessage:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptConfirmPanel(WKWebView webView, string message, WKFrameInfo frame, Action<bool> completionHandler)
        {
            var alertController = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);

            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction => {
                completionHandler(true);
            }));

            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction => {
                completionHandler(false);
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }

        [Foundation.Export("webView:runJavaScriptTextInputPanelWithPrompt:defaultText:initiatedByFrame:completionHandler:")]
        public void RunJavaScriptTextInputPanel(WebKit.WKWebView webView, string prompt, string defaultText, WebKit.WKFrameInfo frame, System.Action<string> completionHandler)
        {
            var alertController = UIAlertController.Create(null, prompt, UIAlertControllerStyle.Alert);

            UITextField alertTextField = null;
            alertController.AddTextField(textField => {
                textField.Placeholder = defaultText;
                alertTextField = textField;
            });

            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, okAction => {
                completionHandler(alertTextField.Text);
            }));

            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, cancelAction => {
                completionHandler(null);
            }));

            UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(alertController, true, null);
        }
    }
}
