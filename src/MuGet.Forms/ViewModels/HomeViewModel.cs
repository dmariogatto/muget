using MvvmHelpers;
using MvvmHelpers.Commands;
using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using MuGet.Forms.Views;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.StateSquid;

namespace MuGet.Forms.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel()
        {
            Title = Resources.Home;

            RecentPackages = new ObservableRangeCollection<PackageMetadata>();
            FavouritePackages = new ObservableRangeCollection<PackageMetadata>();
            LoadCommand = new AsyncCommand<CancellationToken>(Load);
            PackageTappedCommand = new AsyncCommand<PackageMetadata>(PackageTapped);
            SettingsTappedCommand = new AsyncCommand(SettingsTapped);
            RunJobsCommand = new AsyncCommand(async () =>
            {
                var results = await Shiny.ShinyHost.Resolve<Shiny.Jobs.IJobManager>().RunAll();
            });

            CurrentState = State.Loading;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadCommand.ExecuteAsync(default);
        }
        
        public ObservableRangeCollection<PackageMetadata> RecentPackages { get; private set; }
        public ObservableRangeCollection<PackageMetadata> FavouritePackages { get; private set; }

        public AsyncCommand<CancellationToken> LoadCommand { get; private set; }
        public AsyncCommand<PackageMetadata> PackageTappedCommand { get; private set; }
        public AsyncCommand SettingsTappedCommand { get; private set; }
        public AsyncCommand RunJobsCommand { get; private set; }

        private async Task Load(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            CurrentState = State.Loading;

            try
            {
                await NuGetService.GetNuGetSource(cancellationToken);

                var recents = NuGetService.GetRecentPackages();
                var favourites = NuGetService.GetFavouritePackages();

                var recentTasks = recents.Select(i => NuGetService.GetPackageMetadata(i.PackageId, cancellationToken));
                await Task.WhenAll(recentTasks);
                var favouriteTasks = favourites.Select(i => NuGetService.GetPackageMetadata(i.PackageId, cancellationToken));
                await Task.WhenAll(favouriteTasks);

                RecentPackages.ReplaceRange(recentTasks.Select(t => t.Result));
                FavouritePackages.ReplaceRange(favouriteTasks.Select(t => t.Result));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                IsBusy = false;
                CurrentState = State.None;
            }
        }

        private async Task PackageTapped(PackageMetadata package)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (package != null)
            {
                try
                {
                    await Shell.GoToAsync($"{PackagePage.RouteName}?{PackagePage.PackageIdUrlQueryProperty}={WebUtility.UrlEncode(package.Id)}&{PackagePage.VersionQueryProperty}={WebUtility.UrlEncode(package.Version)}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            IsBusy = false;            
        }

        private async Task SettingsTapped()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await Shell.GoToAsync(SettingsPage.RouteName);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
