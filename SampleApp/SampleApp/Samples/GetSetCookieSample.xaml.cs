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

            // This cookie will expire after 10 days
            Cookie cookie = new Cookie();
            cookie.Name = "testCookie";
            cookie.Value = "testCookieValue";
            cookie.Domain = (new Uri(localContent.Source)).Host;
            cookie.Expired = false;
            cookie.Expires = expiresDate;
            cookie.Path = "/";


            // This cookie will expire 30 seconds after it is set
            Cookie cookie2 = new Cookie();
            cookie2.Name = "testCookie1";
            cookie2.Value = "testCookieValue1";
            cookie2.Domain = (new Uri(localContent.Source)).Host;
            cookie2.Expired = false;
            cookie2.Expires = DateTime.Now.AddSeconds(30);

            var str = await localContent.SetCookieAsync(cookie);
            var str2 = await localContent.SetCookieAsync(cookie2);

        }

        private async void GetCookieClicked(object sender, EventArgs e)
        {
            var str = await localContent.GetCookieAsync("testCookie");
            var str2 = await localContent.GetCookieAsync("testCookie1");
        }

        private async void GetAllCookiesClicked(object sender, EventArgs e) {
            var str = await localContent.GetAllCookiesAsync();
        }

        void OnRefreshPageClicked(object sender, EventArgs e)
        {
            localContent.Refresh();
        }
    }
}