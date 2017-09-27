using System;
using Xam.Plugin.WebView.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class BackForwardSample : ContentPage
	{
		public BackForwardSample()
		{
			InitializeComponent ();
		}

        void OnGoClicked(object sender, EventArgs e)
        {
            var path = UrlText.Text;
            WebContent.Source = path;
        }

        void BackClicked(object sender, EventArgs e)
        {
            WebContent.GoBack();
        }

        void ForwardClicked(object sender, EventArgs e)
        {
            WebContent.GoForward();
        }
    }
}