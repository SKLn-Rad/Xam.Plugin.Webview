namespace WebView.Plugin.Abstractions.Events.Inbound
{
    public class NavigationCompletedDelegate : WebViewDelegate
    {

        public bool IsSuccess { get; set; }
        public string Uri { get; set; }

        public NavigationCompletedDelegate(FormsWebView element, string uri, bool isSuccess = true) : base(element)
        {
            Uri = uri;
            IsSuccess = isSuccess;
        }

    }
}
