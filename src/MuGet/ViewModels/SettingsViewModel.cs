using MuGet.Localisation;
using MuGet.Models;
using MuGet.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace MuGet.ViewModels
{
    public enum SettingItem
    {
        RateApp,
        SendFeedback,
        ViewGitHub,
        NuGet,
    }

    public class SettingsViewModel : BaseViewModel
    {
        private const string AndroidAppId = "com.dgatto.muget";
        private const string AppleAppId = "1489637407";
        private const string FeedbackEmail = "outtaapps@gmail.com";
        private const string GitHubRepo = "https://github.com/dmariogatto/muget";
        private const string NuGetUrl = "https://nuget.org/";

        private readonly IBrowser _browser;
        private readonly ILauncher _launcher;
        private readonly IDeviceInfo _deviceInfo;
        private readonly IAppInfo _appInfo;
        private readonly IVersionTracking _versionTracking;
        private readonly IEmail _email;

        private readonly IStoreReview _storeReview;

        private readonly IMuGetPackageService _muGetPackageService;

        public SettingsViewModel(
            IBrowser browser,
            ILauncher launcher,
            IDeviceInfo deviceInfo,
            IAppInfo appInfo,
            IVersionTracking versionTracking,
            IEmail email,
            IStoreReview storeReview,
            IMuGetPackageService muGetPackageService,
            IBvmConstructor bvmConstructor) : base(bvmConstructor)
        {
            Title = Resources.Settings;

            _browser = browser;
            _launcher = launcher;
            _deviceInfo = deviceInfo;
            _appInfo = appInfo;
            _versionTracking = versionTracking;
            _email = email;

            _storeReview = storeReview;

            _muGetPackageService = muGetPackageService;

            MuGetPackages = new ObservableRangeCollection<MuGetPackage>(_muGetPackageService.GetPackages());

            SettingsItemTappedCommand = new AsyncCommand<SettingItem>(SettingsItemTappedAsync);
            PackageTappedCommand = new AsyncCommand<MuGetPackage>(async (p) =>
            {
                if (!string.IsNullOrEmpty(p?.PackageId))
                    await _launcher.TryOpenAsync($"muget://package/{p.PackageId}/");
            });
            ResetNotificationsCommand = new Command(ResetNotifications);
            RunJobsCommand = new AsyncCommand(RunJobsAsync);
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

        public string Version => _versionTracking.CurrentVersion;
        public string Build => _versionTracking.CurrentBuild;

        public AsyncCommand<SettingItem> SettingsItemTappedCommand { get; private set; }
        public AsyncCommand<MuGetPackage> PackageTappedCommand { get; private set; }
        public Command ResetNotificationsCommand { get; private set; }
        public AsyncCommand RunJobsCommand { get; private set; }

        private async Task SettingsItemTappedAsync(SettingItem item)
        {
            switch (item)
            {
                case SettingItem.RateApp:
                    var id = _deviceInfo.Platform == DevicePlatform.iOS ? AppleAppId : AndroidAppId;
                    _storeReview.OpenStoreReviewPage(id);
                    break;
                case SettingItem.SendFeedback:
                    await SendFeedbackAsync();
                    break;
                case SettingItem.ViewGitHub:
                    await _browser.OpenAsync(GitHubRepo);
                    break;
                case SettingItem.NuGet:
                    await _browser.OpenAsync(NuGetUrl);
                    break;
            }
        }

        private async Task SendFeedbackAsync()
        {
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine($"App: {_appInfo.VersionString} | {_appInfo.BuildString}");
                builder.AppendLine($"OS: {_deviceInfo.Platform} | {_deviceInfo.VersionString}");
                builder.AppendLine($"Device: {_deviceInfo.Manufacturer} | {_deviceInfo.Model}");
                builder.AppendLine();
                builder.AppendLine(string.Format(Resources.ItemComma, Resources.AddYourMessageBelow));
                builder.AppendLine("----");
                builder.AppendLine();

                var message = new EmailMessage
                {
                    Subject = string.Format(Resources.FeedbackSubjectItem, _deviceInfo.Platform),
                    Body = builder.ToString(),
                    To = new List<string>(1) { FeedbackEmail },
                };

                await _email.ComposeAsync(message);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                Dialogs.Alert(Resources.EmailDirectly, Resources.UnableToSendEmail, Resources.OK);
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

        private async Task RunJobsAsync()
        {
            try
            {
                var results = await Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().RunAll();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
