using Android.Webkit;

namespace WebView.Plugin.Droid.Extras
{
    public class FormsWebViewChromeClient : WebChromeClient
    {

        private FormsWebViewRenderer Renderer;

        public FormsWebViewChromeClient(FormsWebViewRenderer renderer)
        {
            Renderer = renderer;
        }
    }
}