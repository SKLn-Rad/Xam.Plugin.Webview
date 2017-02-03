# WebView Plugin for Xamarin
Lightweight cross platform WebView designed to leverage the native WebView components in Android, iOS, and Windows to provide enhanced functionality over the base control.

### Why I made this?
Hybrid WebViews are common across many applications these days with many different implementations available on the Web.
Unfortunately for Xamarin, generally the only common HybridWebView is included as part of the XLabs toolset which gives the user the extra bloat of the additional components, as well as the problems associated with setting up this framework.

**Forms WebView** is designed to be a lightweight alternative with only minor configuration needed to be performed by the developer before giving them access to a denser API allowing them more flexibility in creating hybrid applications in the Xamarin platform.


### Setup
* NuGET package available here: https://www.nuget.org/packages/Xam.Plugin.WebView/1.0.0
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


### If this helps you!
Pints are greatly appreciated: PayPal @ ryandixon1993@gmail.com


### Build Status
* Jenkins build history can be found here: TBA


### Platform Support
*Please note: I have only put in platforms I have tested myself.*
* Xamarin.iOS : iOS 9 +
* Xamarin.Droid : API 17 +
* Windows Phone/Store RT : 8.1 +
* Windows UWP : 10 +
* Xamarin Forms : 2.3.3.180


### API Usage
```c#
/// <summary>
/// Initialize the WebView, only call navigate once the WebView is visible on the screen.
/// </summary>
FormsWebView WebView = new FormsWebView();
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
/// Attach events using a static context, this allows for better decoupling across multiple WebViews.
/// Each callback will include a sender for its WebView.
/// </summary>
FormsWebView.NavigationStarted += OnNavigationStarted;
FormsWebView.NavigationCompleted += OnNavigationComplete;
FormsWebView.OnJavascriptResponse += OnJavascriptResponse;
```

```c#
/// <summary>
/// You can cancel a URL from being loaded by returning a delegate with the cancel boolean set to true.
/// </summary>
private NavigationRequestedDelegate OnNavigationStarted(NavigationRequestedDelegate eventObj)
{
    if (eventObj.Uri == "www.somebadwebsite.com")
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
    System.Diagnostics.Debug.WriteLine(string.Format("Load Complete: {0}", eventObj.Sender.Uri));
    eventObj.Sender.InjectJavascript("csharp('Testing');");
}
```

```c#
/// <summary>
/// Navigate by using the Navigate method and passing in either:
/// String HTML data, the path to a bundled HTML file, or a http/s URL.
/// </summary>
WebView.Navigate("https://www.google.com", WebViewContentType.Internet);
```

**Local File Locations**
*Plans are to eventually allow access from anywhere on the file system, but for now you MUST bundle them*
* **iOS**: Resources Folder as a bundle resource
* **Android**: Assets folder as an Android Asset
* **Windows**: Root folder as content


## Feature Requests
DM me on LinkedIn / Twitter: http://linkedin.radsrc.com || https://twitter.com/SkysRad

### Notes
**For iOS 9 onwards, if you wish to access unsecure sites you may need to configure or disable ATS**
```
<key>NSAppTransportSecurity</key>
<dict>
<key>NSAllowsArbitraryLoads</key><true/>
</dict>
```

**For Android make sure to add the "Internet" property to your manifest.**


**For Windows make sure to add the websites to your appxmanifest ContentUris to allow JS invoking.**
