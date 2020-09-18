using Newtonsoft.Json;
using NuGet.Versioning;

namespace MuGet.Models
{
    public class Dependency
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }

        public string Id { get; set; }
        public string Range { get; set; }
        public string Registration { get; set; }
        
        private VersionRange _versionRange;
        [JsonIgnore]
        public VersionRange VersionRange
        {
            get
            {
                if (_versionRange == null && !string.IsNullOrEmpty(Range))
                {
                    VersionRange.TryParse(Range, out _versionRange);
                }

                return _versionRange;
            }
        }

        [JsonIgnore]
        public string RangePretty => VersionRange?.PrettyPrint();
    }
}
