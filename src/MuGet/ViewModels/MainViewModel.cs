using MuGet.Localisation;
using MuGet.Models;
using MuGet.Services;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private const int DefaultPageSize = 20;

        private readonly SemaphoreSlim _searchSemaphore = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _searchCancellation;

        public MainViewModel(IBvmConstructor bvmConstructor) : base(bvmConstructor)
        {
            Title = Resources.Search;

            Packages = new ObservableRangeCollection<PackageMetadata>();
            SearchCommand = new AsyncCommand<CancellationToken>((ct) => SearchAsync(SearchText, 0, ct));
            RemainingItemsThresholdReachedCommand = new AsyncCommand(() =>
                _searchCancellation != null
                    ? SearchAsync(SearchText, Packages.Count, _searchCancellation.Token)
                    : Task.FromResult(0));
            PackageTappedCommand = new Command<PackageMetadata>(PackageTapped);
        }

        public bool PackagesLoading => !string.IsNullOrEmpty(SearchText) && IsBusy && !Packages.Any();
        public bool PackagesLoaded => !string.IsNullOrEmpty(SearchText) && (Packages.Any() || !IsBusy);

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value?.Trim());

                _searchCancellation?.Cancel();
                _searchCancellation?.Dispose();
                _searchCancellation = null;

                _searchCancellation = new CancellationTokenSource();
                SearchCommand.ExecuteAsync(_searchCancellation.Token);
            }
        }

        private int _remainingItemsThreshold = -1;
        public int RemainingItemsThreshold
        {
            get => _remainingItemsThreshold;
            set => SetProperty(ref _remainingItemsThreshold, value);
        }

        public ObservableRangeCollection<PackageMetadata> Packages { get; private set; }

        private int _totalHits;
        public int TotalHits
        {
            get => _totalHits;
            set => SetProperty(ref _totalHits, value);
        }

        public AsyncCommand<CancellationToken> SearchCommand { get; private set; }
        public AsyncCommand RemainingItemsThresholdReachedCommand { get; private set; }
        public Command<PackageMetadata> PackageTappedCommand { get; private set; }

        private async Task SearchAsync(string searchText, int skip, CancellationToken cancellationToken)
        {
            RemainingItemsThreshold = -1;

            await _searchSemaphore.WaitAsync();

            if (SearchText == searchText && !cancellationToken.IsCancellationRequested)
            {
                IsBusy = true;

                try
                {
                    if (skip == 0)
                        Packages.Clear();

                    OnPropertyChanged(nameof(PackagesLoading));
                    OnPropertyChanged(nameof(PackagesLoaded));

                    var packages = default(IList<PackageMetadata>);
                    var totalHits = 0;

                    if (!string.IsNullOrWhiteSpace(searchText))
                    {
                        var searchResult = await NuGetService.SearchAsync(searchText, skip, DefaultPageSize, cancellationToken);
                        packages = searchResult.Item2;
                        totalHits = searchResult.Item1;
                    }
                    else
                    {
                        packages = new List<PackageMetadata>(0);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (skip > 0)
                        {
                            // Stop iOS exploding with an NSInternalInconsistencyException
                            foreach (var p in packages)
                            {
                                Packages.Add(p);
                            }
                        }
                        else
                        {                            
                            if (packages?.Any() == true)
                                Packages.ReplaceRange(packages);
                        }

                        TotalHits = totalHits;
                        RemainingItemsThreshold = Packages.Count == TotalHits ? -1 : 5;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
                finally
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        IsBusy = false;                        
                    }

                    OnPropertyChanged(nameof(PackagesLoading));
                    OnPropertyChanged(nameof(PackagesLoaded));
                }
            }

            _searchSemaphore.Release();
        }

        private void PackageTapped(PackageMetadata package)
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (package != null)
            {
                try
                {
                    NuGetService.AddRecentPackage(new RecentPackage()
                    {
                        PackageId = package.Id,
                        IndexUrl = package.IndexUrl,
                        SourceUrl = string.Empty,
                        SortOrder = 0,
                    });                    
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            }

            IsBusy = false;
        }
    }
}
