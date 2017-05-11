using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;

namespace Xam.Plugin.Abstractions.Events.Inbound
{
    public class ContentLoadedDelegate : WebViewDelegate
    {
        public string Uri { get; set; }

        public ContentLoadedDelegate(FormsWebView element, string uri) : base(element)
        {
            Uri = uri;
        }
    }
}
