using System;
using System.Threading.Tasks;
using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using MuGet.Forms.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview;
using Xamarin.Essentials;

namespace MuGet.Forms.ViewModels
{
    public enum SettingItem
    {
        RateApp,
        SendFeedback,
        ViewGitHub
    }

    public class SettingsViewModel : BaseViewModel
    {
        private const string AndroidAppId = "com.dgatto.muget";
        private const string AppleAppId = "1489637407";
        private const string FeedbackEmail = "outtaapps@gmail.com";
        private const string GitHubRepo = "https://github.com/dmariogatto/muget";

        private readonly IMuGetPackageService _muGetPackageService;

        public SettingsViewModel()
        {
            Title = Resources.Settings;

            _muGetPackageService = Shiny.ShinyHost.Resolve<IMuGetPackageService>();
            MuGetPackages = new ObservableRangeCollection<MuGetPackage>(_muGetPackageService.GetPackages());

            SettingsItemTappedCommand = new AsyncCommand<SettingItem>(SettingsItemTapped);
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

        public ObservableRangeCollection<MuGetPackage> MuGetPackages { get; private set; }

        public AsyncCommand<SettingItem> SettingsItemTappedCommand { get; private set; }
        public Command ResetNotificationsCommand { get; private set; }
        public AsyncCommand RunJobsCommand { get; private set; }

        private async Task SettingsItemTapped(SettingItem item)
        {
            switch (item)
            {
                case SettingItem.RateApp:
                    var id = DeviceInfo.Platform == DevicePlatform.iOS ? AppleAppId : AndroidAppId;
                    CrossStoreReview.Current.OpenStoreReviewPage(id);
                    break;
                case SettingItem.SendFeedback:
                    await Email.ComposeAsync("MuGet Feedback", string.Empty, FeedbackEmail);
                    break;
                case SettingItem.ViewGitHub:
                    await Browser.OpenAsync(GitHubRepo);
                    break;
            }
        }

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
