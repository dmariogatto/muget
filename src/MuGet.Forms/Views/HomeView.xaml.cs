using MuGet.Forms.Models;
using MuGet.Forms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HomeView : BaseView<HomeViewModel>
    {
        private SettingsPage _settingsPage;

        public HomeView() : base()
        {
            InitializeComponent();
        }

        private void PackageTapped(object sender, System.EventArgs e)
        {
            if (sender is View v && v.BindingContext is PackageMetadata metadata)
            {
                var packagePage = new PackagePage();
                packagePage.PackageId = metadata.Id;
                packagePage.Version = metadata.Version;

                Navigation.PushAsync(packagePage);
            }
        }

        private void SettingsTapped(object sender, System.EventArgs e)
        {
            if (_settingsPage == null)
                _settingsPage = new SettingsPage();

            Navigation.PushAsync(_settingsPage);
        }
    }
}