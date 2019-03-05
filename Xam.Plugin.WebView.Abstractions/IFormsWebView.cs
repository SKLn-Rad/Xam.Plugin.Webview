using System;
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

        event EventHandler<DecisionHandlerDelegate> OnContentTypeLoaded;

        WebViewContentType ContentType { get; set; }

        string Source { get; set; }

        string BaseUrl { get; set; }

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
        Task AddCookieAsync(System.Net.Cookie cookie);
    }
}
