using System.Text;
using WebView.Plugin.Abstractions.Enumerations;
using WebView.Plugin.Abstractions.Events.Inbound;
using WebView.Plugin.Abstractions.Events.Outbound;
using Xamarin.Forms;

namespace WebView.Plugin.Abstractions
{
    public class FormsWebView : View
    {
        private WebViewControlEventAbstraction _controlEventAbstraction;
        
        public delegate NavigationRequestedDelegate WebViewNavigationStartedEventArgs(NavigationRequestedDelegate eventObj);
        public static event WebViewNavigationStartedEventArgs NavigationStarted;

        public delegate void WebViewNavigationCompletedEventArgs(NavigationCompletedDelegate eventObj);
        public static event WebViewNavigationCompletedEventArgs NavigationCompleted;

        public delegate void JavascriptResponseEventArgs(JavascriptResponseDelegate eventObj);
        public static event JavascriptResponseEventArgs OnJavascriptResponse;

        public string Uri
        {
            get { return _controlEventAbstraction.Target.GetUri(this); }
        }

        public FormsWebView()
        {
            _controlEventAbstraction = new WebViewControlEventAbstraction() { Source = new WebViewControlEventStub() };
        }

        public void Navigate(string uri, WebViewContentType contentType, string baseUri = "")
        {
            _controlEventAbstraction.Target.PerformNavigation(this, uri, contentType, baseUri);
        }

        public void InjectJavascript(string js)
        {
            _controlEventAbstraction.Target.InjectJavascript(this, js);
        }

        /// <summary>
        /// Internal Use Only.
        /// </summary>
        /// <param name="sender">The FWV sender</param>
        /// <param name="type">The type of event</param>
        /// <param name="eventObject">The WVE object to pass</param>
        /// <returns></returns>
        public object InvokeEvent(FormsWebView sender, WebViewEventType type, WebViewDelegate eventObject)
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
