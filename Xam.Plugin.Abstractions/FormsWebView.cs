using System;
using System.Collections.Generic;
using Xam.Plugin.Abstractions.Delegates;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions
{
    public partial class FormsWebView : View, IFormsWebView, IDisposable
    {

        public readonly Dictionary<string, Action<string>> LocalRegisteredCallbacks = new Dictionary<string, Action<string>>();
        public readonly Dictionary<string, string> LocalRegisteredHeaders = new Dictionary<string, string>();

        public event EventHandler<DecisionHandlerDelegate> OnNavigationStarted;

        public event EventHandler OnNavigationCompleted;

        public event EventHandler<int> OnNavigationError;

        public event EventHandler OnContentLoaded;

        public WebViewContentType ContentType
        {
            get => (WebViewContentType)GetValue(ContentTypeProperty);
            set => SetValue(ContentTypeProperty, value);
        }

        public string Source
        {
            get => (string) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public string BaseUrl
        {
            get { return (string)GetValue(BaseUrlProperty); }
            set { SetValue(BaseUrlProperty, value); }
        }

        public bool EnableGlobalCallbacks
        {
            get => (bool) GetValue(EnableGlobalCallbacksProperty);
            set => SetValue(EnableGlobalCallbacksProperty, value);
        }

        public bool EnableGlobalHeaders
        {
            get => (bool) GetValue(EnableGlobalHeadersProperty);
            set => SetValue(EnableGlobalHeadersProperty, value);
        }

        public bool Navigating
        {
            get => (bool)GetValue(NavigatingProperty);
            internal set => SetValue(NavigatingProperty, value);
        }

        public bool CanGoBack
        {
            get => (bool) GetValue(CanGoBackProperty);
            internal set => SetValue(CanGoBackProperty, value);
        }

        public bool CanGoForward
        {
            get => (bool) GetValue(CanGoForwardProperty);
            internal set => SetValue(CanGoForwardProperty, value);
        }

        public FormsWebView()
        {
            HorizontalOptions = VerticalOptions = LayoutOptions.FillAndExpand;
        }

        public void InjectJavascript(string js)
        {

        }

        public string EvaluateJavascriptAsync(string js)
        {
            return null;
        }

        public void Dispose()
        {
            // TODO
        }

        // All code which should be hidden from the end user goes here
        #region Internals



        #endregion
    }
}
