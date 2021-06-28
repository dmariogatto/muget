using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MuGet.Models
{
    public class PackageEntry
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }

        public int CompressedLength { get; set; }
        public string FullName { get; set; }
        public int Length { get; set; }
        public string Name { get; set; }
    }
}