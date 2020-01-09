using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.StateSquid;

namespace MuGet.Forms.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private bool _isFirstLoad = true;

        public HomeViewModel()
        {
            Title = Resources.Home;

            RecentPackages = new ObservableRangeCollection<PackageMetadata>();
            FavouritePackages = new ObservableRangeCollection<PackageMetadata>();
            LoadCommand = new AsyncCommand<CancellationToken>(Load);
            
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
        
        private async Task Load(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (_isFirstLoad)
            {
                CurrentState = State.Loading;
                _isFirstLoad = false;
            }

            try
            {
                await NuGetService.GetNuGetSource(cancellationToken);

                var recents = NuGetService.GetRecentPackages();
                var favourites = NuGetService.GetFavouritePackages();

                var recentTasks = recents.Select(i => NuGetService.GetPackageMetadata(i.PackageId, cancellationToken, true));
                await Task.WhenAll(recentTasks);
                var favouriteTasks = favourites.Select(i => NuGetService.GetPackageMetadata(i.PackageId, cancellationToken, true));
                await Task.WhenAll(favouriteTasks);

                var recentResults = recentTasks
                    .Select(t => t.Result)
                    .Where(m => m != null).ToList();
                var favResults = favouriteTasks
                    .Select(t => t.Result)
                    .Where(m => m != null)
                    .OrderByDescending(m => m.TotalDownloads).ToList();

                if (!Enumerable.SequenceEqual(recentResults, RecentPackages))
                    RecentPackages.ReplaceRange(recentResults);
                if (!Enumerable.SequenceEqual(favResults, FavouritePackages))
                    FavouritePackages.ReplaceRange(favResults);
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
    }
}
