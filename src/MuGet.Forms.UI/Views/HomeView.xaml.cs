using MuGet.Models;
using MuGet.ViewModels;
using Xamarin.Forms;

namespace MuGet.Forms.UI.Views
{
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
                var packagePage = new PackagePage
                {
                    PackageId = metadata.Id,
                    Version = metadata.Version
                };

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