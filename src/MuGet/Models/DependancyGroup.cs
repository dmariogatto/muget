using Newtonsoft.Json;
using System.Collections.Generic;

namespace MuGet.Models
{
    public class DependencyGroup
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string TargetFramework { get; set; }
        public List<Dependency> Dependencies { get; set; }
    }
}