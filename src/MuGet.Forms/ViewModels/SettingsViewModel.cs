using MuGet.Forms.Localisation;

namespace MuGet.Forms.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Title = Resources.Settings;
        }

        public bool IncludePrerelease
        {
            get => NuGetService.IncludePrerelease;
            set
            {
                NuGetService.IncludePrerelease = value;
                OnPropertyChanged(nameof(IncludePrerelease));
            }
        }

        public bool Notifications
        {
            get => NuGetService.Notifications;
            set
            {
                NuGetService.Notifications = value;

                if (value)
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Schedule(ShinyStartup.NuGetJobInfo);
                }
                else
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Cancel(ShinyStartup.NuGetJobInfo.Identifier);
                }

                OnPropertyChanged(nameof(Notifications));
            }
        }
    }
}
