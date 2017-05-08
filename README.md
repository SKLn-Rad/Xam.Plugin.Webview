# WebView Plugin for Xamarin
Lightweight cross platform WebView designed to leverage the native WebView components in Android, iOS, and Windows to provide enhanced functionality over the base control.

[Xamarin Forums Link](https://forums.xamarin.com/discussion/87935/new-simple-webview-plugin-for-forms)

### This is a work in development
This WebView is the brain child of a need for a lightweight solution to supply in my own applications and is purely open source for you guys to use. If this helps you in anyway then please do support open source developers such as myself! PayPal: ryandixon1993@gmail.com

## New in 1.5.*
### Local and Global Callbacks
Previous to 1.5, all callbacks were made global, meaning that if you had multiple WebViews existing in your application, then all of them would inherit the callbacks registered to one-another. In 1.5 you now have the option of setting them as "local" callbacks, meaning they will only effect that local instance of the WebView. This is in tandem with the previous mechanism.

* RegisterCallback has been deprecated for the renamed RegisterGlobalCallback
* RemoveCallback has been deprecated for the renamed RemoveGlobalCallback
* RemoveAllCallbacks has been deprecated for the renamed RemoveAllGlobalCallbacks
* GetCallbacks has been deprecated for the renamed GetGlobalCallbacks
* Four new APIs have been added (RegisterLocalCallback, RemoveLocalCallback, RemoveAllLocalCallbacks, and GetAllLocalCallbacks)

## New in 1.4.*
### Fixes for Background Colour
The WebViews will now inherit the background colour supplied by FormsWebView.BackgroundColor.

### HttpError Handling
You can now catch HttpErrorCodes when an error occurs during Navigation. To do this, simple use the OnNavigationError event in the WebView.

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
FormsWebViewRenderer.Init();
Xamarin.Forms.Forms.Init(e);
```

## Build Status
* Jenkins build history can be found here: TBA


## Platform Support
*Please note: I have only put in platforms I have tested myself.*
* Xamarin.iOS : iOS 9 +
* Xamarin.Droid : API 17 +
* Windows Phone/Store RT : 8.1 +
* Windows UWP : 10 +
* Xamarin Forms : 2.3.3.180

### Known Limitations
* Android API level 22 and below will not be able to report HTTPErrors correctly. This is down to the lack of API support from Google up until this release. If you need a way around this, you can add in a hack using System.Web during the OnNavigationRequest.

## API Usage
### New!
```c#
/// <summary>
/// Bind an action to a Javascript function
/// </summary>
FormsWebView WebView = new FormsWebView();
WebView.RegisterCallback("test", (str) => Debug.WriteLine(str));
WebView.RemoveCallback("test");
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
FormsWebViewRenderer.OnControlChanging += ModifyControlBeforeReady;
FormsWebViewRenderer.OnControlChanged += ModifyControlAfterReady;
```

```c#
/// <summary>
/// Attach events using a instance of the WebView.
/// </summary>
WebView.OnNavigationStarted += OnNavigationStarted;
WebView.OnNavigationCompleted += OnNavigationComplete;
WebView.OnJavascriptResponse += OnJavascriptResponse;
```

```c#
/// <summary>
/// You can cancel a URL from being loaded by returning a delegate with the cancel boolean set to true.
/// </summary>
private NavigationRequestedDelegate OnNavigationStarted(NavigationRequestedDelegate eventObj)
{
    if (eventObj.Source == "www.somebadwebsite.com")
        eventObj.Cancel = true;
    return eventObj;
}
```

```c#
/// <summary>
/// To return a string to c#, simple invoke the csharp(str) method.
/// </summary>
private void OnNavigationComplete(NavigationCompletedDelegate eventObj)
{
    System.Diagnostics.Debug.WriteLine(string.Format("Load Complete: {0}", eventObj.Sender.Source));
    eventObj.Sender.InjectJavascript("csharp('Testing');");
}
```


**Local File Locations**
To modify the file locations, change the BaseUrl in each platforms renderer
* **iOS**: Resources Folder as a bundle resource
* **Android**: Assets folder as an Android Asset
* **Windows**: Root folder as content


## Feature Requests
DM me on LinkedIn: http://linkedin.radsrc.com

## Notes
**For iOS 9 onwards, if you wish to access unsecure sites you may need to configure or disable ATS**
```
<key>NSAppTransportSecurity</key>
<dict>
<key>NSAllowsArbitraryLoads</key><true/>
</dict>
```

**For Android make sure to add the "Internet" property to your manifest.**


**For Windows make sure to add the websites to your appxmanifest ContentUris to allow JS invoking.**
