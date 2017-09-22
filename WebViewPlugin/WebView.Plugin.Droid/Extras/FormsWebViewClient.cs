using Android.Webkit;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions;
using Android.Graphics;
using WebView.Plugin.Abstractions.Events.Inbound;
using Android.Net.Http;

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

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Element, errorResponse.StatusCode));
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            var request = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested,
                new NavigationRequestedDelegate(Element, url));

            if (request?.Cancel ?? false)
                view.StopLoading();
            else
                Element.SetValue(FormsWebView.SourceProperty, url);
        }

        public override void OnReceivedSslError(Android.Webkit.WebView view, SslErrorHandler handler, SslError error)
        {
            if (FormsWebViewRenderer.IgnoreSslGlobally)
                handler.Proceed();
            else
                handler.Cancel();
        }

        public override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, url, true));
            Renderer.InjectJavascript(FormsWebView.InjectedFunction);

            foreach (var key in FormsWebView.GetGlobalCallbacks())
                Renderer.InjectJavascript(FormsWebView.GenerateFunctionScript(key));

            foreach (var key in Element.GetLocalCallbacks())
                Renderer.InjectJavascript(FormsWebView.GenerateFunctionScript(key));

            Element.InvokeEvent(WebViewEventType.NavigationStackUpdate, new NavigationStackUpdateDelegate(Element, Renderer.Control.CanGoBack(), Renderer.Control.CanGoForward()));
            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, url));
            base.OnPageFinished(view, url);
        }
    }
}