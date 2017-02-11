namespace Xam.Plugin.Abstractions.Events.Inbound
{
    public class JavascriptResponseDelegate : WebViewDelegate
    {
        public string Data { get; set; }

        public JavascriptResponseDelegate(FormsWebView element, string data) : base(element)
        {
            Data = data;
        }
    }
}
