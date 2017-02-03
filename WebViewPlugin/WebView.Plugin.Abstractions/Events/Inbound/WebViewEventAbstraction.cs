using System.Runtime.InteropServices;

namespace WebView.Plugin.Abstractions.Inbound
{
    /// <summary>
    /// "They don't think it be like it is, but it do."
    /// 
    /// TL;DR:
    /// Don't ask how this works unless you have a firm grasp of quantum mechanics, complete inner solar system celestial alignment, and a living sacrifice to lord Gaben.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public class WebViewEventAbstraction
    {
        [FieldOffset(0)]
        public WebViewEventStub Source;

        [FieldOffset(0)]
        public FormsWebView Target;
    }
}
