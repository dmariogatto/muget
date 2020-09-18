using Newtonsoft.Json;
using System.Collections.Generic;

namespace MuGet.Models
{
    public class CatalogRoot
    {
        [JsonProperty("@id")]
        public string Id { get; set; }

        public List<CatalogPage> Items { get; set; }
    }
}
