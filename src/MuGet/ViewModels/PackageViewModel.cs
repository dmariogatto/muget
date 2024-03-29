﻿using MuGet.Models;
using MvvmHelpers;
using MvvmHelpers.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials.Interfaces;

namespace MuGet.ViewModels
{
    public class PackageViewModel : BaseViewModel
    {
        private readonly IBrowser _browser;

        private CancellationTokenSource _cancellation;

        public PackageViewModel(
            IBrowser browser,
            IBvmConstructor bvmConstructor) : base(bvmConstructor)
        {
            _browser = browser;

            CatalogEntries = new ObservableRangeCollection<CatalogEntry>();
            LoadCommand = new AsyncCommand<CancellationToken>(LoadAsync);
            EntryTappedCommand = new AsyncCommand<CatalogEntry>((e) => EntryTappedAsync(e, _cancellation.Token));
            LinkTappedCommand = new AsyncCommand<LinkType>(LinkTappedAsync);
            FavouriteCommand = new AsyncCommand<CatalogEntry>(FavouriteAsync);

            CatalogEntries.CollectionChanged += (sender, args) =>
            {
                if (sender is ObservableRangeCollection<CatalogEntry> entries)
                {
                    var totalDownloads = entries.Sum(e => e.Downloads);
                    var nowUtc = DateTime.UtcNow;
                    var firstPublish = entries.LastOrDefault()?.Published ?? nowUtc;
                    var daysDiff = (nowUtc - firstPublish).TotalDays;
                    AvgDownloads = daysDiff > 0
                        ? (long)(totalDownloads / daysDiff)
                        : 0;
                }
            };

            IsBusy = true;
        }

        public override void OnAppearing()
        {
            base.OnAppearing();

            LoadPackage();
        }

        private bool _isFirstLoad = true;
        public bool IsFirstLoad
        {
            get => _isFirstLoad;
            set => SetProperty(ref _isFirstLoad, value);
        }

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

        private long _avgDownloads;
        public long AvgDownloads
        {
            get => _avgDownloads;
            set => SetProperty(ref _avgDownloads, value);
        }

        public ObservableRangeCollection<CatalogEntry> CatalogEntries { get; private set; }

        public AsyncCommand<CancellationToken> LoadCommand { get; private set; }
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

        private async Task LoadAsync(CancellationToken cancellationToken)
        {
            if (IsBusy && !IsFirstLoad)
                return;

            IsBusy = true;
            IsFirstLoad = false;

            try
            {
                Clear();

                if (!string.IsNullOrEmpty(PackageId))
                {
                    var entries = await NuGetService.GetCatalogEntriesAsync(PackageId, cancellationToken);
                    var latest = !string.IsNullOrEmpty(Version)
                                 ? entries.FirstOrDefault(e => e.Version == Version) ?? entries.FirstOrDefault()
                                 : entries.FirstOrDefault();

                    if (latest != null)
                    {
                        var entryTask = NuGetService.GetCatalogDataAsync(latest.IndexUrl, cancellationToken);
                        var metadataTask = NuGetService.GetPackageMetadataAsync(latest.Id, cancellationToken, true);
                        await Task.WhenAll(entryTask, metadataTask);

                        Metadata = await metadataTask;

                        var savedPackage = NuGetService.GetFavouritePackages().FirstOrDefault(i => i.PackageId == latest.Id);
                        if (savedPackage != null)
                        {
                            savedPackage.TotalDownloads = Metadata.TotalDownloads;
                            NuGetService.UpsertFavouritePackage(savedPackage);
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

                Clear();
            }
            finally
            {
                IsBusy = false;
            }

            HasError = Entry == null;
        }

        private async Task EntryTappedAsync(CatalogEntry entry, CancellationToken cancellationToken)
        {
            if (IsBusy || entry == null)
                return;

            IsBusy = true;

            try
            {
                var entryData = await NuGetService.GetCatalogDataAsync(entry.IndexUrl, cancellationToken);
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

        private async Task LinkTappedAsync(LinkType type)
        {
            if (Entry != null && EntryData != null)
            {
                switch (type)
                {
                    case LinkType.NuGet:
                    case LinkType.FuGet:
                        var source = await NuGetService.GetNuGetSourceAsync(default);
                        if (!string.IsNullOrEmpty(source?.PackageDetailsUriTemplate))
                        {
                            var detailsUrl = source.PackageDetailsUriTemplate
                                .Replace("{id}", Entry.Id)
                                .Replace("{version}", Entry.PackVersion.ToString());
                            detailsUrl = type == LinkType.FuGet
                                ? detailsUrl.Replace("nuget.org", "fuget.org")
                                : detailsUrl;

                            await _browser.OpenAsync(detailsUrl);
                        }
                        break;
                    case LinkType.Project:
                        if (!string.IsNullOrEmpty(Entry.ProjectUrl))
                        {
                            await _browser.OpenAsync(Entry.ProjectUrl);
                        }
                        break;
                    case LinkType.Repository:
                        //await Browser.OpenAsync(EntryData.Repository);
                        break;
                    case LinkType.License:
                        if (!string.IsNullOrEmpty(Entry.LicenseUrl))
                        {
                            await _browser.OpenAsync(Entry.LicenseUrl);
                        }
                        break;
                    default:
                        break;
                }
            }
            else if (!string.IsNullOrEmpty(PackageId) &&
                     !string.IsNullOrEmpty(Version) &&
                     type == LinkType.NuGet)
            {
                var source = await NuGetService.GetNuGetSourceAsync(default);
                if (!string.IsNullOrEmpty(source?.PackageDetailsUriTemplate))
                {
                    var detailsUrl = source.PackageDetailsUriTemplate
                        .Replace("{id}", PackageId)
                        .Replace("{version}", Version);
                    await _browser.OpenAsync(detailsUrl);
                }
            }
        }

        private async Task FavouriteAsync(CatalogEntry entry)
        {
            if (entry != null)
            {
                if (entry.IsFavourite)
                {
                    var favourite = NuGetService.GetFavouritePackages();
                    var sp = favourite.FirstOrDefault(f => f.PackageId == entry.Id);
                    if (sp != null)
                    {
                        NuGetService.RemoveFavouritePackage(sp);
                    }
                }
                else
                {
                    var includePrerelease = NuGetService.IncludePrerelease;
                    var lastestEntry = CatalogEntries.FirstOrDefault(e => includePrerelease || !e.PackVersion.IsPrerelease);
                    var source = await NuGetService.GetNuGetSourceAsync(default);
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

                    NuGetService.UpsertFavouritePackage(sp);
                }

                var isFav = !entry.IsFavourite;
                foreach (var e in CatalogEntries)
                {
                    e.IsFavourite = isFav;
                }
            }
        }

        private void Clear()
        {
            Metadata = null;
            Entry = null;
            EntryData = null;
            CatalogEntries.Clear();
            Dependencies.Clear();
        }
    }
}