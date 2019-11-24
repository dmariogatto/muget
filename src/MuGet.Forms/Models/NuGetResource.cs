using Newtonsoft.Json;

namespace MuGet.Forms.Models
{
    public class NuGetResource
    {
        [JsonProperty("@id")]
        public string Id { get; set; }
        [JsonProperty("@type")]
        public string Type { get; set; }
        public string Comment { get; set; }
    }
}
