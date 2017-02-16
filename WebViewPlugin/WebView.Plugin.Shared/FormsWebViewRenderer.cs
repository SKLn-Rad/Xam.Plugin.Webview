using System;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Shared;
using Xam.Plugin.Shared.Enumerations;
using Xam.Plugin.Shared.Resolvers;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#else
using Xamarin.Forms.Platform.WinRT;
#endif

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace Xam.Plugin.Shared
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Windows.UI.Xaml.Controls.WebView>
    {

        public static event WebViewControlChangedDelegate OnControlChanging;
        public static event WebViewControlChangedDelegate OnControlChanged;

        private LocalFileStreamResolver _resolver;
        private WebViewResourceScheme _resourceScheme = WebViewResourceScheme.ApplicationPackage;

        public static void Init()
        {
            var dt = DateTime.Now;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<FormsWebView> e)
        {
            base.OnElementChanged(e);

            if (Control == null && e.NewElement != null)
                SetupControl(e.NewElement);

            if (e.NewElement != null)
                SetupElement(e.NewElement);

            if (e.OldElement != null)
                DestroyElement(e.OldElement);
        }

        private void SetupControl(FormsWebView element)
        {
            WebViewControlDelegate.OnNavigationRequestedFromUser += OnUserNavigationRequested;
            WebViewControlDelegate.OnInjectJavascriptRequest += InjectJavascript;
            WebViewControlDelegate.OnActionAdded += OnActionAdded;
            var control = new Windows.UI.Xaml.Controls.WebView();
            OnControlChanging?.Invoke(this, Element, control);
            _resolver = new LocalFileStreamResolver();

            SetNativeControl(control);
            OnControlChanged?.Invoke(this, Element, control);
        }

        private async void InjectJavascript(FormsWebView sender, string js)
        {
            if (Element == sender && Control != null)
                await Control.InvokeScriptAsync("eval", new[] { js });
        }

        private void SetupElement(FormsWebView element)
        {
            Control.NavigationStarting += OnNavigating;
            Control.NavigationCompleted += OnNavigated;
            Control.ScriptNotify += OnScriptNotify;

            if (element.Uri != null)
                OnUserNavigationRequested(element, element.Uri, element.ContentType, element.BasePath);
        }

        private void DestroyElement(FormsWebView element)
        {
            if (this != null && Control != null)
            {
                Control.NavigationStarting -= OnNavigating;
                Control.NavigationCompleted -= OnNavigated;
                Control.ScriptNotify -= OnScriptNotify;
            }
        }

        private async void OnActionAdded(FormsWebView sender, string key)
        {
            if (Element == sender && Control != null)
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });
        }

        private async void OnNavigated(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.InjectedFunction });
            foreach (var key in Element.GetAllCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });

            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            Element.SetValue(FormsWebView.UriProperty, uri);

            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, uri)); 
        }

        private void OnNavigating(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            NavigationRequestedDelegate nrd = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, uri));
            args.Cancel = nrd.Cancel;
        }

        private void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            Element.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, e.Value));
        }

        private void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType, string baseUri = "")
        {
            if (sender == Element)
            {
                switch (contentType)
                {
                    case WebViewContentType.Internet:
                        Control.Navigate(new Uri(uri));
                        break;
                    case WebViewContentType.StringData:
                        Control.NavigateToString(uri);
                        break;
                    case WebViewContentType.LocalFile:
                        LoadLocalFile(uri);
                        break;
                }
            }
        }

        private void LoadLocalFile(string uri)
        {
            var luri = Control.BuildLocalStreamUri("/", uri);
            Control.NavigateToLocalStreamUri(luri, _resolver);
        }

        private string GetUriScheme()
        {
            switch (_resourceScheme)
            {
                default:
                case WebViewResourceScheme.ApplicationPackage:
                    return "ms-appx-web:///";
                case WebViewResourceScheme.LocalStorage:
                    return "ms-appdata:///local/";
                case WebViewResourceScheme.RoamingStorage:
                    return "ms-appdata:///roaming/";
                case WebViewResourceScheme.TempStorage:
                    return "ms-appdata:///temp/";
            }
        }
    }
}
