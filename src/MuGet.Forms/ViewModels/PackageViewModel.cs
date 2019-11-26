using MvvmHelpers;
using MvvmHelpers.Commands;
using MuGet.Forms.Controls;
using MuGet.Forms.Localisation;
using MuGet.Forms.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms.StateSquid;

namespace MuGet.Forms.ViewModels
{
    public class PackageViewModel : BaseViewModel
    {
        private readonly Stack<PackageViewState> _navStack;

        private CancellationTokenSource _cancellation;

        public PackageViewModel()
        {
            _navStack = new Stack<PackageViewState>();

            CatalogEntries = new ObservableRangeCollection<CatalogEntry>();
            LoadCommand = new AsyncCommand<CancellationToken>(Load);
            DependancyTappedCommand = new Command<Dependency>(DependencyTapped);
            EntryTappedCommand = new AsyncCommand<CatalogEntry>((e) => EntryTapped(e, _cancellation.Token));
            LinkTappedCommand = new AsyncCommand<LinkType>(LinkTapped);
            FavouriteCommand = new AsyncCommand<CatalogEntry>(Favourite);            
            BackCommand = new AsyncCommand(Back);
            CloseCommand = new AsyncCommand(Close);

            CatalogEntries.CollectionChanged += (sender, args) =>
            {
                if (sender is ObservableRangeCollection<CatalogEntry> entries)
                {
                    var totalDownloads = entries.Sum(e => e.Downloads);
                    var nowUtc = DateTime.UtcNow;
                    var firstPublish = entries.LastOrDefault()?.Published ?? nowUtc;
                    var daysDiff = (nowUtc - firstPublish).TotalDays;                    
                    AvgDownloads = daysDiff > 0
                        ? Convert.ToInt32(totalDownloads / daysDiff)
                        : 0;
                }
            };
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadPackage();
        }

        public string PreviousPackageId => _navStack.Any() 
            ? _navStack.Peek().Entry?.Id ?? string.Empty 
            : string.Empty;

        private string _packageId;
        public string PackageId
        {
            get => _packageId;
            set => SetProperty(ref _packageId, value);
        }

        private string _version;
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        private CatalogEntry _entry;
        public CatalogEntry Entry
        {
            get => _entry;
            set => SetProperty(ref _entry, value);
        }

        private CatalogData _entryData;
        public CatalogData EntryData
        {
            get => _entryData;
            set
            {
                SetProperty(ref _entryData, value);
                Dependencies = EntryData
                    ?.DependencyGroups
                    ?.Select(dg => new DependencyGrouping(dg.TargetFramework, dg.Dependencies ?? new List<Dependency>(1) { new Dependency() }))
                    ?.ToList() ?? new List<DependencyGrouping>(0);
            }
        }

        private List<DependencyGrouping> _dependencies;
        public List<DependencyGrouping> Dependencies
        {
            get => _dependencies;
            set => SetProperty(ref _dependencies, value);
        }

        private PackageMetadata _metadata;
        public PackageMetadata Metadata
        {
            get => _metadata;
            set => SetProperty(ref _metadata, value);
        }

        private int _avgDownloads;
        public int AvgDownloads
        {
            get => _avgDownloads;
            set => SetProperty(ref _avgDownloads, value);
        }

        public ObservableRangeCollection<CatalogEntry> CatalogEntries { get; private set; }

        public AsyncCommand<CancellationToken> LoadCommand { get; private set; }
        public Command<Dependency> DependancyTappedCommand { get; private set; }
        public AsyncCommand<CatalogEntry> EntryTappedCommand { get; private set; }
        public AsyncCommand<LinkType> LinkTappedCommand { get; private set; }
        public AsyncCommand<CatalogEntry> FavouriteCommand { get; private set; }
        public AsyncCommand BackCommand { get; private set; }
        public AsyncCommand CloseCommand { get; private set; }

        private void LoadPackage()
        {
            _cancellation?.Cancel();
            _cancellation?.Dispose();
            _cancellation = null;

            _cancellation = new CancellationTokenSource();
            LoadCommand.ExecuteAsync(_cancellation.Token);
        }

