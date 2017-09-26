using System;
using System.Collections.Generic;
using System.Text;
using Xam.Plugin.Abstractions.Delegates;
using Xam.Plugin.Abstractions.Enumerations;

namespace Xam.Plugin.Abstractions
{
    public interface IFormsWebView
    {

        event EventHandler<DecisionHandlerDelegate> OnNavigationStarted;

        event EventHandler OnNavigationCompleted;

        event EventHandler<int> OnNavigationError;

        event EventHandler OnContentLoaded;

        WebViewContentType ContentType { get; set; }
        
        string Source { get; set; }

        string BaseUrl { get; set; }

        bool EnableGlobalCallbacks { get; set; }

        bool EnableGlobalHeaders { get; set; }

        bool Navigating { get; }

        bool CanGoBack { get; }

        bool CanGoForward { get; }

        void InjectJavascript(string js);

        string EvaluateJavascriptAsync(string js);

        void AddLocalCallback(string functionName, Action<string> action);

        void RemoveLocalCallback(string functionName);

        void RemoveAllLocalCallbacks();
    }
}
