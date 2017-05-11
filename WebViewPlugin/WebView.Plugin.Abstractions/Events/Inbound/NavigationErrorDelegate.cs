using System.Net;
using Xam.Plugin.Abstractions;
using Xam.Plugin.Abstractions.Events.Inbound;

namespace Xam.Plugin.Abstractions.Events.Inbound
{
    public class NavigationErrorDelegate : WebViewDelegate
    {
        public int ErrorCode { get; set; }

        public NavigationErrorDelegate(FormsWebView sender, int errorCode) : base(sender)
        {
            ErrorCode = errorCode;
        }
    }
}
