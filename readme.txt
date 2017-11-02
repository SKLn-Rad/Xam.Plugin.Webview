2.1.0 Patch Notes:

Limitation
A bug fix where the Android webview could be invalidated was fixed in this build.
The bug fix however exposed a limitation in the Android OverloadUrlLoading method. From now on, Android will only ever be able to cancel requests that start from the webview.
If you set the Uri, then on Android OnNavigationStarted will be ignored.

1) OnNavigationCompleted will now return the string of the url back as the EventArg parameter
2) OnNavigationStarted now passes a new parameter in its delegate (OffloadOntoDevice)

If you set OffloadOntoDevice to true, then the device will try to pass the Url to another part of the device
For example: if you set this to true with a https uri, then the device will open the https page in its default browser.
Another example: The mailto scheme so that you can email someone from the webview

By default, this is false for HTTP/S schemes, and string/local file uris. It is true for any other valid scheme type.

OffloadOntoDevice Support

Android:
Uri Scheme
Mailto Scheme

iOS:
All available to the device (Using UIApplication.CanOpenUri)

UWP/MacOS
No available yet

Any questions, email me at ryandixon1993@gmail.com or PM me on the Xamarin Chat slack channel
https://xamarinchat.herokuapp.com/ (ryandixon)