using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
            var expiresDate = DateTime.Now;
            expiresDate = expiresDate.AddDays(10);

            Cookie cookie = new Cookie();
            cookie.Name = "testCookie";
            cookie.Value = "testCookieValue";
            cookie.Domain = (new Uri(localContent.Source)).Host;
            cookie.Expired = false;
            cookie.Expires = expiresDate;
            cookie.Path = "/";

            Cookie cookie2 = new Cookie();
            cookie2.Name = "testCookie1";
            cookie2.Value = "testCookieValue1";
            cookie2.Domain = (new Uri(localContent.Source)).Host;
            cookie2.Expired = false;
            cookie2.Expires = DateTime.Now;

            var str = await localContent.SetCookieAsync(cookie);
            var str2 = await localContent.SetCookieAsync(cookie2);

            Debug.WriteLine($"Cookie is {str} and {str2}");
        }

        private async void GetCookieClicked(object sender, EventArgs e)
        {
            
            var str = await localContent.GetCookieAsync("data");

            Debug.WriteLine($"Cookie is {str}");

            var str2 = await localContent.GetCookieAsync("dataOther");

            Debug.WriteLine($"Cookie is {str2}");
        }

        private async void GetAllCookiesClicked(object sender, EventArgs e) {
            var str = await localContent.GetAllCookiesAsync();

            Debug.WriteLine($"All cookies {str}");
        }

        void OnRefreshPageClicked(object sender, EventArgs e)
        {
            localContent.Refresh();
        }
    }
}