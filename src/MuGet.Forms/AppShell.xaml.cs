using MuGet.Forms.Views;
using Xamarin.Forms;

namespace MuGet.Forms
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == Device.Android)
            {
                BackgroundColor = (Color)Application.Current.Resources["AccentColor"];
            }

            Routing.RegisterRoute(PackagePage.RouteName, typeof(PackagePage));
            Routing.RegisterRoute(SettingsPage.RouteName, typeof(SettingsPage));
        }
    }
}
