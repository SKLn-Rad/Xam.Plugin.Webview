using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EmailDataSample : ContentPage
    {
        public EmailDataSample()
        {
            InitializeComponent();
            stringContent.Source = @"
<!doctype html>
<html>
    <body>
        <h1>This is a HTML string</h1>
        <a href='mailto: someone @example.com'>Click here to mail someone</a>
    </body>
</html>
            ";
        }
    }
}
