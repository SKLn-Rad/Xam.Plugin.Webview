using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp.Samples
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GetSetCookieSample : ContentPage
    {
        public GetSetCookieSample()
        {
            InitializeComponent();
        }
        private async void SetCookieClicked(object sender, EventArgs e)
        {
            var str = await localContent.SetCookieValue("data", "This is a sessioncookie");
            var str2 = await localContent.SetCookieValue("dataOther", "This is a kind of permanent cookie", 10000);

            Debug.WriteLine($"Cookie is {str} and {str2}");
        }

        private async void GetCookieClicked(object sender, EventArgs e)
        {
            var str = await localContent.GetCookieValue("data");

            Debug.WriteLine($"Cookie is {str}");
            DisplayAlert("CookieValue", str, "ok");

            var str2 = await localContent.GetCookieValue("dataOther");

            Debug.WriteLine($"Cookie is {str2}");
            DisplayAlert("CookieValue", str2, "ok");
        }

        private async void GetAllCookiesClicked(object sender, EventArgs e) {
            var str = await localContent.GetAllCookiesValue();

            Debug.WriteLine($"All cookies {str}");
        }

        void OnRefreshPageClicked(object sender, EventArgs e)
        {
            localContent.Refresh();
        }
    }
}