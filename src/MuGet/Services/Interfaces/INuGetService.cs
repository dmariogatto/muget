using MuGet.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.Services
{
    public interface INuGetService
    {
        bool IncludePrerelease { get; set; }
        bool NewReleaseNotifications { get; set; }

        Task<PackageSource> GetNuGetSourceAsync(CancellationToken cancellationToken);

        Task<(int, IList<PackageMetadata>)> SearchAsync(string query, int skip, int take, CancellationToken cancellationToken, bool? includePrerelease = null);
        Task<PackageMetadata> GetPackageMetadataAsync(string id, CancellationToken cancellationToken, bool? includePrerelease = null);

        Task<IList<CatalogEntry>> GetCatalogEntriesAsync(string packageId, CancellationToken cancellationToken);
        Task<CatalogData> GetCatalogDataAsync(string indexUrl, CancellationToken cancellationToken);

        IList<FavouritePackage> GetFavouritePackages();
        bool UpsertFavouritePackage(FavouritePackage package);
        bool RemoveFavouritePackage(FavouritePackage package);

        IList<RecentPackage> GetRecentPackages();
        bool AddRecentPackage(RecentPackage package);
    }
}
