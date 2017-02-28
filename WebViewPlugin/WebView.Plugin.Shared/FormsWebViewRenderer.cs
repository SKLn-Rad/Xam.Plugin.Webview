using System;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Shared;
using Xam.Plugin.Shared.Resolvers;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using static Xam.Plugin.Abstractions.Events.Inbound.WebViewDelegate;
using WebView.Plugin.Abstractions.Events.Inbound;
using Xamarin.Forms;

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

        public string BaseUrl { get; set; } = "ms-appx:///";

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
            _resolver = new LocalFileStreamResolver(this);

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
            Control.DOMContentLoaded += OnContentLoaded;
            Control.ScriptNotify += OnScriptNotify;

            if (element.Source != null)
                OnUserNavigationRequested(element, element.Source, element.ContentType);
        }

        private void DestroyElement(FormsWebView element)
        {
            if (this != null && Control != null)
            {
                Control.NavigationStarting -= OnNavigating;
                Control.NavigationCompleted -= OnNavigated;
                Control.DOMContentLoaded -= OnContentLoaded;
                Control.ScriptNotify -= OnScriptNotify;
            }
        }

        private async void OnActionAdded(FormsWebView sender, string key)
        {
            if (Element == sender && Control != null)
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });
        }

        private void OnNavigated(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            Element.SetValue(FormsWebView.SourceProperty, uri);
            
            Element.InvokeEvent(WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, uri, args.IsSuccess)); 
        }

        private void OnNavigating(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            NavigationRequestedDelegate nrd = (NavigationRequestedDelegate) Element.InvokeEvent(WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, uri));
            args.Cancel = nrd.Cancel;
        }

        private async void OnContentLoaded(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewDOMContentLoadedEventArgs args)
        {
            await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.InjectedFunction });
            foreach (var key in Element.GetAllCallbacks())
                await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.GenerateFunctionScript(key) });

            Element.InvokeEvent(WebViewEventType.ContentLoaded, new ContentLoadedDelegate(Element, args.Uri != null ? args.Uri.AbsoluteUri : ""));
        }

        private void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            Element.InvokeEvent(WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, e.Value));
        }

        private void OnUserNavigationRequested(FormsWebView sender, string uri, WebViewContentType contentType)
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
            Control.NavigateToLocalStreamUri(Control.BuildLocalStreamUri("/", uri), _resolver);
        }
    }
}
