using System;
using System.Collections.Generic;
using System.Linq;

namespace MuGet.Models
{
    public class PackageSource : Entity
    {
        public PackageSource(string name, string url, NuGetSource source)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentException(nameof(url));
            if (source?.Resources == null) throw new ArgumentNullException(nameof(source));

            Source = url;
            Name = name ?? string.Empty;

            SearchQueryService = source.Resources
                .LastOrDefault(r => r.Type.StartsWith(nameof(SearchQueryService)))?.Id ?? string.Empty;
            SearchAutocompleteService = source.Resources
                .LastOrDefault(r => r.Type.StartsWith(nameof(SearchAutocompleteService)))?.Id ?? string.Empty;
            RegistrationsBaseUrl = source.Resources
                .LastOrDefault(r => r.Type.StartsWith(nameof(RegistrationsBaseUrl)))?.Id ?? string.Empty;
            PackageVersionDisplayMetadataUriTemplate = source.Resources
                .LastOrDefault(r => r.Type.StartsWith(nameof(PackageVersionDisplayMetadataUriTemplate)))?.Id ?? string.Empty;
            PackageDetailsUriTemplate = source.Resources
                .LastOrDefault(r => r.Type.StartsWith(nameof(PackageDetailsUriTemplate)))?.Id ?? string.Empty;
        }

        public PackageSource()
        {
        }

        [LiteDB.BsonId]
        public string Source { get; set; }
        public string Name { get; set; }

        public string SearchQueryService { get; set; }
        public string SearchAutocompleteService { get; set; }
        public string RegistrationsBaseUrl { get; set; }
        public string PackageVersionDisplayMetadataUriTemplate { get; set; }
        public string PackageDetailsUriTemplate { get; set; }

        public string GetRegistrationUrl(string packageId) => !string.IsNullOrEmpty(RegistrationsBaseUrl) && !string.IsNullOrEmpty(packageId)
            ? $"{RegistrationsBaseUrl}{(RegistrationsBaseUrl.EndsWith("/") ? string.Empty : "/")}{packageId.ToLowerInvariant()}/index.json"
            : string.Empty;

        #region IEntity
        public override string Key { get => Source; }
        public override DateTime Timestamp { get; set; }
        #endregion
    }
}