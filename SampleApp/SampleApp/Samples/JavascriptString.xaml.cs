using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xam.Plugin.WebView.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JavascriptString : ContentPage
    {
        public JavascriptString()
        {
            InitializeComponent();

            WebContent.Source = "<!doctype html><html><head></head><body><h1>String Data Example</h1></body></html>";
            InjectionText.Text = "document.body.style.backgroundColor = \"red\";";
            GlobalText.Text = "globalCallback('Hello from Javascript!');";
            LocalText.Text = "localCallback('Hello from Javascript!');";

            FormsWebView.AddGlobalCallback("globalCallback", GlobalCallback);
            WebContent.AddLocalCallback("localCallback", LocalCallback);
        }

        void GlobalCallback(JToken obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got global callback: {obj}");
        }

        void LocalCallback(JToken obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got local callback: {obj}");
        }

        async void OnInjectionClicked(object sender, EventArgs e)
        {
            var text = InjectionText.Text;
            var response = await WebContent.InjectJavascriptAsync(text);

            System.Diagnostics.Debug.WriteLine($"Got javascript response: {response}");
        }

        void GlobalCallbackClicked(object sender, EventArgs e)
        {
            var text = GlobalText.Text;
            WebContent.InjectJavascriptAsync(text).ConfigureAwait(false);
        }

        void LocalCallbackClicked(object sender, EventArgs e)
        {
            var text = LocalText.Text;
            WebContent.InjectJavascriptAsync(text).ConfigureAwait(false);
        }
    }
}