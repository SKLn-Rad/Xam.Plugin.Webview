using System;
using System.Collections.Generic;
using Xam.Plugin.Abstractions.Enumerations;
using Xamarin.Forms;

namespace Xam.Plugin.Abstractions
{
    public partial class FormsWebView
    {
        
        public static readonly BindableProperty NavigatingProperty = BindableProperty.Create(nameof(Navigating), typeof(bool), typeof(FormsWebView), false);

        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(FormsWebView));
        public static readonly BindableProperty ContentTypeProperty = BindableProperty.Create(nameof(ContentType), typeof(WebViewContentType), typeof(FormsWebView), WebViewContentType.Internet);

        public static readonly BindableProperty BaseUrlProperty = BindableProperty.Create(nameof(BaseUrl), typeof(string), typeof(FormsWebView));

        public static readonly BindableProperty CanGoBackProperty = BindableProperty.Create(nameof(CanGoBack), typeof(bool), typeof(FormsWebView), false);
        public static readonly BindableProperty CanGoForwardProperty = BindableProperty.Create(nameof(CanGoForward), typeof(bool), typeof(FormsWebView), false);

        public static readonly BindableProperty EnableGlobalCallbacksProperty = BindableProperty.Create(nameof(EnableGlobalCallbacks), typeof(bool), typeof(FormsWebView), true);
        public static readonly BindableProperty EnableGlobalHeadersProperty = BindableProperty.Create(nameof(EnableGlobalHeaders), typeof(bool), typeof(FormsWebView), true);

        public readonly static Dictionary<string, Action<string>> GlobalRegisteredCallbacks = new Dictionary<string, Action<string>>();
        public readonly static Dictionary<string, string> GlobalRegisteredHeaders = new Dictionary<string, string>();

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
