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
	public partial class BackForwardSample : ContentPage
	{
		public BackForwardSample()
		{
			InitializeComponent ();
		}

        void OnGoClicked(object sender, EventArgs e)
        {
            var path = UrlText.Text;
            WebView.Source = path;
        }

        void BackClicked(object sender, EventArgs e)
        {
            WebView.GoBack();
        }

        void ForwardClicked(object sender, EventArgs e)
        {
            WebView.GoForward();
        }
    }
}