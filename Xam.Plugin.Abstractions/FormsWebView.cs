using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xam.Plugin.Abstractions.Delegates;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

[assembly:InternalsVisibleTo("Xam.Plugin.UWP")]
namespace Xam.Plugin.Abstractions
{
    public partial class FormsWebView : View, IFormsWebView, IDisposable
    {

        internal readonly Dictionary<string, Action<string>> LocalRegisteredCallbacks = new Dictionary<string, Action<string>>();
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

        public void AddLocalCallback(string functionName, Action<string> action)
        {
            throw new NotImplementedException();
        }

        public void RemoveLocalCallback(string functionName)
        {
            throw new NotImplementedException();
        }

        public void RemoveAllLocalCallbacks()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            LocalRegisteredCallbacks.Clear();
            LocalRegisteredHeaders.Clear();
        }

        // All code which should be hidden from the end user goes here
        #region Internals

        internal DecisionHandlerDelegate HandleNavigationStartRequest(string uri)
        {
            var handler = new DecisionHandlerDelegate() { Uri = uri };
            OnNavigationStarted?.Invoke(this, handler);
            return handler;
        }

        internal void HandleNavigationCompleted()
        {
            OnNavigationCompleted?.Invoke(this, EventArgs.Empty);
        }

        internal void HandleNavigationError(int errorCode)
        {
            OnNavigationError?.Invoke(this, errorCode);
        }

        internal void HandleContentLoaded()
        {
            OnContentLoaded?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
