using System;

using Android.Webkit;
using Android.Net.Http;
using Android.Graphics;
using Xam.Plugin.Abstractions;
using Android.Runtime;

namespace Xam.Plugin.Droid
{
    public class FormsWebViewClient : WebViewClient
    {

        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewClient(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public override void OnReceivedHttpError(WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError(errorResponse.StatusCode);
            renderer.Element.Navigating = false;
        }

        public override void OnReceivedError(WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int) error.ErrorCode);
            renderer.Element.Navigating = false;
        }

        public override void OnPageStarted(WebView view, string url, Bitmap favicon)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel)
                view.StopLoading();

            else
                renderer.Element.Navigating = true;
        }

        public override void OnReceivedSslError(WebView view, SslErrorHandler handler, SslError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            if (FormsWebViewRenderer.IgnoreSSLGlobally)
            {
                handler.Proceed();
            }

            else
            {
                handler.Cancel();
                renderer.Element.Navigating = false;
            }
        }

        public async override void OnPageFinished(WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            // Add Injection Function
            await renderer.OnJavascriptInjectionRequest(FormsWebView.InjectedFunction);

            // Add Global Callbacks
            foreach (var callback in FormsWebView.GlobalRegisteredCallbacks)
                await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(callback.Key));

            // Add Local Callbacks
            foreach (var callback in renderer.Element.LocalRegisteredCallbacks)
                await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(callback.Key));

            renderer.Element.CanGoBack = view.CanGoBack();
            renderer.Element.CanGoForward = view.CanGoForward();
            renderer.Element.Navigating = false;
        }
    }
}