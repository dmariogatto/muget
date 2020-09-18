using System.Collections.Generic;

namespace MuGet.Models
{
    public class SearchResult
    {
        public int TotalHits { get; set; }
        public List<PackageMetadata> Data { get; set; }
    }
}
