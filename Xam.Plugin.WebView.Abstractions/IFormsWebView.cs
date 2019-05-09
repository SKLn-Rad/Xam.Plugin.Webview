using System;
using System.Net;
using System.Threading.Tasks;
using Xam.Plugin.WebView.Abstractions.Delegates;
using Xam.Plugin.WebView.Abstractions.Enumerations;

namespace Xam.Plugin.WebView.Abstractions
{
    public interface IFormsWebView
    {

        event EventHandler<DecisionHandlerDelegate> OnNavigationStarted;

        event EventHandler<string> OnNavigationCompleted;

        event EventHandler<int> OnNavigationError;

        event EventHandler OnContentLoaded;

        WebViewContentType ContentType { get; set; }

        string Source { get; set; }

        string BaseUrl { get; set; }

        string CurrentUrl { get; set; }

        bool EnableGlobalCallbacks { get; set; }

        bool EnableGlobalHeaders { get; set; }

        bool Navigating { get; }

        bool CanGoBack { get; }

        bool CanGoForward { get; }

        void GoBack();

        void GoForward();

        void Refresh();

        Task<string> InjectJavascriptAsync(string js);

        void AddLocalCallback(string functionName, Action<string> action);

        void RemoveLocalCallback(string functionName);

        void RemoveAllLocalCallbacks();
        Task ClearCookiesAsync();
        Task<string> GetAllCookiesAsync();
        Task<string> GetCookieAsync(string key);
        Task<string> SetCookieAsync(Cookie cookie);
    }
}
