using Newtonsoft.Json;

namespace MuGet.Forms.Models
{
    public class VersionMetadata
    {
        [JsonProperty("@id")]
        public string IndexUrl { get; set; }
        public string Version { get; set; }
        public int Downloads { get; set; }
    }
}
