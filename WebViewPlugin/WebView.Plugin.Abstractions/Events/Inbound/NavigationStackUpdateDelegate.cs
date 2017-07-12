using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;

namespace WebView.Plugin.Abstractions.Events.Inbound
{
    public class NavigationStackUpdateDelegate : WebViewDelegate
    {

        public bool CanGoBack { get; set; }
        public bool CanGoForward { get; set; }

        public NavigationStackUpdateDelegate(FormsWebView sender, bool canGoBack, bool canGoForward) : base(sender)
        {
            CanGoBack = canGoBack;
            CanGoForward = canGoForward;
        }
    }
}
