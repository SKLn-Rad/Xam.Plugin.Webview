using Android.Webkit;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Outbound;
using WebView.Plugin.Abstractions.Events.Inbound;
using Android.Graphics;

namespace Xam.Plugin.Droid.Extras
{
    public class FormsWebViewClient : WebViewClient
    {

        private FormsWebView Element { get; set; }
        private FormsWebViewRenderer Renderer { get; set; }

        public FormsWebViewClient(FormsWebView element, FormsWebViewRenderer renderer)
        {
            Element = element;
            Renderer = renderer;
        }

        public override void OnLoadResource(Android.Webkit.WebView view, string url)
        {
            base.OnLoadResource(view, url);
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
        {
            return ((NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, url))).Cancel;
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            return ((NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, request.Url.ToString()))).Cancel;
        }

        public override WebResourceResponse ShouldInterceptRequest(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            return base.ShouldInterceptRequest(view, request);
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            Element.SetValue(FormsWebView.SourceProperty, url);

            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, url, true));
            base.OnPageStarted(view, url, favicon);
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);
            foreach (var key in Element.GetAllCallbacks())
                Renderer.InjectJS(WebViewControlDelegate.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, url));
            base.OnPageFinished(view, url);
        }
    }
}