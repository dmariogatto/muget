using MuGet.Forms.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MuGet.Forms.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : BasePage<SettingsViewModel>
    {
        public SettingsPage() : base()
        {
            InitializeComponent();

            NuGetCell.DescriptionFontSize =
                ResetNotificationsCell.DescriptionFontSize =
                    RunAllJobsCell.DescriptionFontSize =
                        Device.GetNamedSize(NamedSize.Caption, typeof(Label));

            ResetNotificationsCell.Description = Localisation.Resources.ResetNotificationsDescription;
            RunAllJobsCell.Description = Localisation.Resources.RunAllJobsDescription;
        }
    }
}