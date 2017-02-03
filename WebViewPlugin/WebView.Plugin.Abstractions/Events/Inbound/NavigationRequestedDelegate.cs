namespace WebView.Plugin.Abstractions.Events.Inbound
{
    public class NavigationRequestedDelegate : WebViewDelegate
    {

        public bool Cancel { get; set; } = false;
        public string Uri { get; set; }

        public NavigationRequestedDelegate(FormsWebView element, string uri) : base(element)
        {
            Uri = uri;
        }

    }
}
