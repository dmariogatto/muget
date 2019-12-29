using LiteDB;
using MuGet.Forms.Exceptions;
using MuGet.Forms.Models;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace MuGet.Forms.Services
{
    public class NuGetService : INuGetService
    {
        private const string NuGet = "https://api.nuget.org/v3/index.json";

        private readonly static string _dbPath = Path.Combine(FileSystem.AppDataDirectory, "nugets.db");

        private readonly static HttpClient _httpClient = new HttpClient(new HttpClientHandler()
        {
            // Required for Android
            // https://github.com/xamarin/xamarin-android/issues/2619
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        });

        private readonly static LiteDatabase _db = new LiteDatabase(_dbPath);

        private readonly ICacheProvider _cache;
        private readonly TimeSpan _defaultCacheExpires = TimeSpan.FromMinutes(10);

        private readonly IEntityRepository<PackageSource> _packageSourceRepo;
        private readonly IEntityRepository<FavouritePackage> _favouriteRepo;
        private readonly IEntityRepository<RecentPackage> _recentRepo;

        private readonly ILogger _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        
        public NuGetService(ICacheProvider cacheProvider, ILogger logger)
        {
            if (cacheProvider == null) throw new ArgumentNullException(nameof(cacheProvider));
            if (logger == null) throw new ArgumentNullException(nameof(logger));

            _cache = cacheProvider;
            _logger = logger;
            
            _packageSourceRepo = new EntityRepository<PackageSource>(_db, TimeSpan.FromDays(7));
            _favouriteRepo = new EntityRepository<FavouritePackage>(_db, TimeSpan.MaxValue);
            _recentRepo = new EntityRepository<RecentPackage>(_db, TimeSpan.MaxValue);            

            _retryPolicy =
               Policy.Handle<WebException>()
                     .Or<HttpRequestException>()
                     .Or<TaskCanceledException>()
                     .WaitAndRetryAsync
                       (
                           retryCount: 2,
                           sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                       );
        }

        public bool IncludePrerelease 
        {
            get => Preferences.Get(nameof(IncludePrerelease), false);
            set => Preferences.Set(nameof(IncludePrerelease), value);
        }

        public bool NewReleaseNotifications
        {
            get => Preferences.Get(nameof(NewReleaseNotifications), true);
            set => Preferences.Set(nameof(NewReleaseNotifications), value);
        }

        public async Task<PackageSource> GetNuGetSource(CancellationToken cancellationToken)
        {
            var packageSource = default(PackageSource);

            try
            {
                packageSource = _packageSourceRepo.FindById(NuGet);

                // Refresh NuGet source on updates
                if (packageSource == null || VersionTracking.IsFirstLaunchForCurrentBuild)
                {
                    var nuget = await GetWithRetry<NuGetSource>(NuGet, cancellationToken).ConfigureAwait(false);
                    packageSource = new PackageSource(nameof(NuGet), NuGet, nuget);
                    _packageSourceRepo.Upsert(packageSource);
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    _logger.Error(ex);
            }

            return packageSource;
        }

        public async Task<(int, IList<PackageMetadata>)> Search(string query, int skip, int take, CancellationToken cancellationToken, bool? includePrerelease = null)
        {
            var result = default(SearchResult);

            try
            {
                var source = await GetNuGetSource(cancellationToken).ConfigureAwait(false);
                var url = $"{source.SearchQueryService}?q={WebUtility.UrlEncode(query)}&prerelease={includePrerelease ?? IncludePrerelease}&skip={skip}&take={take}&semVerLevel=2";
                result = await GetWithRetry<SearchResult>(url, cancellationToken).ConfigureAwait(false);

                var tasks = new List<Task>();
                foreach (var i in result.Data)
                {
                    if (!string.IsNullOrWhiteSpace(i.IconUrl))
                    {
                        // FFImageLoading does not cache 404's, which results in
                        // exceptions when scrolling the packages... this kills performance
                        // so validate the url before loading.
                        tasks.Add(IsValidUrl(i.IconUrl, cancellationToken).ContinueWith((r) =>
                        {
                            if (r.IsFaulted || !r.Result)
                            {
                                System.Diagnostics.Debug.WriteLine($"Invalid Url: '{i.IconUrl}'");
                                i.IsIconUrlValid = false;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Valid Url: '{i.IconUrl}'");
                                i.IsIconUrlValid = true;
                            }
                        }));
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    _logger.Error(ex);
            }

            return (result?.TotalHits ?? 0, result?.Data ?? new List<PackageMetadata>(0));
        }

        public async Task<PackageMetadata> GetPackageMetadata(string id, CancellationToken cancellationToken, bool? includePrerelease = null)
        {
            var cacheKey = $"PackageId:{id};IncludePrerelease:{includePrerelease ?? IncludePrerelease}";
            var metadata = _cache.Get<PackageMetadata>(cacheKey);

            if (metadata == null)
            {
                var result = await Search($"PackageId:\"{id}\"", 0, 1, cancellationToken, includePrerelease).ConfigureAwait(false);
                metadata = result.Item2?.FirstOrDefault();

                if (metadata != null)
                {
                    _cache.Set(cacheKey, metadata, _defaultCacheExpires);
                }
            }

            return metadata;
        }

        public async Task<IList<CatalogEntry>> GetCatalogEntries(string packageId, CancellationToken cancellationToken)
        {
            var cacheKey = $"CatalogEntries:{packageId}";
            var result = _cache.Get<IList<CatalogEntry>>(cacheKey);

            try
            {
                if (result == null)
                {
                    var source = await GetNuGetSource(cancellationToken).ConfigureAwait(false);
                    var catalogRoot = await GetWithRetry<CatalogRoot>(source.GetRegistrationUrl(packageId), cancellationToken).ConfigureAwait(false);                    
                    if (catalogRoot?.Items?.Any() == true)
                    {
                        async Task<IList<CatalogEntry>> getCatalogEntrys(CatalogPage page, CancellationToken ct)
                        {
                            var items = page?.Items;

                            if (items == null)
                            {
                                if (!string.IsNullOrEmpty(page?.Id))
                                {
                                    var pageResult = await GetWithRetry<CatalogPage>(page.Id, ct).ConfigureAwait(false);
                                    items = pageResult?.Items;
                                }
                            }

                            return items?.Select(i => i.CatalogEntry)?.ToList() ?? new List<CatalogEntry>(0);
                        }

                        var catalogTasks = catalogRoot.Items.Select(i => getCatalogEntrys(i, cancellationToken)).ToList();
                        await Task.WhenAll(catalogTasks).ConfigureAwait(false);
                        result = catalogTasks.SelectMany(t => t.Result)
                                             .Where(i => i.Listed)
                                             .OrderByDescending(i => i.PackVersion)
                                             .ToList();
                    }

                    if (result?.Any() == true)
                    {
                        _cache.Set(cacheKey, result, _defaultCacheExpires);
                    }
                }                
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    _logger.Error(ex);
            }

            return result ?? new List<CatalogEntry>();
        }

        public async Task<CatalogData> GetCatalogData(string indexUrl, CancellationToken cancellationToken)
        {
            var cacheKey = $"CatalogData:{indexUrl}";
            var result = _cache.Get<CatalogData>(cacheKey);

            try
            {
                if (result == null)
                {
                    result = await GetWithRetry<CatalogData>(indexUrl, cancellationToken).ConfigureAwait(false);

                    if (result != null)
                    {
                        _cache.Set(cacheKey, result, _defaultCacheExpires);
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException))
                    _logger.Error(ex);
            }

            return result;
        }

        public IList<FavouritePackage> GetFavouritePackages()
        {
            return _favouriteRepo.GetAll().OrderBy(f => f.TotalDownloads).ToList();
        }

        public bool UpsertFavouritePackage(FavouritePackage package)
        {
            if (!string.IsNullOrEmpty(package?.Key))
            {
                return _favouriteRepo.Upsert(package);
            }

            return false;
        }

        public bool RemoveFavouritePackage(FavouritePackage package)
        {
            if (!string.IsNullOrEmpty(package?.Key))
            {
                return _favouriteRepo.Delete(package.Id);
            }

            return false;
        }

        public IList<RecentPackage> GetRecentPackages()
        {
            return _recentRepo.GetAll().OrderBy(p => p.SortOrder).ToList();
        }

        public bool AddRecentPackage(RecentPackage package)
        {
            if (!string.IsNullOrEmpty(package?.Key))
            {
                // Delete if already exists
                _recentRepo.Delete(package.Id);

                var recents = GetRecentPackages();
                recents.Insert(0, package);

                var toUpsert = recents.Take(10).ToList();
                var toDelete = recents.Skip(10).ToList();

                foreach (var p in toUpsert)
                {
                    // Upsert in correct order
                    p.SortOrder = toUpsert.IndexOf(p);
                    _recentRepo.Upsert(p);
                }

                foreach (var p in toDelete)
                {                    
                    // Delete overflow
                    _recentRepo.Delete(p.Id);
                }

                return true;
            }

            return false;
        }

        private Task<T> GetWithRetry<T>(string url, CancellationToken cancellationToken)
            => _retryPolicy.ExecuteAsync((ct) => Get<T>(url, ct), cancellationToken);
        
        private async Task<bool> IsValidUrl(string url, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            using (var request = new HttpRequestMessage(HttpMethod.Head, url))
            using (var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false))
            {
                return response.IsSuccessStatusCode;
            }
        }

        #region Http Methods
        private static async Task<T> Get<T>(string url, CancellationToken cancellationToken)
        {
            var result = default(T);
            var apiEx = default(ApiException);

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await _httpClient.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false))
            {
                var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    result = DeserializeJsonFromStream<T>(stream);
                }
                else
                {
                    var content = await StreamToStringAsync(stream).ConfigureAwait(false);
                    apiEx = new ApiException
                    {
                        StatusCode = (int)response.StatusCode,
                        Content = content
                    };
                }
            }

            if (apiEx != default)
                throw apiEx;

            return result;
        }

        private static T DeserializeJsonFromStream<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default(T);

            using (var sr = new StreamReader(stream))
            using (var jtr = new JsonTextReader(sr))
            {
                var js = new JsonSerializer();
                var searchResult = js.Deserialize<T>(jtr);
                return searchResult;
            }
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            string content = null;

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync().ConfigureAwait(false);

            return content;
        }
        #endregion
    }
}
