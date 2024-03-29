﻿using MvvmHelpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace MuGet.Models
{
    public class CatalogEntry : ObservableObject
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Authors { get; set; }
        public string Description { get; set; }

        public string IconUrl { get; set; }

        public string Id { get; set; }
        public string Language { get; set; }
        public string LicenseExpression { get; set; }
        public string LicenseUrl { get; set; }
        public bool Listed { get; set; }
        public string MinClientVersion { get; set; }
        public string PackageContent { get; set; }
        public string ProjectUrl { get; set; }
        public DateTime Published { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Summary { get; set; }
        public List<string> Tags { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public List<DependencyGroup> DependencyGroups { get; set; }

        [JsonIgnore]
        public long Downloads { get; set; }
        private bool _isFavourite;
        [JsonIgnore]
        public bool IsFavourite
        {
            get => _isFavourite;
            set => SetProperty(ref _isFavourite, value);
        }

        [JsonIgnore]
        public DateTime PublishedLocal => Published.ToLocalTime();

        private PackageVersion _packVersion;
        [JsonIgnore]
        public PackageVersion PackVersion => _packVersion ?? (_packVersion = new PackageVersion(Version));
    }
}