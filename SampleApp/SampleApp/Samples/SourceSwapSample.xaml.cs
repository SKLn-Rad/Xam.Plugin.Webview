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
    public partial class SourceSwapSample : ContentPage
    {
        SourceSwapViewModel ViewModel = new SourceSwapViewModel();

        public SourceSwapSample()
        {
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public class SourceSwapViewModel : INotifyPropertyChanged
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

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            ViewModel.Uri = EntryField.Text;
        }
    }
}