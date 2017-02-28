using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;
using Xam.Plugin.Abstractions.Extensions;
using Xam.Plugin.Abstractions.Enumerations;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xam.Plugin.Abstractions.Events.Outbound;
using Xam.Plugin.Abstractions.DTO;
using WebView.Plugin.Abstractions.Events.Inbound;

namespace Xam.Plugin.Abstractions
{
    public class FormsWebView : View
    {

        public static readonly BindableProperty NavigatingProperty = BindableProperty.Create(nameof(Navigating), typeof(bool), typeof(FormsWebView), false);
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(FormsWebView), null);
        public static readonly BindableProperty ContentTypeProperty = BindableProperty.Create(nameof(ContentType), typeof(WebViewContentType), typeof(FormsWebView), WebViewContentType.Internet);
        public static readonly BindableProperty RegisteredActionsProperty = BindableProperty.Create(nameof(RegisteredActions), typeof(Dictionary<string, Action<string>>), typeof(FormsWebView), new Dictionary<string, Action<string>>());
        
        public bool Navigating
        {
            get { return (bool) GetValue(NavigatingProperty); }
            set { SetValue(NavigatingProperty, value); }
        }

        public string Source
        {
            get { return (string) GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); _controlEventAbstraction.Target.PerformNavigation(this, value, ContentType);}
        }

        public WebViewContentType ContentType
        {
            get { return (WebViewContentType) GetValue(ContentTypeProperty); }
            set { SetValue(ContentTypeProperty, value); }
        }

        private Dictionary<string, Action<string>> RegisteredActions
        {
            get { return (Dictionary<string, Action<string>>) GetValue(RegisteredActionsProperty); }
            set { SetValue(RegisteredActionsProperty, value); }
        }

        private WebViewControlEventAbstraction _controlEventAbstraction;
        
        public delegate NavigationRequestedDelegate WebViewNavigationStartedEventArgs(NavigationRequestedDelegate eventObj);
        public event WebViewNavigationStartedEventArgs OnNavigationStarted;

        public delegate void WebViewNavigationCompletedEventArgs(NavigationCompletedDelegate eventObj);
        public event WebViewNavigationCompletedEventArgs OnNavigationCompleted;

        public delegate void JavascriptResponseEventArgs(JavascriptResponseDelegate eventObj);
        public event JavascriptResponseEventArgs OnJavascriptResponse;

        public delegate void ContentLoadedEventArgs(ContentLoadedDelegate eventObj);
        public event ContentLoadedEventArgs OnContentLoaded;

        public FormsWebView()
        {
            _controlEventAbstraction = new WebViewControlEventAbstraction() { Source = new WebViewControlEventStub() };
        }

        public void InjectJavascript(string js)
        {
            _controlEventAbstraction.Target.InjectJavascript(this, js);
        }

        public void RegisterCallback(string name, Action<string> callback)
        {
            if (!RegisteredActions.ContainsKey(name))
            {
                RegisteredActions.Add(name, callback);
                _controlEventAbstraction.Target.NotifyCallbacksChanged(this, name);
            }
        }

        public void RemoveCallback(string name)
        {
            if (RegisteredActions.ContainsKey(name))
                RegisteredActions.Remove(name);
        }

        public string[] GetAllCallbacks()
        {
            return RegisteredActions.Keys.ToArray();
        }

        public void RemoveAllCallbacks()
        {
            RegisteredActions.Clear();
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
                    return OnNavigationStarted == null ? eventObject as NavigationRequestedDelegate : OnNavigationStarted.Invoke(eventObject as NavigationRequestedDelegate);

                case WebViewEventType.NavigationComplete:
                    OnNavigationCompleted?.Invoke(eventObject as NavigationCompletedDelegate);
                    break;

                case WebViewEventType.ContentLoaded:
                    OnContentLoaded?.Invoke(eventObject as ContentLoadedDelegate);
                    break;

                case WebViewEventType.JavascriptCallback:
                    var data = (eventObject as JavascriptResponseDelegate).Data;
                    ActionResponse ar;
                    if (data.ValidateJSON() && (ar = data.AttemptParseActionResponse()) != null)
                    {
                        if (RegisteredActions.ContainsKey(ar.Action))
                            RegisteredActions[ar.Action]?.Invoke(ar.Data);
                    }
                    else
                    {
                        OnJavascriptResponse?.Invoke(eventObject as JavascriptResponseDelegate);
                    }
                    break;
            }

            return eventObject;
        }
    }
}
