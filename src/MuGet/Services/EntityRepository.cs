using LiteDB;
using MuGet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xamarin.Essentials;

namespace MuGet.Services
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IEntity
    {
        private readonly LiteDatabase _db;
        private readonly ILiteCollection<T> _collection;
        private readonly TimeSpan _lifeSpan;

        public EntityRepository(LiteDatabase db, TimeSpan entityLifeSpan)
        {
            if (db == null) throw new ArgumentException(nameof(db));

            _db = db;
            _lifeSpan = entityLifeSpan;

            _collection = _db.GetCollection<T>();
        }
        
        public bool Upsert(T entity, DateTime? timestamp = null)
        {
            entity.Timestamp = timestamp ?? DateTime.UtcNow;
            return _collection.Upsert(entity);
        }
        
        public T FindById(string key, bool includeStale = false)
        {
            var document = _collection.FindById(key);

            if (document != null &&
                !includeStale &&
                Connectivity.NetworkAccess == NetworkAccess.Internet &&
                document.IsStale(_lifeSpan))
            {
                document = default;
            }

            return document;
        }

        public IList<T> GetAll()
        {
            return _collection.FindAll().ToList();
        }

        public IList<T> FindAll(bool includeStale = false)
        {
            return _collection.FindAll()
                              .Where(e => includeStale || !e.IsStale(_lifeSpan)).ToList();
        }

        public IList<T> Find(Expression<Func<T, bool>> predicate, bool includeStale = false)
        {
            return _collection.Find(predicate)
                              .Where(e => includeStale || !e.IsStale(_lifeSpan)).ToList();
        }

        public IList<string> FindAllKeys(bool includeStale = false)
        {
            return FindAll(includeStale).Select(d => d.Key).ToList();
        }

        public IList<string> GetAllKeys()
        {
            return _collection.FindAll().Select(d => d.Key).ToList();
        }

        public int EmptyStale()
        {
            var compareDate = DateTime.UtcNow.Subtract(_lifeSpan);
            return _collection.DeleteMany((e) => e.Timestamp < compareDate);
        }

        public int EmptyAll()
        {
            return _collection.DeleteMany(_ => true);
        }

        public bool Delete(string key)
        {
            return _collection.Delete(key);
        }
    }
}
