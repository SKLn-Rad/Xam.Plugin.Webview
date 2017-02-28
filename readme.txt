1.3.0 Patch Notes:

I apologise for all the breaking changes. These changes were made with discussion with the community into making the plugin more friendly for people coming over from XLabs HWV.
This patch is preparation for 1.4.0 which will include support for Razor templating.

BREAKING CHANGES:
--------------------------------------------------

All abstracted events now have the prefix of "On". Please change your NavigationStarted and NavigationCompleted methods to align with this.
This will make it easier in the future for you to quickly highlight events added to FormsWebView.

OnNavigationComplete has been changed to cohere to its actual meaning in UWP and Android.
The OnNavigationComplete method will now be fired when the WebView starts loading the DOM whereas the new OnContentLoaded event will fire when the page has completed loading.
For this reason, any Javascript calls MUST be made after the content has finished loading instead of previously when the OnNavigationComplete event had been invoked.

The WebView.URI parameter is now called Source. This was to make it clearer to users using HTML from a string to use this field as the source for their string data.

BasePath was removed in favour for BaseUrl.

FEATURES:
--------------------------------------------------

OnContentLoaded is now a fully supported callback you can use to identify when the DOM is fully loaded.
It is using this callback that you should then inject your Javascript to the DOM.

BaseUrl is now fully supported across Android and iOS projects. UWP needs further testing as API's are missing from the native component to include a base url when navigating from a string.
To configure the BaseUrl, please set this field in the Renderer for each platform.
Defaults for this are ->
Android: Assets Folder
iOS: Resources Folder
Windows: Root Folder