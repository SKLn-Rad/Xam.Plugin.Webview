using Android.Webkit;
using WebView.Plugin.Abstractions.Events.Inbound;
using WebView.Plugin.Abstractions;
using WebView.Plugin.Abstractions.Events.Outbound;

namespace WebView.Plugin.Droid.Extras
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
            return ((NavigationRequestedDelegate)Renderer._eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, url))).Cancel;
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            return ((NavigationRequestedDelegate) Renderer._eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, request.Url.ToString()))).Cancel;
        }

        public override WebResourceResponse ShouldInterceptRequest(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            return base.ShouldInterceptRequest(view, request);
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            Renderer.InjectJS(WebViewControlDelegate.InjectedFunction);
            Renderer._eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, url, true));
            base.OnPageFinished(view, url);
        }

    }
}