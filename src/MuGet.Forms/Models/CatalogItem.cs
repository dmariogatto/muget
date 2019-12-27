using Newtonsoft.Json;

namespace MuGet.Forms.Models
{
    public class CatalogItem
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        public CatalogEntry CatalogEntry { get; set; }
    }
}
