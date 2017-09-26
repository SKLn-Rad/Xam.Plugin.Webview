using System;
using System.Collections.Generic;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions
{
    public partial class FormsWebView
    {

        internal static event EventHandler<string> GlobalCallbackAdded;

        public static readonly BindableProperty NavigatingProperty = BindableProperty.Create(nameof(Navigating), typeof(bool), typeof(FormsWebView), false);

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(FormsWebView));
        public static readonly BindableProperty ContentTypeProperty = BindableProperty.Create(nameof(ContentType), typeof(WebViewContentType), typeof(FormsWebView), WebViewContentType.Internet);

        public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(FormsWebView));

        public static readonly BindableProperty CanGoBackProperty = BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(FormsWebView), false);
        public static readonly BindableProperty CanGoForwardProperty = BindableProperty.Create(nameof(CanGoForward), typeof(bool), typeof(FormsWebView), false);

        public static readonly BindableProperty EnableGlobalCallbacksProperty = BindableProperty.Create(nameof(EnableGlobalCallbacks), typeof(bool), typeof(FormsWebView), true);
        public static readonly BindableProperty EnableGlobalHeadersProperty = BindableProperty.Create(nameof(EnableGlobalHeaders), typeof(bool), typeof(FormsWebView), true);

        internal readonly static Dictionary<string, Action<string>> GlobalRegisteredCallbacks = new Dictionary<string, Action<string>>();
        public readonly static Dictionary<string, string> GlobalRegisteredHeaders = new Dictionary<string, string>();

        public static void AddGlobalCallback(string functionName, Action<string> action)
        {
            if (GlobalRegisteredCallbacks.ContainsKey(functionName))
                GlobalRegisteredCallbacks.Remove(functionName);

            GlobalRegisteredCallbacks.Add(functionName, action);
            GlobalCallbackAdded?.Invoke(null, functionName);
        }

        public static void RemoveGlobalCallback(string functionName)
        {
            if (GlobalRegisteredCallbacks.ContainsKey(functionName))
                GlobalRegisteredCallbacks.Remove(functionName);
        }

        public static void RemoveAllGlobalCallbacks()
        {
            GlobalRegisteredCallbacks.Clear();
        }

        internal static string InjectedFunction
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

        internal static string GenerateFunctionScript(string name)
        {
            return $"function {name}(str){{csharp(\"{{'action':'{name}','data':'\"+str+\"'}}\");}}";
        }
    }
}
