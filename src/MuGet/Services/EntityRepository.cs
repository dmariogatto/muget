using LiteDB;
using MuGet.Models;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace MuGet.Services
{
    public class EntityRepository<T> : IEntityRepository<T> where T : IEntity
    {
        private readonly ILiteCollection<T> _collection;
        private readonly TimeSpan _lifeSpan;

        private readonly IConnectivity _connectivity;

        private readonly RetryPolicy _retryPolicy =
               Policy.Handle<LiteException>()
                     .WaitAndRetry
                       (
                           retryCount: 3,
                           sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt))
                       );

        public EntityRepository(
            LiteDatabase db,
            TimeSpan entityLifeSpan,
            IConnectivity connectivity)
        {
            if (db == null) throw new ArgumentException(nameof(db));

            _lifeSpan = entityLifeSpan;
            _collection = db.GetCollection<T>();

            _connectivity = connectivity;
        }

        public bool Upsert(T entity, DateTime? timestamp = null)
        {
            entity.Timestamp = timestamp ?? DateTime.UtcNow;
            return _retryPolicy.Execute(() => _collection.Upsert(entity));
        }

        public T FindById(string key, bool includeStale = false)
        {
            var document = _retryPolicy.Execute(() => _collection.FindById(key));

            if (document != null &&
                !includeStale &&
                _connectivity.NetworkAccess == NetworkAccess.Internet &&
                document.IsStale(_lifeSpan))
            {
                document = default;
            }

            return document;
        }

        public IList<T> GetAll()
        {
            return _retryPolicy.Execute(() => _collection.FindAll().ToList());
        }

        public IList<T> FindAll(bool includeStale = false)
        {
            return _retryPolicy.Execute(() =>
                _collection.FindAll().Where(e => includeStale || !e.IsStale(_lifeSpan)).ToList());
        }

        public IList<T> Find(Expression<Func<T, bool>> predicate, bool includeStale = false)
        {
            return _retryPolicy.Execute(() =>
                _collection.Find(predicate).Where(e => includeStale || !e.IsStale(_lifeSpan)).ToList());
        }

        public IList<string> FindAllKeys(bool includeStale = false)
        {
            return _retryPolicy.Execute(() => FindAll(includeStale).Select(d => d.Key).ToList());
        }

        public IList<string> GetAllKeys()
        {
            return _retryPolicy.Execute(() => _collection.FindAll().Select(d => d.Key).ToList());
        }

        public int EmptyStale()
        {
            var compareDate = DateTime.UtcNow.Subtract(_lifeSpan);
            return _retryPolicy.Execute(() => _collection.DeleteMany((e) => e.Timestamp < compareDate));
        }

        public int EmptyAll()
        {
            return _retryPolicy.Execute(() => _collection.DeleteMany(_ => true));
        }

        public bool Delete(string key)
        {
            return _retryPolicy.Execute(() => _collection.Delete(key));
        }

        public static (bool success, int count) EnsurePropertyDataTypes(LiteDatabase db)
        {
            var success = true;
            var count = 0;

            try
            {
                var dataType = typeof(T);
                var collectionName = dataType.Name;

                if (db?.CollectionExists(collectionName) == true)
                {
                    var properties = dataType.GetProperties()
                        .Where(p => p.CanRead &&
                                    p.CanWrite &&
                                    !p.GetCustomAttributes(false)
                                      .Any(i => i is BsonIdAttribute || i is BsonIgnoreAttribute))
                        .ToList();

                    var collection = db.GetCollection(collectionName);
                    var items = collection.FindAll();

                    foreach (var i in items)
                    {
                        var bDoc = i.AsDocument;
                        var hasChanged = false;

                        foreach (var p in properties.Where(p => bDoc.ContainsKey(p.Name)))
                        {
                            var bProp = bDoc[p.Name];
                            var rawVal = bProp.RawValue;

                            if (rawVal?.GetType() is Type rawType &&
                                rawVal.GetType() != p.PropertyType &&
                                (rawType.IsNumeric() || rawType.IsString()) &&
                                (p.PropertyType.IsNumeric() || p.PropertyType.IsString()))
                            {
                                var type = p.PropertyType.IsGenericType &&
                                           p.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                                           ? Nullable.GetUnderlyingType(p.PropertyType)
                                           : p.PropertyType;
                                var val = Convert.ChangeType(rawVal, type);
                                bDoc[p.Name] = new BsonValue(val);

                                hasChanged = true;
                            }
                        }

                        if (hasChanged)
                        {
                            collection.Update(i);
                            count++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                success = false;
            }

            return (success, count);
        }
    }
}