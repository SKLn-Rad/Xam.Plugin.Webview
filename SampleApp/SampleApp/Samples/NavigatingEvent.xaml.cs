using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigatingEvent : ContentPage
    {

        public NavigatingEventViewModel ViewModel = new NavigatingEventViewModel();

        public NavigatingEvent()
        {
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public class NavigatingEventViewModel : INotifyPropertyChanged
        {

            string _uri = "https://www.google.co.uk";

            public string Uri
            {
                get => _uri;
                set
                {
                    _uri = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Uri)));
                }
            }

            Command _reloadCommand;

            public Command ReloadCommand
            {
                get => _reloadCommand;
                set
                {
                    _reloadCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReloadCommand)));
                }
            }

            public NavigatingEventViewModel()
            {
                ReloadCommand = new Command(() =>
                {
                    if (Uri.Equals("https://www.google.co.uk"))
                        Uri = "https://www.xamarin.com";
                    else
                        Uri = "https://www.google.co.uk";
                });
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}