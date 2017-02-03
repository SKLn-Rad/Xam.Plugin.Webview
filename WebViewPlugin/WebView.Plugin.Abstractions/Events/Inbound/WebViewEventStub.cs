using WebView.Plugin.Abstractions.Events.Inbound;

namespace WebView.Plugin.Abstractions.Inbound
{
    /// <summary>
    /// Stub class used in rewriting calls in unmanaged memory
    /// </summary>
    public class WebViewEventStub
    {
        public object InvokeEvent(FormsWebView sender, WebViewEventType type, WebViewDelegate eventObject)
        {
            return null;
        }
    }
}
