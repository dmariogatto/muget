using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MuGet.Models
{
    public class CatalogData
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public List<string> Type { get; set; }

        public string Authors { get; set; }

        [JsonProperty("catalog:commitId")]
        public string CatalogCommitId { get; set; }
        [JsonProperty("catalog:commitTimeStamp")]
        public DateTime CatalogCommitTimeStamp { get; set; }

        public DateTime Created { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public bool IsPrerelease { get; set; }
        public DateTime LastEdited { get; set; }
        public string LicenseUrl { get; set; }
        public bool Listed { get; set; }
        public string PackageHash { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public int PackageSize { get; set; }
        public DateTime Published { get; set; }
        public string ReleaseNotes { get; set; }
        public string Repository { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Title { get; set; }
        public string VerbatimVersion { get; set; }
        public string Version { get; set; }
        public List<DependencyGroup> DependencyGroups { get; set; }
        public List<PackageEntry> PackageEntries { get; set; }
        public List<string> Tags { get; set; }
    }
}
