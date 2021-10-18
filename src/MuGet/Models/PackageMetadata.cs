using MvvmHelpers;
using Newtonsoft.Json;
using MuGet.Localisation;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MuGet.Models
{
    public class PackageMetadata : ObservableObject
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Registration { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public string Title { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }
        public string ProjectUrl { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Authors { get; set; }
        public int TotalDownloads { get; set; }
        public bool Verified { get; set; }
        public List<VersionMetadata> Versions { get; set; }

        [JsonIgnore]
        public string AuthorsDisplayText =>
            string.Join(Resources.StringJoinComma, Authors ?? Enumerable.Empty<string>());
        [JsonIgnore]
        public string TagsDisplayText =>
            string.Join(Resources.StringJoinComma, Tags ?? Enumerable.Empty<string>());

        private bool _isIconUrlValid;
        [JsonIgnore]
        public bool IsIconUrlValid
        {
            get => _isIconUrlValid;
            set
            {
                if (SetProperty(ref _isIconUrlValid, value))
                    OnPropertyChanged(nameof(ValidatedIconUrl));
            }
        }

        [JsonIgnore]
        public string ValidatedIconUrl => IsIconUrlValid
            ? IconUrl
            : string.Empty;

        [JsonIgnore]
        public string SearchDescription => !string.IsNullOrEmpty(Summary)
            ? Summary
            : Description;

        private PackageVersion _packVersion;
        [JsonIgnore]
        public PackageVersion PackVersion => _packVersion ?? (_packVersion = new PackageVersion(Version));

        public override bool Equals(object obj)
        {
            return obj is PackageMetadata metadata &&
                   Id == metadata.Id &&
                   Version == metadata.Version;
        }

        public override int GetHashCode()
            => HashCode.Combine(Id, Version);
    }
}
