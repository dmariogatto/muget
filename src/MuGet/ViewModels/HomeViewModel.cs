using MuGet.Localisation;
using MuGet.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private bool _isFirstLoad = true;

        public HomeViewModel(IBvmConstructor bvmConstructor) : base(bvmConstructor)
        {
            Title = Resources.Home;

            RecentPackages = new ObservableRangeCollection<PackageMetadata>();
            FavouritePackages = new ObservableRangeCollection<PackageMetadata>();
            LoadCommand = new AsyncCommand<CancellationToken>(LoadAsync);
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadCommand.ExecuteAsync(default);
        }

        public ObservableRangeCollection<PackageMetadata> RecentPackages { get; private set; }
        public ObservableRangeCollection<PackageMetadata> FavouritePackages { get; private set; }

        public AsyncCommand<CancellationToken> LoadCommand { get; private set; }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (_isFirstLoad)
            {
                State = State.Loading;
                _isFirstLoad = false;
            }

            try
            {
                await NuGetService.GetNuGetSourceAsync(cancellationToken);

                var recents = NuGetService.GetRecentPackages();
                var favourites = NuGetService.GetFavouritePackages();

                var recentTasks = recents.Select(i => NuGetService.GetPackageMetadataAsync(i.PackageId, cancellationToken, true));
                await Task.WhenAll(recentTasks);
                var favouriteTasks = favourites.Select(i => NuGetService.GetPackageMetadataAsync(i.PackageId, cancellationToken, true));
                await Task.WhenAll(favouriteTasks);

                var recentResults = recentTasks
                    .Select(t => t.Result)
                    .Where(m => m != null).ToList();
                var favResults = favouriteTasks
                    .Select(t => t.Result)
                    .Where(m => m != null)
                    .ToList();

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
                State = State.None;
            }
        }
    }
}