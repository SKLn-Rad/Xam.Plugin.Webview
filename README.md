# WebView Plugin for Xamarin
Lightweight cross platform WebView designed to leverage the native WebView components in Android, iOS, and Windows to provide enhanced functionality over the base control.

[Xamarin Forums Link](https://forums.xamarin.com/discussion/87935/new-simple-webview-plugin-for-forms)


## Version 2.0.0!
Finally! in alignment with the MacOS and netstandard support in Xamarin.Forms, here is the new release of Xam.Plugins.WebView.
Yes there are many changes, but all of these are designed to make less changes in the future with only stable from here on in.

## Warning
This build has many changes, please read migration before deciding to make the jump.

### Whats new!
1) netstandard support
2) MacOS support
3) Local and Global Headers
4) Various fixes for bugs on the issue tracker
5) The ability to evaluate Javascript directly without needing a callback
6) Massively improved sample application, designed to help you with implementation

### Migration
1) WinRT (Desktop and Phone) are no longer supported! This is to align with Xamarin Forms.
2) FormsWebViewRenderer.Init is now called FormsWebViewRenderer.Initialize. This is as not to hide a default property in the MacOS renderer.
3) Headers now have to be applied globally or locally via GlobalRegisteredHeaders and LocalRegisteredHeaders. All previous calls will no longer work.
4) OnNavigationRequest no longer requires a return response
5) InjectJavascript is now called InjectJavascriptAsync as to allow the string response mentioned earlier
6) The assembly name is now Xam.Plugin.WebView. This is to align with the namespaces in the plugin
7) Callbacks are now added by calling AddLocalCallback and AddGlobalCallback
8) OnControlChanging has been removed, OnControlChanged is still there. This will be called after the renderer has finished setting up the plugin.
9) OnJavascriptResponse has been removed as it was no longer needed with the callbacks and string response.

Anything I forgot? Let me know in the issues!

## Why I made this?
Hybrid WebViews are common across many applications these days with many different implementations available on the Web.
Unfortunately for Xamarin, generally the only common HybridWebView is included as part of the XLabs toolset which gives the user the extra bloat of the additional components, as well as the problems associated with setting up this framework.

**Forms WebView** is designed to be a lightweight alternative with only minor configuration needed to be performed by the developer before giving them access to a denser API allowing them more flexibility in creating hybrid applications in the Xamarin platform.


## Setup
* NuGET package available here: https://www.nuget.org/packages/Xam.Plugin.WebView
* Install into both your PCL and Platform projects
* On Android, include the Android.Mono.Export reference for the Javascript Interface

```c#
/// <summary>
/// Please call this before Forms is initialized to make sure assemblies link properly.
/// Make sure to perform this step on each platform.
/// </summary>
FormsWebViewRenderer.Initialize();
Xamarin.Forms.Forms.Init(e);
```

## Build Status
* Jenkins build history can be found here: TBA


## Platform Support
*Please note: I have only put in platforms I have tested myself.*
* Xamarin.iOS : iOS 9 +
* Xamarin.MacOS : All
* Xamarin.Droid : API 17 +
* Windows UWP : 10 +

### Known Limitations
* Android API level 22 and below will not be able to report HTTPErrors correctly. This is down to the lack of API support from Google up until this release. If you need a way around this, you can add in a hack using System.Web during the OnNavigationRequest.

## API Usage
### New!
```c#
/// <summary>
/// Bind an action to a Javascript function
/// </summary>
FormsWebView WebView = new FormsWebView();
WebView.AddLocalCallback("test", (str) => Debug.WriteLine(str));
WebView.RemoveLocalCallback("test");
```

```c#
/// <summary>
/// Initialize the WebView, Navigation will occur when the Source is changed so make sure to set the BaseUrl and ContentType prior.
/// </summary>
FormsWebView WebView = new FormsWebView() {
    ContentType = WebContentType.Internet,
    Source = "http://www.somewebsite.com"
}
```

```c#
/// <summary>
/// If you wish to further modify the native control, then you can bind to these events in your platform specific code.
/// These events will be called when the control is preparing and ready.
/// </summary>
FormsWebViewRenderer.OnControlChanged += ModifyControlAfterReady;
```

```c#
/// <summary>
/// Attach events using a instance of the WebView.
/// </summary>
WebView.OnNavigationStarted += OnNavigationStarted;
WebView.OnNavigationCompleted += OnNavigationComplete;
WebView.OnContentLoaded += OnContentLoaded;
```

```c#
/// <summary>
/// You can cancel a URL from being loaded by returning a delegate with the cancel boolean set to true.
/// </summary>
private void OnNavigationStarted(NavigationRequestedDelegate eventObj)
{
    if (eventObj.Source == "www.somebadwebsite.com")
        eventObj.Cancel = true;
}
```

```c#
/// <summary>
/// To return a string to c#, simple invoke the csharp(str) method.
/// </summary>
private void OnNavigationComplete(NavigationCompletedDelegate eventObj)
{
    System.Diagnostics.Debug.WriteLine(string.Format("Load Complete: {0}", eventObj.Sender.Source));
}

/// <summary>
/// RUN ALL JAVASCRIPT HERE
/// </summary>
private void OnContentLoaded(ContentLoadedDelegate eventObj)
{
    System.Diagnostics.Debug.WriteLine(string.Format("DOM Ready: {0}", eventObj.Sender.Source));
    eventObj.Sender.InjectJavascript("csharp('Testing');");
}
```


**Local File Locations**
To modify the file locations, change the BaseUrl in each platforms renderer
* **iOS**: Resources Folder as a bundle resource
* **Android**: Assets folder as an Android Asset
* **Windows**: Root folder as content
* **MacOS**: Resources Folder as a bundle resource


## Feature Requests
DM me on LinkedIn: http://linkedin.radsrc.com

## Notes
**For iOS 9 onwards and MacOS, if you wish to access unsecure sites you may need to configure or disable ATS**
```
<key>NSAppTransportSecurity</key>
<dict>
<key>NSAllowsArbitraryLoads</key><true/>
</dict>
```

**For Android make sure to add the "Internet" property to your manifest.**


**For Windows make sure to add the websites to your appxmanifest ContentUris to allow JS invoking.**
