using System;
using Xam.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HeadersSample : ContentPage
    {
        public HeadersSample()
        {
            InitializeComponent();

            if (!FormsWebView.GlobalRegisteredHeaders.ContainsKey("default-Global"))
                FormsWebView.GlobalRegisteredHeaders.Add("default-Global", "default");

            if (!WebView.LocalRegisteredHeaders.ContainsKey("default-Local"))
                WebView.LocalRegisteredHeaders.Add("default-Local", "default");
        }

        void OnGlobalAdd(object sender, EventArgs e)
        {
            var globalKey = GlobalKey.Text;
            var globalValue = GlobalValue.Text;

            if (string.IsNullOrWhiteSpace(globalKey) || string.IsNullOrWhiteSpace(globalValue)) return;
            FormsWebView.GlobalRegisteredHeaders.Add(globalKey, globalValue);

            GlobalKey.Text = "";
            GlobalValue.Text = "";
        }

        void OnLocalAdd(object sender, EventArgs e)
        {
            var localKey = LocalKey.Text;
            var localValue = LocalValue.Text;

            if (string.IsNullOrWhiteSpace(localKey) || string.IsNullOrWhiteSpace(localValue)) return;
            WebView.LocalRegisteredHeaders.Add(localKey, localValue);

            LocalKey.Text = "";
            LocalValue.Text = "";
        }

        void OnReloadClicked(object sender, EventArgs e)
        {
            WebView.Refresh();
        }
    }
}