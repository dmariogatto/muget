using MvvmHelpers;
using Newtonsoft.Json;
using System;

namespace MuGet.Models
{
    public interface IEntity
    {
        [JsonIgnore, LiteDB.BsonIgnore]
        string Key { get; }
        [JsonIgnore]
        DateTime Timestamp { get; set; }
        bool IsStale(TimeSpan lifeSpan);
    }

    public abstract class Entity : ObservableObject, IEntity
    {
        public abstract string Key { get; }
        public abstract DateTime Timestamp { get; set; }

        public bool IsStale(TimeSpan lifeSpan) => Timestamp < DateTime.UtcNow.Subtract(lifeSpan);
    }
}