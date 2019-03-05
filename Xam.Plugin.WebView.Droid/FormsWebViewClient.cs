﻿using System;

using Android.Webkit;
using Android.Net.Http;
using Android.Graphics;
using Xam.Plugin.WebView.Abstractions;
using Android.Runtime;
using Android.Content;
using Xamarin.Forms;
using System.Net.Http;
using System.Diagnostics;

namespace Xam.Plugin.WebView.Droid
{
    public class FormsWebViewClient : WebViewClient
    {

        //************************************************************************************************************************
        /// <summary>
        /// Shoulds the intercept request. Raise OnContentTypeLoaded to webview with content type
        /// </summary>
        /// <returns>The intercept request.</returns>
        /// <param name="view">View.</param>
        /// <param name="request">Request.</param>
        public override WebResourceResponse ShouldInterceptRequest(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return null;
            if (renderer.Element == null) return null;

            if (request == null || request.Url == null)
                return null;

            using (var client = new HttpClient()) {
                try
                {
                    var result = client.GetAsync(request.Url.ToString()).Result;
                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var encoding = result.Content.Headers.ContentEncoding.GetEnumerator().Current;

                        string contentType = result?.Content?.Headers?.ContentType?.ToString();

                        if (!string.IsNullOrEmpty(contentType))
                        {
                            renderer.Element.HandleContentTypeLoaded(request.Url.ToString(), contentType);
                            //renderer.Element.Navigating = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"FormsWebViewClient - Exception Message: {ex.Message}");
                }
            }

            return base.ShouldInterceptRequest(view, request);
        }

        //************************************************************************************************************************


        readonly WeakReference<FormsWebViewRenderer> Reference;

        public FormsWebViewClient(FormsWebViewRenderer renderer)
        {
            Reference = new WeakReference<FormsWebViewRenderer>(renderer);
        }

        public override void OnReceivedHttpError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceResponse errorResponse)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError(errorResponse.StatusCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
            renderer.Element.Navigating = false;
        }

        public override void OnReceivedError(Android.Webkit.WebView view, IWebResourceRequest request, WebResourceError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int)error.ErrorCode);
            renderer.Element.HandleNavigationCompleted(request.Url.ToString());
            renderer.Element.Navigating = false;
        }

        //For Android < 5.0
        [Obsolete]
        public override void OnReceivedError(Android.Webkit.WebView view, [GeneratedEnum] ClientError errorCode, string description, string failingUrl)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop) return;

            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.HandleNavigationError((int)errorCode);
            renderer.Element.HandleNavigationCompleted(failingUrl.ToString());
            renderer.Element.Navigating = false;
        }

        [Obsolete]
        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, string url)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
                return base.ShouldOverrideUrlLoading(view, url);

            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer))
                return base.ShouldOverrideUrlLoading(view, url);
            if (renderer.Element == null)
                return base.ShouldOverrideUrlLoading(view, url);

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice) {
                if (response.OffloadOntoDevice)
                    AttemptToHandleCustomUrlScheme(view, url);
                view.StopLoading();
                return true;
            }

            return base.ShouldOverrideUrlLoading(view, url);
        }

        public override bool ShouldOverrideUrlLoading(Android.Webkit.WebView view, IWebResourceRequest request)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer))
                return base.ShouldOverrideUrlLoading(view, request);
            if (renderer.Element == null)
                return base.ShouldOverrideUrlLoading(view, request);

            string url = request.Url.ToString();

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice) {
                if (response.OffloadOntoDevice)
                    AttemptToHandleCustomUrlScheme(view, url);
                view.StopLoading();
                return true;
            }
            return base.ShouldOverrideUrlLoading(view, request);
        }

        void CheckResponseValidity(Android.Webkit.WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            var response = renderer.Element.HandleNavigationStartRequest(url);

            if (response.Cancel || response.OffloadOntoDevice) {
                Device.BeginInvokeOnMainThread(() => {
                    if (response.OffloadOntoDevice)
                        AttemptToHandleCustomUrlScheme(view, url);

                    view.StopLoading();
                });
            }
        }

        public override void OnPageStarted(Android.Webkit.WebView view, string url, Bitmap favicon)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            renderer.Element.Navigating = true;
        }

        bool AttemptToHandleCustomUrlScheme(Android.Webkit.WebView view, string url)
        {
            if (url.StartsWith("mailto")) {
                Android.Net.MailTo emailData = Android.Net.MailTo.Parse(url);

                Intent email = new Intent(Intent.ActionSendto);

                email.SetData(Android.Net.Uri.Parse("mailto:"));
                email.PutExtra(Intent.ExtraEmail, new String[] { emailData.To });
                email.PutExtra(Intent.ExtraSubject, emailData.Subject);
                email.PutExtra(Intent.ExtraCc, emailData.Cc);
                email.PutExtra(Intent.ExtraText, emailData.Body);

                if (email.ResolveActivity(Forms.Context.PackageManager) != null)
                    Forms.Context.StartActivity(email);

                return true;
            }

            if (url.StartsWith("http")) {
                Intent webPage = new Intent(Intent.ActionView, Android.Net.Uri.Parse(url));
                if (webPage.ResolveActivity(Forms.Context.PackageManager) != null)
                    Forms.Context.StartActivity(webPage);

                return true;
            }

            return false;
        }

        public override void OnReceivedSslError(Android.Webkit.WebView view, SslErrorHandler handler, SslError error)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            if (FormsWebViewRenderer.IgnoreSSLGlobally) {
                handler.Proceed();
            } else {
                handler.Cancel();
                renderer.Element.Navigating = false;
            }
        }

        public async override void OnPageFinished(Android.Webkit.WebView view, string url)
        {
            if (Reference == null || !Reference.TryGetTarget(out FormsWebViewRenderer renderer)) return;
            if (renderer.Element == null) return;

            // Add Injection Function
            await renderer.OnJavascriptInjectionRequest(FormsWebView.InjectedFunction);

            // Add Global Callbacks
            if(renderer != null && renderer.Element != null)
            {
                if (renderer.Element.EnableGlobalCallbacks)
                    foreach (var callback in FormsWebView.GlobalRegisteredCallbacks)
                        await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(callback.Key));

                // Add Local Callbacks
                foreach (var callback in renderer.Element.LocalRegisteredCallbacks)
                    await renderer.OnJavascriptInjectionRequest(FormsWebView.GenerateFunctionScript(callback.Key));

                renderer.Element.CanGoBack = view.CanGoBack();
                renderer.Element.CanGoForward = view.CanGoForward();
                renderer.Element.Navigating = false;

                renderer.Element.HandleNavigationCompleted(url);
                renderer.Element.HandleContentLoaded();
            }
        }
    }
}