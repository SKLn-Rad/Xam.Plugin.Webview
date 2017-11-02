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

        /// <summary>
        /// If true, then the application will attempt to offload the url call to the device.
        /// For example mailto: schema's will attempt to load a mail app.
        /// 
        /// By default, this is false for http/s schema's and true for every other schema.
        /// Note: This is only supported in iOS and Android currently.
        /// </summary>
        public bool OffloadOntoDevice { get; set; }

    }
}
