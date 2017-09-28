2.0.0 Patch Notes:

Hey everyone and thank you for trying my plugin!
After a great amount of support for 1.*, I decided to fully revamp 2.0.0 from the ground up in order to maintain a more consistant API, with full support for netstandard and MacOS.

Yes there will be some breaking changes from 1.*.
To make your life easier migrating, here is a list of some of them.

FormsWebViewRenderer:
1) Init has been changed to Initialize(). This is as to not overlap with a method already in the new MacOS delegate.
2) Android now has a static boolean called "IgnoreSSLErrors"; and as guessed by its name, setting this to true will cause the web client to ignore SSL errors.

FormsWebView:

Headers now exist in two scopes.
1) Global can be accessed from a static context (FormsWebView.GlobalRegisteredHeaders)
2) Local can be accessed from a class instance (obj.LocalRegisteredHeaders)
3) These headers are only applied during Internet based requests, and global headers can be disabled for a class instance by setting EnableGlobalHeaders to false.

Callbacks now exist in two scopes.
1) Same as before, but with callbacks. However to access these, you instead need to call Add(Local/Global)Callback; passing in a string key and an action

Navigation Request Delegate
1) You no longer need to return the delegate, instead just set its cancel property to true if you wish to cancel the request

Invoke Javascript
1) This method is now called InjectJavascriptAsync
2) This method will now return a string if the WebView so allows; allowing you to evaluate JS variables without having to pass them to a callback

MacOS
1) MacOS is now a supported platform, with identical functionality to iOS.
2) Local assets must be put in the Resources folder with their build property set to "BundleResource"

Once again, thanks a lot guys! And do feel free to pop me any errors on Github, or simply submit a PR yourself.