using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xam.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JavascriptSample : ContentPage
    {
        public JavascriptSample()
        {
            InitializeComponent();

            InjectionText.Text = "document.body.style.backgroundColor = \"red\";";
            GlobalText.Text = "globalCallback('Hello from Javascript!');";
            LocalText.Text = "localCallback('Hello from Javascript!');";

            FormsWebView.AddGlobalCallback("globalCallback", GlobalCallback);
            WebView.AddLocalCallback("localCallback", LocalCallback);
        }

        void GlobalCallback(string obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got global callback: {obj}");
        }

        void LocalCallback(string obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got local callback: {obj}");
        }

        async void OnInjectionClicked(object sender, EventArgs e)
        {
            var text = InjectionText.Text;
            var response = await WebView.InjectJavascriptAsync(text);

            System.Diagnostics.Debug.WriteLine($"Got javascript response: {response}");
        }

        void GlobalCallbackClicked(object sender, EventArgs e)
        {
            var text = GlobalText.Text;
            WebView.InjectJavascriptAsync(text).ConfigureAwait(false);
        }

        void LocalCallbackClicked(object sender, EventArgs e)
        {
            var text = LocalText.Text;
            WebView.InjectJavascriptAsync(text).ConfigureAwait(false);
        }
    }
}