        private async Task Load(CancellationToken cancellationToken)
        {
            if (IsBusy)
                return;

            IsBusy = true;
            CurrentState = State.Loading;

            try
            {
                Entry = null;
                CatalogEntries.Clear();

                if (!string.IsNullOrEmpty(PackageId))
                {
                    var entries = await NuGetService.GetCatalogEntries(PackageId, cancellationToken);
                    var latest = entries.FirstOrDefault(e => e.Version == Version) ?? entries.FirstOrDefault();

                    if (latest != null)
                    {
                        var entryTask = NuGetService.GetCatalogData(latest.IndexUrl, cancellationToken);
                        var metadataTask = NuGetService.GetPackageMetadata(latest.Id, cancellationToken, true);
                        await Task.WhenAll(entryTask, metadataTask);

                        Metadata = await metadataTask;

                        var savedPackage = NuGetService.GetFavouritePackages().FirstOrDefault(i => i.PackageId == latest.Id);
                        if (savedPackage != null)
                        {
                            savedPackage.TotalDownloads = Metadata.TotalDownloads;
                            NuGetService.AddFavouritePackage(savedPackage);
                            foreach (var e in entries)
                            {
                                e.IsFavourite = true;
                            }
                        }

                        foreach (var v in Metadata.Versions)
                        {
                            var e = entries.FirstOrDefault(i => i.Version == v.Version);
                            if (e != null)
                            {
                                e.Downloads = v.Downloads;
                            }
                        }

                        EntryData = await entryTask;

                        Entry = latest;
                        CatalogEntries.ReplaceRange(entries);
                    }
                }
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

        private void DependencyTapped(Dependency dependency)
        {
            if (IsBusy && dependency != null)
                return;

            IsBusy = true;

            try
            {
                var packageId = dependency.Id;
                if (!string.IsNullOrEmpty(packageId))
                {
                    var currentState = new PackageViewState(
                        PackageId,
                        Entry,
                        EntryData,
                        Metadata,
                        CatalogEntries.ToList());

                    _navStack.Push(currentState);
                    OnPropertyChanged(nameof(PreviousPackageId));

                    PackageId = packageId;
                    Version = dependency.VersionRange?.MinVersion != null
                        ? dependency.VersionRange.MinVersion.ToString()
                        : string.Empty;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                IsBusy = false;
            }

            LoadPackage();
        }

        private async Task EntryTapped(CatalogEntry entry, CancellationToken cancellationToken)
        {
            if (IsBusy || entry == null)
                return;

            IsBusy = true;

            try
            {
                var entryData = await NuGetService.GetCatalogData(entry.IndexUrl, cancellationToken);
                if (entryData != null)
                {
                    Entry = entry;
                    EntryData = entryData;
                }                
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

        private async Task LinkTapped(LinkType type)
        {
            if (Entry != null && EntryData != null)
            {
                switch (type)
                {
                    case LinkType.NuGet:
                    case LinkType.FuGet:
                        var source = await NuGetService.GetNuGetSource(default);
                        if (!string.IsNullOrEmpty(source?.PackageDetailsUriTemplate))
                        {
                            var detailsUrl = source.PackageDetailsUriTemplate
                                .Replace("{id}", Entry.Id)
                                .Replace("{version}", Entry.Version);
                            detailsUrl = type == LinkType.FuGet
                                ? detailsUrl.Replace("nuget.org", "fuget.org")
                                : detailsUrl;

                            await Browser.OpenAsync(detailsUrl);
                        }
                        break;
                    case LinkType.Project:
                        if (!string.IsNullOrEmpty(Entry.ProjectUrl))
                        {
                            await Browser.OpenAsync(Entry.ProjectUrl);
                        }
                        break;
                    case LinkType.Repository:
                        //await Browser.OpenAsync(EntryData.Repository);
                        break;
                    case LinkType.License:
                        if (!string.IsNullOrEmpty(Entry.LicenseUrl))
                        {
                            await Browser.OpenAsync(Entry.LicenseUrl);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private async Task Favourite(CatalogEntry entry)
        {
            if (entry != null)
            {
                if (entry.IsFavourite)
                {
                    var favourite = NuGetService.GetFavouritePackages();
                    var sp = favourite.FirstOrDefault(f => f.PackageId == entry.Id && f.IndexUrl == entry.IndexUrl);
                    if (sp != null)
                    {
                        NuGetService.RemoveFavouritePackage(sp);
                    }
                }
                else
                {
                    var includePrerelease = NuGetService.IncludePrerelease;
                    var lastestEntry = CatalogEntries.FirstOrDefault(e => includePrerelease || !e.PackVersion.IsPrerelease);
                    var source = await NuGetService.GetNuGetSource(default);
                    var sp = new FavouritePackage()
                    {
                        PackageId = entry.Id,
                        IndexUrl = entry.IndexUrl,
                        SourceUrl = source.Source,
                        Version = lastestEntry?.Version ?? string.Empty,
                        Published = lastestEntry?.Published ?? DateTime.MinValue,
                        SortOrder = 0,
                    };

#if DEBUG
                    sp.Version = string.Empty;
                    sp.Published = DateTime.MinValue;
#endif

                    NuGetService.AddFavouritePackage(sp);
                }

                var isFav = !entry.IsFavourite;
                foreach (var e in CatalogEntries)
                {
                    e.IsFavourite = isFav;
                }          
            }
        }

        private async Task Back()
        {
            try
            {
                _cancellation?.Cancel();

                if (_navStack.Any())
                {
                    var state = _navStack.Pop();
                    OnPropertyChanged(nameof(PreviousPackageId));

                    PackageId = string.Empty;

                    _packageId = state.PackageId;
                    CatalogEntries.ReplaceRange(state.Entries);
                    Entry = state.Entry;
                    EntryData = state.EntryData;
                    Metadata = state.Metadata;
                }
                else
                {
                    await Shell.Navigation.PopAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private async Task Close()
        {
            try
            {
                _cancellation?.Cancel();

                await Shell.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }        

        private class PackageViewState
        {
            public PackageViewState(
                string packageId,
                CatalogEntry entry,
                CatalogData entryData,
                PackageMetadata metadata,
                List<CatalogEntry> entries)
            {
                PackageId = packageId;
                Entry = entry;
                EntryData = entryData;
                Metadata = metadata;
                Entries = entries;
            }

            public string PackageId { get; set; }
            public CatalogEntry Entry { get; set; }
            public CatalogData EntryData { get; set; }
            public PackageMetadata Metadata { get; set; }
            public List<CatalogEntry> Entries { get; set; }
        }
    }
}
