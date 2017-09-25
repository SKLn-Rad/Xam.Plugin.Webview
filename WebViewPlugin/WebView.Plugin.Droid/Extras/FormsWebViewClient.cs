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

        FormsWebViewRenderer Renderer { get; set; }

        public FormsWebViewClient(FormsWebView element, FormsWebViewRenderer renderer)
        {
            Renderer = renderer;
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            if (Renderer.Element == null) return;
            Renderer.Element.InvokeEvent(WebViewEventType.NavigationError, new NavigationErrorDelegate(Renderer.Element, errorResponse.StatusCode));
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            if (Renderer.Element == null) return;

            var request = (NavigationRequestedDelegate) Renderer.Element.InvokeEvent(WebViewEventType.NavigationRequested,
                new NavigationRequestedDelegate(Renderer.Element, url));

            if (request?.Cancel ?? false)
                view.StopLoading();
            else
                Renderer.Element.SetValue(FormsWebView.SourceProperty, url);
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
            if (Renderer.Element == null) return;

            Renderer.Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Renderer.Element, url, true));
            Renderer.InjectJavascript(FormsWebView.InjectedFunction);

            foreach (var key in FormsWebView.GetGlobalCallbacks())
                Renderer.InjectJavascript(FormsWebView.GenerateFunctionScript(key));

            foreach (var key in Renderer.Element.GetLocalCallbacks())
                Renderer.InjectJavascript(FormsWebView.GenerateFunctionScript(key));

            Renderer.Element.InvokeEvent(WebViewEventType.NavigationStackUpdate, new NavigationStackUpdateDelegate(Renderer.Element, Renderer.Control.CanGoBack(), Renderer.Control.CanGoForward()));
            Renderer.Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Renderer.Element, url));
            base.OnPageFinished(view, url);
        }
    }
}