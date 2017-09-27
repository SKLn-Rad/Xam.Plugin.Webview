using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Foundation;
using WebKit;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: Xamarin.Forms.ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.iOS
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WKWebView>, IWKScriptMessageHandler, IWKUIDelegate
    {

        public event EventHandler<WKWebView> OnControlChanged;

        public static string BaseUrl { get; set; } = NSBundle.MainBundle.BundlePath;

        FormsNavigationDelegate _navigationDelegate;

        WKWebViewConfiguration _configuration;

        WKUserContentController _contentController;

        public static void Initialize() {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                SetupControl();

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);
        }

		void SetupElement(FormsWebView element)
		{
            element.PropertyChanged += OnPropertyChanged;
            element.OnJavascriptInjectionRequest += OnJavascriptInjectionRequest;

            SetSource();
		}

        void DestroyElement(FormsWebView element)
        {
            element.PropertyChanged -= OnPropertyChanged;
            element.OnJavascriptInjectionRequest -= OnJavascriptInjectionRequest;

            element.Dispose();
        }

        void SetupControl()
        {
            _navigationDelegate = new FormsNavigationDelegate(this);
            _contentController = new WKUserContentController();
            _contentController.AddScriptMessageHandler(this, "invokeAction");
            _configuration = new WKWebViewConfiguration {
                UserContentController = _contentController
            };

            var wkWebView = new WKWebView(Frame, _configuration)
            {
                Opaque = false,
                UIDelegate = this,
                NavigationDelegate = _navigationDelegate
            };

            SetNativeControl(wkWebView);
            OnControlChanged?.Invoke(this, wkWebView);
        }

		void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            switch (e.PropertyName) {
                case "Source":
                    SetSource();
                    break;
            }
		}

		internal async Task<string> OnJavascriptInjectionRequest(string js)
		{
            if (Control == null || Element == null) return string.Empty;

            var response = string.Empty;
            var obj = await Control.EvaluateJavaScriptAsync(js);

            if (obj != null)
                response = obj.ToString();

            return response;
		}

        void SetSource()
        {
            if (Element == null || Control == null) return;

            switch (Element.ContentType) {
                case WebViewContentType.Internet:
                    LoadInternetContent();
                    break;

                case WebViewContentType.LocalFile:
                    LoadLocalFile();
                    break;

                case WebViewContentType.StringData:
                    LoadStringData();
                    break;
            }
        }

        void LoadStringData()
        {
            if (Control == null || Element == null) return;

            var nsBaseUri = new NSUrl($"file://{Element.BaseUrl ?? BaseUrl}");
            Control.LoadHtmlString(Element.Source, nsBaseUri);
        }

        void LoadLocalFile()
        {
            if (Control == null || Element == null) return;

            var path = Path.Combine(Element.BaseUrl ?? BaseUrl, Element.Source);
            var nsFileUri = new NSUrl($"file://{path}");
            var nsBaseUri = new NSUrl($"file://{Element.BaseUrl ?? BaseUrl}");

            Control.LoadFileUrl(nsFileUri, nsBaseUri);
        }

        void LoadInternetContent()
        {
            if (Control == null || Element == null) return;

            // Get Headers

            var url = new NSUrl(Element.Source);
            var request = new NSMutableUrlRequest(url);

            Control.LoadRequest(request);
        }

        public void DidReceiveScriptMessage(WKUserContentController userContentController, WKScriptMessage message)
        {
            if (Element == null || message == null || message.Body == null) return;
            Element.HandleScriptReceived(message.Body.ToString());
        }
    }
}
