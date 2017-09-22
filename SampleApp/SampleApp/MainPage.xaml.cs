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

                default:
                    break;
            }
        }
    }

}
