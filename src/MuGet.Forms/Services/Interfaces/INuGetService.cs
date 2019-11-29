using MuGet.Forms.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MuGet.Forms.Services
{
    public interface INuGetService
    {
        bool IncludePrerelease { get; set; }
        bool NewReleaseNotifications { get; set; }

        Task<PackageSource> GetNuGetSource(CancellationToken cancellationToken);

        Task<(int, IList<PackageMetadata>)> Search(string query, int skip, int take, CancellationToken cancellationToken, bool? includePrerelease = null);
        Task<PackageMetadata> GetPackageMetadata(string id, CancellationToken cancellationToken, bool? includePrerelease = null);

        Task<IList<CatalogEntry>> GetCatalogEntries(string packageId, CancellationToken cancellationToken);
        Task<CatalogData> GetCatalogData(string indexUrl, CancellationToken cancellationToken);

        IList<FavouritePackage> GetFavouritePackages();
        bool UpsertFavouritePackage(FavouritePackage package);
        bool RemoveFavouritePackage(FavouritePackage package);

        IList<RecentPackage> GetRecentPackages();
        bool AddRecentPackage(RecentPackage package);
    }
}
