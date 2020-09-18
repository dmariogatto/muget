using MuGet.Localisation;
using MuGet.Models;
using MuGet.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.StoreReview.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IVersionTracking _versionTracking;
        private readonly IEmail _email;

        private readonly IStoreReview _storeReview;

        private readonly IMuGetPackageService _muGetPackageService;

        public SettingsViewModel(
            IBrowser browser,
            ILauncher launcher,
            IDeviceInfo deviceInfo,
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
            var message = new EmailMessage
            {
                Subject = string.Format("MuGet Feedback ({0})", _deviceInfo.Platform),
                Body = string.Empty,
                To = new List<string>(1) { FeedbackEmail },
            };

            const string gmailScheme = "googlegmail://";
            const string gmail = "Gmail";
            const string appleMail = "Mail";

            if (_deviceInfo.Platform == DevicePlatform.iOS &&
                await _launcher.CanOpenAsync(gmailScheme))
            {
                var option = await Dialogs.ActionSheetAsync(Resources.SendFeedback, Resources.Cancel, null, null, gmail, appleMail);

                if (!string.IsNullOrEmpty(option))
                {
                    switch (option)
                    {
                        case gmail:
                            await _launcher.TryOpenAsync($"{gmailScheme}co?subject={message.Subject}&body={message.Body}&to={message.To.First()}");
                            break;
                        case appleMail:
                            await _email.ComposeAsync(message);
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {
                await _email.ComposeAsync(message);
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
