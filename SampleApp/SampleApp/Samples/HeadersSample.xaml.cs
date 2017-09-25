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
    public partial class HeadersSample : ContentPage
    {
        public HeadersSample()
        {
            InitializeComponent();

            FoWebView.LocalRegisteredHeaders.Add("Testing", "Hello World!");
        }
    }
}