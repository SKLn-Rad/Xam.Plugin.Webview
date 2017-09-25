using System;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.UWP;
using Xamarin.Forms.Platform.UWP;

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.UWP
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, WebView>
    {

        public static event EventHandler<WebView> OnControlChanging;
        public static event EventHandler<WebView> OnControlChanged;

        public static string BaseUrl { get; set; } = "ms-appx:///";

        public static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
                SetupControl();

            if (e.NewElement != null)
                SetupNewElement(e.NewElement);

            if (e.OldElement != null)
                DestroyOldElement(e.OldElement);
        }

        void SetupNewElement(FormsWebView element)
        {
            element.PropertyChanged += OnWebViewPropertyChanged;

            ReloadElement();
        }

        void DestroyOldElement(FormsWebView element)
        {
            element.PropertyChanged -= OnWebViewPropertyChanged;
            element.Dispose();
        }

        void SetupControl()
        {
            var control = new WebView();
            OnControlChanging?.Invoke(this, control);
            SetNativeControl(control);

            Control.NavigationStarting += OnNavigationStarting;

            OnControlChanged?.Invoke(this, control);
        }

        void OnWebViewPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Source":
                    SetSource();
                    break;
            }
        }

        void OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {

        }

        void ReloadElement()
        {
            if (Element == null) return;

            SetSource();
            SetBackgroundColor();
        }

        void SetSource()
        {
            if (Element == null || Control == null || Element.Source == null) return;
            
            switch (Element.ContentType)
            {
                case WebViewContentType.Internet:
                    NavigateWithHttpRequest(new Uri(Element.Source));
                    break;
                case WebViewContentType.StringData:
                    LoadStringData(Element.Source);
                    break;
                case WebViewContentType.LocalFile:
                    LoadLocalFile(Element.Source);
                    break;
            }
        }

        void NavigateWithHttpRequest(Uri uri)
        {
            if (Element == null || Control == null) return;

            var requestMsg = new HttpRequestMessage(HttpMethod.Get, uri);

            // Add Global Headers
            foreach (var header in FormsWebView.GlobalRegisteredHeaders)
                requestMsg.Headers.Add(header.Key, header.Value);

            // Add Local Headers
            foreach (var header in Element.LocalRegisteredHeaders)
                requestMsg.Headers.Add(header.Key, header.Value);

            // Navigate
            Control.NavigateWithHttpRequestMessage(requestMsg);
        }

        void LoadLocalFile(string source)
        {
            throw new NotImplementedException();
        }

        void LoadStringData(string source)
        {
            throw new NotImplementedException();
        }

        void SetBackgroundColor()
        {
            if (Element == null || Control == null || Element.BackgroundColor == null) return;
            Control.DefaultBackgroundColor = ToWindowsColor(Element.BackgroundColor);
        }

        Windows.UI.Color ToWindowsColor(Xamarin.Forms.Color color)
        {
            // Make colour safe for Windows
            if (color.A == -1 || color.R == -1 || color.G == -1 || color.B == -1)
                color = Xamarin.Forms.Color.Transparent;

            return Windows.UI.Color.FromArgb(Convert.ToByte(color.A * 255), Convert.ToByte(color.R * 255), Convert.ToByte(color.G * 255), Convert.ToByte(color.B * 255));
        }
    }
}
