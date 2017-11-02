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
    public partial class ScrollToSample : ContentPage
    {
        public ScrollToSample()
        {
            InitializeComponent();
        }

        void ScrollUpButtonClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => await SampleWebview.InjectJavascriptAsync("window.scrollTo(0,document.body.scrollHeight);"));
        }

        void ScrollDownButtonClicked(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () => await SampleWebview.InjectJavascriptAsync("window.scrollTo(0,0);"));
        }
    }
}