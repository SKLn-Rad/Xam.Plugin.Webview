using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xam.Plugin.WebView.Abstractions.Delegates;
using Xam.Plugin.WebView.Abstractions.Enumerations;
using Xam.Plugin.WebView.Abstractions.Models;
using Xamarin.Forms;

[assembly: InternalsVisibleTo("Xam.Plugin.WebView.UWP")]
[assembly: InternalsVisibleTo("Xam.Plugin.WebView.Droid")]
[assembly: InternalsVisibleTo("Xam.Plugin.WebView.iOS")]
[assembly: InternalsVisibleTo("Xam.Plugin.WebView.MacOS")]
namespace Xam.Plugin.WebView.Abstractions
{
    public partial class FormsWebView : View, IFormsWebView, IDisposable
    {

        /// <summary>
        /// A delegate which takes valid javascript and returns the response from it, if the response is a string.
        /// </summary>
        /// <param name="js">The valid JS to inject</param>
        /// <returns>Any string response from the DOM or string.Empty</returns>
        public delegate Task<string> JavascriptInjectionRequestDelegate(string js);
        
        /// <summary>
        /// Fired when navigation begins, for example when the source is set.
        /// </summary>
        public event EventHandler<DecisionHandlerDelegate> OnNavigationStarted;

        /// <summary>
        /// Fires when navigation is completed. This can be either as the result of a valid navigation, or on an error.
        /// </summary>
        public event EventHandler OnNavigationCompleted;

        /// <summary>
        /// Fires when navigation fires an error. By default this uses the native systems error codes.
        /// </summary>
        public event EventHandler<int> OnNavigationError;

        /// <summary>
        /// Fires when the content on the DOM is ready. All your calls to Javascript using C# should be performed after this is fired.
        /// </summary>
        public event EventHandler OnContentLoaded;

        internal event EventHandler OnBackRequested;

        internal event EventHandler OnForwardRequested;

        internal event EventHandler OnRefreshRequested;

        internal event JavascriptInjectionRequestDelegate OnJavascriptInjectionRequest;

        internal readonly Dictionary<string, Action<string>> LocalRegisteredCallbacks = new Dictionary<string, Action<string>>();

        /// <summary>
        /// A dictionary containing all headers to be injected into the request. Local headers take precedence over global ones.
        /// </summary>
        public readonly Dictionary<string, string> LocalRegisteredHeaders = new Dictionary<string, string>();

        /// <summary>
        /// The content type to attempt to load. By default this is Internet.
        /// </summary>
        public WebViewContentType ContentType
        {
            get => (WebViewContentType)GetValue(ContentTypeProperty);
            set => SetValue(ContentTypeProperty, value);
        }

        /// <summary>
        /// The source data. This can either be a valid URL, a path to a local file, or a HTML string.
        /// </summary>
        public string Source
        {
            get => (string) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        /// <summary>
        /// Override the BaseURL in the renderer with this property.
        /// By default, the BaseUrls are the following:
        /// Android) Assets folder with AndroidAsset build property
        /// iOS and MacOS) Resources folder with BundleResource build property
        /// UWP) Project folder with content build property
        /// </summary>
        public string BaseUrl
        {
            get { return (string)GetValue(BaseUrlProperty); }
            set { SetValue(BaseUrlProperty, value); }
        }

        /// <summary>
        /// Opt in and out of global callbacks
        /// </summary>
        public bool EnableGlobalCallbacks
        {
            get => (bool) GetValue(EnableGlobalCallbacksProperty);
            set => SetValue(EnableGlobalCallbacksProperty, value);
        }

        /// <summary>
        /// Opt in and out of global headers
        /// </summary>
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

        public void GoBack()
        {
            if (!CanGoBack) return;
            OnBackRequested?.Invoke(this, EventArgs.Empty);
        }

        public void GoForward()
        {
            if (!CanGoForward) return;
            OnForwardRequested?.Invoke(this, EventArgs.Empty);
        }

        public void Refresh()
        {
            OnRefreshRequested?.Invoke(this, EventArgs.Empty);
        }

        public async Task<string> InjectJavascriptAsync(string js)
        {
            if (string.IsNullOrWhiteSpace(js)) return string.Empty;

            if (OnJavascriptInjectionRequest != null)
                return await OnJavascriptInjectionRequest.Invoke(js);

            return string.Empty;
        }

        public void AddLocalCallback(string functionName, Action<string> action)
        {
            if (string.IsNullOrWhiteSpace(functionName)) return;

            if (LocalRegisteredCallbacks.ContainsKey(functionName))
                LocalRegisteredCallbacks.Remove(functionName);

            LocalRegisteredCallbacks.Add(functionName, action);
            CallbackAdded?.Invoke(this, functionName);
        }

        public void RemoveLocalCallback(string functionName)
        {
            if (LocalRegisteredCallbacks.ContainsKey(functionName))
                LocalRegisteredCallbacks.Remove(functionName);
        }

        public void RemoveAllLocalCallbacks()
        {
            LocalRegisteredCallbacks.Clear();
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

        internal void HandleScriptReceived(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return;
            
            var action = JsonConvert.DeserializeObject<ActionEvent>(data);

            // Local takes priority
            if (LocalRegisteredCallbacks.ContainsKey(action.Action))
                LocalRegisteredCallbacks[action.Action]?.Invoke(action.Data);

            // Global is checked if local fails
            else if (GlobalRegisteredCallbacks.ContainsKey(action.Action))
                GlobalRegisteredCallbacks[action.Action]?.Invoke(action.Data);
        }

        #endregion
    }
}
