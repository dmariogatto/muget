using LiteDB;
using MuGet.Exceptions;
using MuGet.Models;
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

namespace MuGet.Services
{
    public class NuGetService : INuGetService
    {
        private const string NuGet = "https://api.nuget.org/v3/index.json";

        private readonly static string DbPath = Path.Combine(FileSystem.AppDataDirectory, "nugets.db");        
        private readonly static JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

        private readonly HttpClient _httpClient;
        private readonly LiteDatabase _db;

        private readonly ICacheService _cache;
        private readonly TimeSpan _defaultCacheExpires = TimeSpan.FromMinutes(10);

        private readonly IEntityRepository<PackageSource> _packageSourceRepo;
        private readonly IEntityRepository<FavouritePackage> _favouriteRepo;
        private readonly IEntityRepository<RecentPackage> _recentRepo;

        private readonly ILogger _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        
        public NuGetService(ICacheService cacheProvider, IHttpHandlerService httpHandlerService, ILogger logger)
        {
            _cache = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _httpClient = new HttpClient(httpHandlerService.GetNativeHandler());

            _db = new LiteDatabase($"Filename={DbPath};Upgrade=true;");
            _db.Pragma("UTC_DATE", true);

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

        public async Task<PackageSource> GetNuGetSourceAsync(CancellationToken cancellationToken)
        {
            var packageSource = default(PackageSource);

            try
            {
                packageSource = _packageSourceRepo.FindById(NuGet);

                // Refresh NuGet source on updates
                if (packageSource == null || VersionTracking.IsFirstLaunchForCurrentBuild)
                {
                    var nuget = await GetWithRetryAsync<NuGetSource>(NuGet, cancellationToken).ConfigureAwait(false);
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

        public async Task<(int, IList<PackageMetadata>)> SearchAsync(string query, int skip, int take, CancellationToken cancellationToken, bool? includePrerelease = null)
        {
            var result = default(SearchResult);

            try
            {
                var source = await GetNuGetSourceAsync(cancellationToken).ConfigureAwait(false);
                var url = $"{source.SearchQueryService}?q={WebUtility.UrlEncode(query)}&prerelease={includePrerelease ?? IncludePrerelease}&skip={skip}&take={take}&semVerLevel=2";
                result = await GetWithRetryAsync<SearchResult>(url, cancellationToken).ConfigureAwait(false);

                foreach (var i in result.Data)
                {
                    if (!string.IsNullOrWhiteSpace(i.IconUrl))
                    {
                        // FFImageLoading does not cache 404's, which results in
                        // exceptions when scrolling the packages... this kills performance
                        // so validate the url before loading.
                        _ = IsValidUrlAsync(i.IconUrl, cancellationToken).ContinueWith((r) =>
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
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return (result?.TotalHits ?? 0, result?.Data ?? new List<PackageMetadata>(0));
        }

        public async Task<PackageMetadata> GetPackageMetadataAsync(string id, CancellationToken cancellationToken, bool? includePrerelease = null)
        {
            var cacheKey = $"PackageId:{id};IncludePrerelease:{includePrerelease ?? IncludePrerelease}";
            var metadata = _cache.Get<PackageMetadata>(cacheKey);

            if (metadata == null)
            {
                var result = await SearchAsync($"PackageId:\"{id}\"", 0, 1, cancellationToken, includePrerelease).ConfigureAwait(false);
                metadata = result.Item2?.FirstOrDefault();

                if (metadata != null)
                {
                    _cache.Set(cacheKey, metadata, _defaultCacheExpires);
                }
                else
                {
                    _logger.Event(
                        AppCenterEvents.Error.PackageLoadFailed,
                        new Dictionary<string, string> { { AppCenterEvents.Property.Key, cacheKey } });
                }
            }

            return metadata;
        }

        public async Task<IList<CatalogEntry>> GetCatalogEntriesAsync(string packageId, CancellationToken cancellationToken)
        {
            var cacheKey = $"CatalogEntries:{packageId}";
            var result = _cache.Get<IList<CatalogEntry>>(cacheKey);

            try
            {
                if (result == null)
                {
                    var source = await GetNuGetSourceAsync(cancellationToken).ConfigureAwait(false);
                    var catalogRoot = await GetWithRetryAsync<CatalogRoot>(source.GetRegistrationUrl(packageId), cancellationToken).ConfigureAwait(false);                    
                    if (catalogRoot?.Items?.Any() == true)
                    {
                        async Task<IList<CatalogEntry>> getCatalogEntries(CatalogPage page, CancellationToken ct)
                        {
                            var items = page?.Items;

                            if (items == null)
                            {
                                if (!string.IsNullOrEmpty(page?.Id))
                                {
                                    var pageResult = await GetWithRetryAsync<CatalogPage>(page.Id, ct).ConfigureAwait(false);
                                    items = pageResult?.Items;
                                }
                            }

                            return items?.Select(i => i.CatalogEntry)?.ToList() ?? new List<CatalogEntry>(0);
                        }

                        var catalogTasks = catalogRoot.Items.Select(i => getCatalogEntries(i, cancellationToken)).ToList();
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
                _logger.Error(ex);
            }

            return result ?? new List<CatalogEntry>();
        }

        public async Task<CatalogData> GetCatalogDataAsync(string indexUrl, CancellationToken cancellationToken)
        {
            var cacheKey = $"CatalogData:{indexUrl}";
            var result = _cache.Get<CatalogData>(cacheKey);

            try
            {
                if (result == null)
                {
                    result = await GetWithRetryAsync<CatalogData>(indexUrl, cancellationToken).ConfigureAwait(false);

                    if (result != null)
                    {
                        _cache.Set(cacheKey, result, _defaultCacheExpires);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return result;
        }

        public IList<FavouritePackage> GetFavouritePackages()
        {
            return _favouriteRepo.GetAll().OrderByDescending(f => f.Published).ToList();
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

        public bool Checkpoint()
        {
            var success = false;

            try
            {
                _db.Checkpoint();
                success = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return success;
        }

        private Task<T> GetWithRetryAsync<T>(string url, CancellationToken cancellationToken)
            => _retryPolicy.ExecuteAsync((ct) => GetAsync<T>(url, ct), cancellationToken);
        
        private async Task<bool> IsValidUrlAsync(string url, CancellationToken cancellationToken)
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
        private async Task<T> GetAsync<T>(string url, CancellationToken cancellationToken)
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
                    result = DeserializeJsonFromStreamAsync<T>(stream);
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

        private static T DeserializeJsonFromStreamAsync<T>(Stream stream)
        {
            if (stream == null || stream.CanRead == false)
                return default;

            using var sr = new StreamReader(stream);
            using var jtr = new JsonTextReader(sr);
            var searchResult = JsonSerializer.Deserialize<T>(jtr);
            return searchResult;
        }

        private static async Task<string> StreamToStringAsync(Stream stream)
        {
            var content = default(string);

            if (stream != null)
                using (var sr = new StreamReader(stream))
                    content = await sr.ReadToEndAsync().ConfigureAwait(false);

            return content;
        }
        #endregion
    }
}
