using System;

namespace MuGet.Models
{
    public class SavedPackage : Entity
    {
        [LiteDB.BsonId]
        public string Id { get => $"{PackageId}_{SourceUrl}"; set { } }
        public string PackageId { get; set; }
        public string IndexUrl { get; set; }
        public string SourceUrl { get; set; }

        public int TotalDownloads { get; set; }

        public string Version { get; set; }
        public DateTime Published { get; set; }

        public int SortOrder { get; set; }

        #region IEntity
        public override string Key { get => Id; }
        public override DateTime Timestamp { get; set; }
        #endregion
    }

    public class FavouritePackage : SavedPackage { }
    public class RecentPackage : SavedPackage { }
}
