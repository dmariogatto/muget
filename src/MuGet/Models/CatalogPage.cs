using Newtonsoft.Json;
using System.Collections.Generic;

namespace MuGet.Models
{
    public class CatalogPage
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        public List<CatalogItem> Items { get; set; }
    }
}