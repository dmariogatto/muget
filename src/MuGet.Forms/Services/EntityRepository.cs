using LiteDB;
using MuGet.Forms.Models;
using Plugin.Connectivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MuGet.Forms.Services
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IEntity
    {
        private readonly LiteDatabase _db;
        private readonly LiteCollection<T> _collection;
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
                CrossConnectivity.Current.IsConnected &&
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
            return _collection.Delete((e) => e.IsStale(_lifeSpan));
        }

        public int EmptyAll()
        {
            return _collection.Delete(_ => true);
        }

        public bool Delete(string key)
        {
            return _collection.Delete(key);
        }
    }
}
