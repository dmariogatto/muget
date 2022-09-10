using Newtonsoft.Json;

namespace MuGet.Models
{
    public class VersionMetadata
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        public string Version { get; set; }
        public long Downloads { get; set; }
    }
}