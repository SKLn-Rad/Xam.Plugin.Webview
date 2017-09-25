using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StringDataSample : ContentPage
    {
        public StringDataSample()
        {
            InitializeComponent();
            stringContent.Source = @"
<!doctype html>
<html>
    <body><h1>This is a HTML string</h1></body>
</html>
            ";
        }
    }
}