using System;
using System.Collections.Specialized;
using System.Text;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions.Events.Outbound
{
    public static class WebViewControlDelegate
    {
        public static string InjectedFunction
        {
            get
            {
                switch (Device.RuntimePlatform)
                {
                    case Device.Android:
                        return "function csharp(data){bridge.invokeAction(data);}";
                    case Device.iOS:
                        return "function csharp(data){window.webkit.messageHandlers.invokeAction.postMessage(data);}";
                    default:
                        return "function csharp(data){window.external.notify(data);}";
                }
            }
        }

        public static string GenerateFunctionScript(string name)
        {
            var sb = new StringBuilder();
            sb.Append(string.Concat("function ", name, "(str){csharp(\"{'action':'" + name + "','data':'\"+str+\"'}\");}"));
            return sb.ToString();
        }

        public delegate void PerformNavigationDelegate(FormsWebView sender, string uri, WebViewContentType contentType);
        public static event PerformNavigationDelegate OnNavigationRequestedFromUser;

        public delegate void InjectJavascriptDelegate(FormsWebView sender, string js);
        public static event InjectJavascriptDelegate OnInjectJavascriptRequest;

        public delegate void RegisterActionsAddedDelegate(FormsWebView sender, string key, bool isGlobal);
        public static event RegisterActionsAddedDelegate OnActionAdded;

        public delegate void NavigateThroughStackDelegate(FormsWebView sender, bool forward);
        public static event NavigateThroughStackDelegate OnStackNavigationRequested;

        public static void NotifyCallbacksChanged(FormsWebView sender, string key, bool isGlobal)
        {
            OnActionAdded?.Invoke(sender, key, isGlobal);
        }

        public static void PerformNavigation(FormsWebView sender, string uri, WebViewContentType contentType)
        {
            OnNavigationRequestedFromUser?.Invoke(sender, uri, contentType);
        }

        internal static void InjectJavascript(FormsWebView sender, string js)
        {
            OnInjectJavascriptRequest?.Invoke(sender, js);
        }

        internal static void NavigateThroughStack(FormsWebView sender, bool forward)
        {
            OnStackNavigationRequested?.Invoke(sender, forward);
        }
    }
}
