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
    public partial class NavigationPageStart : ContentPage
    {
        public NavigationPageStart()
        {
            InitializeComponent();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            ((NavigationPage)Application.Current.MainPage).PushAsync(new InternetSample());
        }
    }
}