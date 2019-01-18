using System;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TestParallelInjectJavascriptAsyncInvocationsSample : ContentPage
    {
        public TestParallelInjectJavascriptAsyncInvocationsSample()
        {
            InitializeComponent();
            stringContent.Source = @"
<!doctype html>
<html>
    <body><h1>This is a HTML string</h1></body>
</html>
            ";
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            const string invocation1ExpectedResult = "Invocation1";
            const string invocation2ExpectedResult = "Invocation2";
            var invocation1Task = stringContent.InjectJavascriptAsync($@"
var delay = 3000; /* 3 seconds */
var start = new Date().getTime();
while (new Date().getTime() < start + delay);
'{invocation1ExpectedResult}'");
            var invocation2Task = stringContent.InjectJavascriptAsync($"'{invocation2ExpectedResult}'");

            var invocation1Result = await invocation1Task;
            var invocation2Result = await invocation2Task;
            var didPass =
                invocation1Result == invocation1ExpectedResult &&
                invocation2Result == invocation2ExpectedResult;

            await DisplayAlert("Parallel invocations test result",
                (didPass
                    ? "PASSED"
                    : $"FAILED")
                + $"\r\n\r\ninvocation1Result: {invocation1Result}\r\ninvocation2Result: {invocation2Result}",
                "OK");
        }
    }
}