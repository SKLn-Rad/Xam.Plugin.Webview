using System.Text;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions.Events.Outbound
{
    public class WebViewControlDelegate
    {
        
        public static string InjectedFunction
        {
            get
            {
                switch (Device.OS)
                {
                    case TargetPlatform.Android:
                        return "function csharp(data){bridge.invokeAction(data);}";
                    case TargetPlatform.iOS:
                        return "function csharp(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
                    default:
                        return "function csharp(data){window.external.notify(data);}";
                }
            }
        }

        public static string GenerateFunctionScript(string name)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Concat("function ", name, "(str){csharp( \"{'action':'" + name + "','data':'\"+str+\"'}\");}"));
            return sb.ToString();
        }

        public delegate void PerformNavigationDelegate(FormsWebView sender, string uri, WebViewContentType contentType);
        public static event PerformNavigationDelegate OnNavigationRequestedFromUser;

        public delegate void InjectJavascriptDelegate(FormsWebView sender, string js);
        public static event InjectJavascriptDelegate OnInjectJavascriptRequest;

        public delegate void RegisterActionsAddedDelegate(FormsWebView sender, string key);
        public static event RegisterActionsAddedDelegate OnActionAdded;

        public void NotifyCallbacksChanged(FormsWebView sender, string key)
        {
            OnActionAdded?.Invoke(sender, key);
        }

        public void PerformNavigation(FormsWebView sender, string uri, WebViewContentType contentType)
        {
            OnNavigationRequestedFromUser?.Invoke(sender, uri, contentType);
        }

        internal void InjectJavascript(FormsWebView sender, string js)
        {
            OnInjectJavascriptRequest?.Invoke(sender, js);
        }
    }
}
