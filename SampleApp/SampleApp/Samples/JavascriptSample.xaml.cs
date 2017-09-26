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
    public partial class JavascriptSample : ContentPage
    {
        public JavascriptSample()
        {
            InitializeComponent();

            WebView.LocalRegisteredCallbacks.Add("test", DisplayAction);
        }

        void DisplayAction(string obj)
        {

        }
    }
}