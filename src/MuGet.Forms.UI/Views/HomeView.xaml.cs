using MuGet.Forms.UI.Extentions;
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
                _ = Navigation.PushPageFactoryAsync(() => new PackagePage
                {
                    PackageId = metadata.Id,
                    Version = metadata.Version
                });
            }
        }

        private void SettingsTapped(object sender, System.EventArgs e)
        {
            _ = Navigation.PushPageFactoryAsync(() => _settingsPage ??= new SettingsPage());
        }
    }
}