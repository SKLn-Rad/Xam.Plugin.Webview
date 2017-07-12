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
    public partial class NavigationStack : ContentPage
    {
        public NavigationStack()
        {
            InitializeComponent();
        }

        void OnNavigate(object sender, EventArgs e) => XamWebview.Source = EntryField.Text;
        void GoBack(object sender, EventArgs e) => XamWebview.GoBack();
        void GoForward(object sender, EventArgs e) => XamWebview.GoForward();
    }
}