using System;
using WebView.Plugin.Abstractions;
using WebView.Plugin.Abstractions.Enumerations;
using WebView.Plugin.Shared;
using WebView.Plugin.Shared.Enumerations;
using WebView.Plugin.Shared.Resolvers;
using WebView.Plugin.Abstractions.Inbound;
using WebView.Plugin.Abstractions.Events.Inbound;
using WebView.Plugin.Abstractions.Events.Outbound;
using static WebView.Plugin.Abstractions.Events.Inbound.WebViewDelegate;

#if WINDOWS_UWP
using Xamarin.Forms.Platform.UWP;
#else
using Xamarin.Forms.Platform.WinRT;
#endif

[assembly: ExportRenderer(typeof(FormsWebView), typeof(FormsWebViewRenderer))]
namespace WebView.Plugin.Shared
{
    public class FormsWebViewRenderer : ViewRenderer<FormsWebView, Windows.UI.Xaml.Controls.WebView>
    {

        internal WebViewEventAbstraction _eventAbstraction;

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
            WebViewControlDelegate.ObtainUri += ReturnUriToUser;
            WebViewControlDelegate.OnInjectJavascriptRequest += InjectJavascript;

            _eventAbstraction = new WebViewEventAbstraction() { Source = new WebViewEventStub() };
            
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

        private string ReturnUriToUser(FormsWebView sender)
        {
            if (sender == Element && Control != null)
                return Control.Source != null ? Control.Source.AbsoluteUri : "";
            return "";
        }

        private void SetupElement(FormsWebView element)
        {
            Control.NavigationStarting += OnNavigating;
            Control.NavigationCompleted += OnNavigated;
            Control.ScriptNotify += OnScriptNotify;
        }

        private void DestroyElement(FormsWebView element)
        {
            Control.NavigationStarting -= OnNavigating;
            Control.NavigationCompleted -= OnNavigated;
            Control.ScriptNotify -= OnScriptNotify;
        }

        private async void OnNavigated(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationCompletedEventArgs args)
        {
            await Control.InvokeScriptAsync("eval", new[] { WebViewControlDelegate.InjectedFunction });

            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            _eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationComplete, new NavigationCompletedDelegate(Element, uri));    
        }

        private void OnNavigating(Windows.UI.Xaml.Controls.WebView sender, Windows.UI.Xaml.Controls.WebViewNavigationStartingEventArgs args)
        {
            var uri = args.Uri != null ? args.Uri.AbsoluteUri : "";
            NavigationRequestedDelegate nrd = (NavigationRequestedDelegate) _eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.NavigationRequested, new NavigationRequestedDelegate(Element, uri));
            args.Cancel = nrd.Cancel;
        }

        private void OnScriptNotify(object sender, Windows.UI.Xaml.Controls.NotifyEventArgs e)
        {
            _eventAbstraction.Target.InvokeEvent(Element, WebViewEventType.JavascriptCallback, new JavascriptResponseDelegate(Element, e.Value));
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
