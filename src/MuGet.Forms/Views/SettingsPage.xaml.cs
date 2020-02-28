using MuGet.Forms.ViewModels;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : BasePage<SettingsViewModel>
    {
        public SettingsPage() : base()
        {
            InitializeComponent();

            ResetNotificationsCell.Description = Localisation.Resources.ResetNotificationsDescription;
            RunAllJobsCell.Description = Localisation.Resources.RunAllJobsDescription;
        }
    }
}