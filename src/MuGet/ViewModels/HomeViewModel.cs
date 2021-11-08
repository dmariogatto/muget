using MuGet.Localisation;
using MuGet.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
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

        private bool _isFirstLoad = true;
        public bool IsFirstLoad
        {
            get => _isFirstLoad;
            set => SetProperty(ref _isFirstLoad, value);
        }

        public ObservableRangeCollection<PackageMetadata> RecentPackages { get; private set; }
        public ObservableRangeCollection<PackageMetadata> FavouritePackages { get; private set; }

        public AsyncCommand<CancellationToken> LoadCommand { get; private set; }

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
                await NuGetService.GetNuGetSourceAsync(cancellationToken);

                var recents = NuGetService.GetRecentPackages();
                var favourites = NuGetService.GetFavouritePackages();

                var metadataTasks = new Dictionary<string, Task<PackageMetadata>>();

                void addMetadataTask(string packageId)
                {
                    if (!string.IsNullOrEmpty(packageId) && !metadataTasks.ContainsKey(packageId))
                        metadataTasks[packageId] = NuGetService.GetPackageMetadataAsync(packageId, cancellationToken, true);
                }

                foreach (var i in recents)
                    addMetadataTask(i.PackageId);
                foreach (var i in favourites)
                    addMetadataTask(i.PackageId);

                await Task.WhenAll(metadataTasks.Values);

                var recentResults =
                    (from r in recents
                     join kv in metadataTasks on r.PackageId equals kv.Key
                     where kv.Value.IsCompletedSuccessfully && kv.Value.Result != null
                     select kv.Value.Result).ToList();
                var favResults =
                    (from f in favourites
                     join kv in metadataTasks on f.PackageId equals kv.Key
                     where kv.Value.IsCompletedSuccessfully && kv.Value.Result != null
                     select kv.Value.Result).ToList();

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
                IsFirstLoad = false;
                IsBusy = false;
            }
        }
    }
}