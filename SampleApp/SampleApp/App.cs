using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebView.Plugin.Abstractions;
using WebView.Plugin.Abstractions.Enumerations;
using WebView.Plugin.Abstractions.Events.Inbound;
using Xamarin.Forms;

namespace SampleApp
{
    public class App : Application
    {

        private FormsWebView WebView;

        public App()
        {

            WebView = new FormsWebView()
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            
            // The root page of your application
            var content = new ContentPage
            {
                Title = "SampleApp",
                Content = new StackLayout
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    BackgroundColor = Color.Red,
                    Children = {
                        WebView
                    }
                }
            };

            FormsWebView.NavigationStarted += OnNavigationStarted;
            FormsWebView.NavigationCompleted += OnNavigationComplete;
            FormsWebView.OnJavascriptResponse += OnJavascriptResponse;

            content.Appearing += Content_Appearing;
            MainPage = new NavigationPage(content);
        }

        private void OnJavascriptResponse(JavascriptResponseDelegate eventObj)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Javascript: {0}", eventObj.Data));
        }

        /// <summary>
        /// To return a string to c#, simple invoke the csharp(str) method.
        /// </summary>
        private void OnNavigationComplete(NavigationCompletedDelegate eventObj)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("Load Complete: {0}", eventObj.Sender.Uri));
            eventObj.Sender.InjectJavascript("csharp('Testing');");
        }

        /// <summary>
        /// You can cancel a URL from being loaded by returning a delegate with the cancel boolean set to true.
        /// </summary>
        private NavigationRequestedDelegate OnNavigationStarted(NavigationRequestedDelegate eventObj)
        {
            if (eventObj.Uri == "www.somebadwebsite.com")
                eventObj.Cancel = true;

            return eventObj;
        }

        private void Content_Appearing(object sender, EventArgs e)
        {
            //WebView.Uri = "https://www.google.com"
            //WebView.Uri = "<html><body>Hello World!</body></html>";
            //WebView.Uri = "Web/Sample.html";

            WebView.Navigate("https://www.google.com", WebViewContentType.Internet);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
