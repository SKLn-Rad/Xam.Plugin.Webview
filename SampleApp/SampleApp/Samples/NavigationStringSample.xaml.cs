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
    public partial class NavigationStringSample : ContentPage
    {
        NavigationEventsViewModel ViewModel = new NavigationEventsViewModel();

        public NavigationStringSample()
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

            const string PageOne = "<html><body><h1>Page One</h1></body></html>";
            const string PageTwo = "<html><body><h1>Page Two</h1></body></html>";

            string _uri = PageOne;

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
                    if (Uri.Equals(PageOne))
                        Uri = PageTwo;
                    else
                        Uri = PageOne;
                });

                ErrorCommand = new Command(() => Uri = "<bd></asd>");
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }

        private void FormsWebView_OnNavigationStarted(object sender, Xam.Plugin.WebView.Abstractions.Delegates.DecisionHandlerDelegate e)
        {
            System.Diagnostics.Debug.WriteLine("Navigation has started");
            System.Diagnostics.Debug.WriteLine($"Will cancel: {ViewModel.IsCancelled}");

            e.Cancel = ViewModel.IsCancelled;
        }

        private void FormsWebView_OnNavigationCompleted(object sender, System.EventArgs e)
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