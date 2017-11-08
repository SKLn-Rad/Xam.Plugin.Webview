using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JavascriptReturnSample : ContentPage
    {
        public JavascriptReturnSample()
        {
            InitializeComponent();
            
            TestWebview.AddLocalCallback("cslog", HandleLog);
            TestWebview.AddLocalCallback("csfakecode", PerformFakeAction);
        }

        private void PerformFakeAction(string obj)
        {
            TestWebview.InjectJavascriptAsync("returnValue = \"NewReturnValue\";").ConfigureAwait(false);

            DateTime time = DateTime.Now.AddSeconds(5);
            SpinWait.SpinUntil(() => DateTime.Now > time);
        }

        private void HandleLog(string obj)
        {
            System.Diagnostics.Debug.WriteLine($"Got from JS: {obj}");
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await TestWebview.InjectJavascriptAsync("doWork()");
        }
    }
}