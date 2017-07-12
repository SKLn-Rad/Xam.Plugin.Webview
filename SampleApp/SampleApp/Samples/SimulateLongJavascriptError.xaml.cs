using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xam.Plugin.Abstractions.Events.Inbound;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SimulateLongJavascriptError : ContentPage
    {

        SLJEViewModel ViewModel = new SLJEViewModel();

        public SimulateLongJavascriptError()
        {
            InitializeComponent();
            BindingContext = ViewModel;
            XamWebview.RegisterGlobalCallback("xamJSCallback", HandleJSCallback);
        }
        
        void StartLongJS(object sender, EventArgs e)
        {
            ViewModel.JSLabel = "Executing Javascript...";
            XamWebview.InjectJavascript("setTimeout(function(){ xamJSCallback('Testing'); }, 5000);");
        }

        void HandleJSCallback(string obj) => ViewModel.JSLabel = "Done Executing";
        void NavigateAway(object sender, EventArgs e) => Application.Current.MainPage = new MainPage();

        public class SLJEViewModel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public void RaisePropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            string JSLabel_ = "Waiting for Javascript";
            public string JSLabel
            {
                get { return JSLabel_; }
                set { JSLabel_ = value; RaisePropertyChanged(nameof(JSLabel)); }
            }
        }
    }
}