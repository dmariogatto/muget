using MvvmHelpers;
using Newtonsoft.Json;
using MuGet.Forms.Localisation;
using System.Collections.Generic;
using System.Linq;

namespace MuGet.Forms.Models
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

        private string _iconUrl;
        public string IconUrl
        {
            get => !string.IsNullOrEmpty(_iconUrl) ? _iconUrl : (string)Xamarin.Forms.Application.Current.Resources["PackageIcon"];
            set => _iconUrl = value;
        }

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
                SetProperty(ref _isIconUrlValid, value);
                OnPropertyChanged(nameof(ValidatedIconUrl));
            }
        }

        [JsonIgnore]
        public string ValidatedIconUrl => IsIconUrlValid
            ? IconUrl
            : (string)Xamarin.Forms.Application.Current.Resources["PackageIcon"];

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
        {
            var hashCode = -1416534245;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + Version.GetHashCode();
            return hashCode;
        }
    }
}
