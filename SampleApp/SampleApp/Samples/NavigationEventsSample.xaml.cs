using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NavigationEventsSample : ContentPage
    {

        NavigationEventsViewModel ViewModel = new NavigationEventsViewModel();

        public NavigationEventsSample()
        {
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public class NavigationEventsViewModel : INotifyPropertyChanged
        {

            Command _errorCommand;

            public Command ErrorCommand
            {
                get => _errorCommand;
                set
                {
                    _errorCommand = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ErrorCommand)));
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

            bool _isCancelled;

            public bool IsCancelled
            {
                get => _isCancelled;
                set
                {
                    _isCancelled = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsCancelled)));
                }
            }

            public NavigationEventsViewModel()
            {
                ReloadCommand = new Command(() =>
                {
                    if (Uri.Equals("https://www.google.co.uk"))
                        Uri = "https://www.xamarin.com";
                    else
                        Uri = "https://www.google.co.uk";
                });

                ErrorCommand = new Command(() => Uri = "http://www.google.co.yk");
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void FormsWebView_OnNavigationStarted(object sender, Xam.Plugin.WebView.Abstractions.Delegates.DecisionHandlerDelegate e)
        {
            System.Diagnostics.Debug.WriteLine("Navigation has started");
            System.Diagnostics.Debug.WriteLine($"Will cancel: {ViewModel.IsCancelled}");

            e.Cancel = ViewModel.IsCancelled;
        }

        private void FormsWebView_OnNavigationCompleted(object sender, string e)
        {
            System.Diagnostics.Debug.WriteLine("Navigation has completed");
        }

        private void FormsWebView_OnContentLoaded(object sender, System.EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Content has loaded");
        }

        private void FormsWebView_OnNavigationError(object sender, int e)
        {
            System.Diagnostics.Debug.WriteLine($"An error was thrown with code: {e}");
        }
    }
}