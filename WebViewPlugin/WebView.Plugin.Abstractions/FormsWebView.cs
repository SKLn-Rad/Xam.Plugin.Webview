using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions
{
    public class FormsWebView : View
    {

        public BindableProperty NavigatingProperty = BindableProperty.Create(nameof(Navigating), typeof(bool), typeof(FormsWebView), false);
        public BindableProperty UriProperty = BindableProperty.Create(nameof(Uri), typeof(string), typeof(FormsWebView), null);
        public BindableProperty BasePathProperty = BindableProperty.Create(nameof(BasePath), typeof(string), typeof(FormsWebView), "");
        public BindableProperty ContentTypeProperty = BindableProperty.Create(nameof(ContentType), typeof(WebViewContentType), typeof(FormsWebView), WebViewContentType.Internet);

        public bool Navigating
        {
            get { return (bool) GetValue(NavigatingProperty); }
            set { SetValue(NavigatingProperty, value); }
        }

        public string Uri
        {
            get { return (string) GetValue(UriProperty); }
            set { SetValue(UriProperty, value); _controlEventAbstraction.Target.PerformNavigation(this, value, ContentType, BasePath);}
        }

        public string BasePath
        {
            get { return (string)GetValue(BasePathProperty); }
            set { SetValue(BasePathProperty, value); }
        }

        public WebViewContentType ContentType
        {
            get { return (WebViewContentType) GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }

        private WebViewControlEventAbstraction _controlEventAbstraction;
        
        public delegate NavigationRequestedDelegate WebViewNavigationStartedEventArgs(NavigationRequestedDelegate eventObj);
        public event WebViewNavigationStartedEventArgs NavigationStarted;

        public delegate void WebViewNavigationCompletedEventArgs(NavigationCompletedDelegate eventObj);
        public event WebViewNavigationCompletedEventArgs NavigationCompleted;

        public delegate void JavascriptResponseEventArgs(JavascriptResponseDelegate eventObj);
        public event JavascriptResponseEventArgs OnJavascriptResponse;

        public FormsWebView()
        {
            _controlEventAbstraction = new WebViewControlEventAbstraction() { Source = new WebViewControlEventStub() };
        }

        public void InjectJavascript(string js)
        {
            _controlEventAbstraction.Target.InjectJavascript(this, js);
        }

        /// <summary>
        /// Internal Use Only.
        /// </summary>
        /// <param name="type">The type of event</param>
        /// <param name="eventObject">The WVE object to pass</param>
        /// <returns></returns>
        public object InvokeEvent(WebViewEventType type, WebViewDelegate eventObject)
        {
            switch (type)
            {
                case WebViewEventType.NavigationRequested:
                    return NavigationStarted == null ? eventObject as NavigationRequestedDelegate : NavigationStarted.Invoke(eventObject as NavigationRequestedDelegate);

                case WebViewEventType.NavigationComplete:
                    NavigationCompleted?.Invoke(eventObject as NavigationCompletedDelegate);
                    break;

                case WebViewEventType.JavascriptCallback:
                    OnJavascriptResponse?.Invoke(eventObject as JavascriptResponseDelegate);
                    break;
            }

            return eventObject;
        }
    }
}
