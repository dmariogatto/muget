using MuGet.ViewModels;

namespace MuGet.Forms.UI.Views
{    
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