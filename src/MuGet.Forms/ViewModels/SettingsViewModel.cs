using System;
using MuGet.Forms.Localisation;
using MvvmHelpers.Commands;

namespace MuGet.Forms.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        public SettingsViewModel()
        {
            Title = Resources.Settings;

            ResetNotificationsCommand = new Command(ResetNotifications);
            RunJobsCommand = new AsyncCommand(async () =>
            {
                var results = await Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().RunAll();
            });
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

        public bool NewReleaseNotifications
        {
            get => NuGetService.NewReleaseNotifications;
            set
            {
                NuGetService.NewReleaseNotifications = value;

                if (value)
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Schedule(ShinyStartup.NuGetJobInfo);
                }
                else
                {
                    Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().Cancel(ShinyStartup.NuGetJobInfo.Identifier);
                }

                OnPropertyChanged(nameof(NewReleaseNotifications));
            }
        }

        public Command ResetNotificationsCommand { get; private set; }
        public AsyncCommand RunJobsCommand { get; private set; }

        private void ResetNotifications()
        {
            var favourites = NuGetService.GetFavouritePackages();
            foreach (var f in favourites)
            {
                f.Version = string.Empty;
                f.Published = DateTime.MinValue;
                NuGetService.UpsertFavouritePackage(f);
            }
        }
    }
}
