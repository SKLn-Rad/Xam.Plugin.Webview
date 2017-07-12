namespace Xam.Plugin.Abstractions.Events.Inbound
{
    public enum WebViewEventType
    {
        NavigationRequested,
        NavigationError,
        NavigationComplete,
        NavigationStackUpdate,
        JavascriptCallback,
        ContentLoaded
    }
}
