namespace Xam.Plugin.WebView.Abstractions.Delegates
{
    public class DecisionHandlerDelegate
    {

        /// <summary>
        /// The publishing Uri
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Whether the Webview should abandon the navigation event
        /// </summary>
        public bool Cancel { get; set; }

    }
}
