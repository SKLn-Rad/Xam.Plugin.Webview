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
    public partial class NavigationPageWipe : ContentPage
    {
        public NavigationPageWipe()
        {
            InitializeComponent();
        }

        void Button_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new InternetSample();
        }
    }
}