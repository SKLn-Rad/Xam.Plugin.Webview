namespace Xam.Plugin.Abstractions.Events.Inbound
{
    /// <summary>
    /// Parent class used in event abstraction and downcasted to obtain fields.
    /// </summary>
    public class WebViewDelegate
    {
        public delegate void WebViewControlChangedDelegate(object sender, FormsWebView element, object control);
        public FormsWebView Sender { get; set; }

        protected WebViewDelegate(FormsWebView sender)
        {
            Sender = sender;
        }
    }
}
