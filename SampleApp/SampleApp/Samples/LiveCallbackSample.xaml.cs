using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LiveCallbackSample : ContentPage
    {
        public LiveCallbackSample()
        {
            InitializeComponent();
        }

        void AddCallback(object sender, EventArgs e)
        {
            WebView.AddLocalCallback("localCallback", HandleCallback);
        }

        void CallCallback(object sender, EventArgs e)
        {
            WebView.InjectJavascriptAsync("localCallback('Hello World');").ConfigureAwait(false);
        }

        void HandleCallback(string obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got callback: {obj}");
        }
    }
}