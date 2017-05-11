using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xam.Plugin.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SampleApp
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            ConfigureCallbacks();
            ConfigureEvents();
        }

        void ConfigureEvents()
        {
            internetContent.OnContentLoaded += (cobj) =>
            {
                internetContent.InjectJavascript("csDebug('Internet WebView Loaded Successfully!')");
            };

            localContent.OnContentLoaded += (cobj) =>
            {
                localContent.InjectJavascript("csDebug('Local WebView Loaded Successfully!')");
            };
        }

        void ConfigureCallbacks()
        {
            localContent.RegisterGlobalCallback("csDebug", (str) => Debug.WriteLine("Got from JS: " + str));

            localContent.RegisterLocalCallback("csLoad", (str) => HandleForm(str));
            localContent.RegisterLocalCallback("csInject", (str) => HandleInject(str));

            localContent.RegisterLocalCallback("csCallback", (str) => Debug.WriteLine("This should NOT be called!"));
            internetContent.RegisterLocalCallback("csCallback", (str) => Debug.WriteLine(string.Format("Internet WebView Says: {0}", str)));
        }

        void HandleInject(string str)
        {
            internetContent.InjectJavascript(string.Format("csCallback('{0}');", str));
        }

        void HandleForm(string str)
        {
            Debug.WriteLine("Got form data: " + str);

            if (Uri.IsWellFormedUriString(str, UriKind.Absolute))
                internetContent.Source = str;
        }
    }

}
