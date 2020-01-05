using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : BasePage<SettingsViewModel>
    {
        public const string RouteName = "settings";

        public SettingsPage()
        {
            InitializeComponent();
        }
    }
}