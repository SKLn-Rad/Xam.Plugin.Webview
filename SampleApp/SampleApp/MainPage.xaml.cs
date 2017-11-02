using SampleApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using SampleApp.Samples;

namespace SampleApp
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {

        ObservableCollection<SelectionItem> Items = new ObservableCollection<SelectionItem>();

        public MainPage()
        {
            AddItems();
            InitializeComponent();

            ItemList.ItemsSource = Items;
        }

        void AddItems()
        {
            Items.Add(new SelectionItem()
            {
                Identifier = 0,
                Title = "Internet",
                Detail = "Generic content loaded from the internet, no additions whatsoever."
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 1,
                Title = "Local Files",
                Detail = "Load local files, by default these are Android Assets, iOS Resources, and files in the root of a UWP solution"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 2,
                Title = "String Data",
                Detail = "Load a WebView using string data as the source"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 3,
                Title = "Bindable Source",
                Detail = "Load a WebView with a bound string as its source"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 4,
                Title = "Navigation Events (Internet)",
                Detail = "Watch and control events on the webview using internet based content"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 5,
                Title = "Navigation Events (Local)",
                Detail = "Watch and control events on the webview using file based content"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 6,
                Title = "Navigation Events (String)",
                Detail = "Watch and control events on the webview using string based content"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 7,
                Title = "Navigating Property",
                Detail = "Monitor when the webview is in a navigating state using the bindable property"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 8,
                Title = "Javascript (Internet)",
                Detail = "Check javascript injection, callbacks, and evaluation functions on a real web page"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 9,
                Title = "Javascript (Local)",
                Detail = "Check javascript injection, callbacks, and evaluation functions on a local file"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 10,
                Title = "Javascript (String)",
                Detail = "Check javascript injection, callbacks, and evaluation functions on string html"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 11,
                Title = "Background Color",
                Detail = "Test sites with a transparent background bleed onto the background set in Xamarin Forms"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 12,
                Title = "CanGoForward and CanGoBack",
                Detail = "Test bindable properties CanGoForward and CanGoBack, as well as the GoBack and GoForward functions"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 13,
                Title = "Refresh",
                Detail = "Test the refresh function"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 14,
                Title = "Headers",
                Detail = "Test global and local request headers"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 15,
                Title = "Live Callbacks",
                Detail = "Test that callbacks which are added during content presentation get added to the DOM"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 16,
                Title = "Navigation Page (Push)",
                Detail = "Test forward navigation maintaining the last view in the stack"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 17,
                Title = "Navigation Page (Wipe)",
                Detail = "Test forward navigation wiping the last view in the stack"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 18,
                Title = "Null source",
                Detail = "Test setting the source to null does not result in a crash"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 19,
                Title = "Email data",
                Detail = "Load a WebView using string data as the source with a mailto: link"
            });

            Items.Add(new SelectionItem()
            {
                Identifier = 20,
                Title = "Scroll test",
                Detail = "Loads a long webview with buttons to scroll to the top and bottom"
            });
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            int i = (e.SelectedItem as SelectionItem).Identifier;

            switch (i)
            {
                case 0:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new InternetSample());
                    break;

                case 1:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new LocalFileSample());
                    break;

                case 2:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new StringDataSample());
                    break;

                case 3:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new SourceSwapSample());
                    break;

                case 4:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigationEventsSample());
                    break;

                case 5:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigationLocalSample());
                    break;

                case 6:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigationStringSample());
                    break;

                case 7:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigatingEvent());
                    break;

                case 8:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new JavascriptInternet());
                    break;

                case 9:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new JavascriptSample());
                    break;

                case 10:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new JavascriptString());
                    break;

                case 11:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new BackgroundColorSample());
                    break;

                case 12:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new BackForwardSample());
                    break;

                case 13:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new RefreshSample());
                    break;

                case 14:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new HeadersSample());
                    break;

                case 15:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new LiveCallbackSample());
                    break;

                case 16:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigationPageStart());
                    break;

                case 17:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NavigationPageWipe());
                    break;

                case 18:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new NullSourceSample());
                    break;

                case 19:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new EmailDataSample());
                    break;

                case 20:
                    await ((NavigationPage)Application.Current.MainPage).PushAsync(new ScrollToSample());
                    break;

                default:
                    break;
            }
        }
    }

}
