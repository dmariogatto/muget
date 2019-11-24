using MuGet.Forms.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MuGet.Forms.Services
{
    public interface IEntityRepository<T> where T : IEntity
    {
        bool Upsert(T entity, DateTime? timestamp = null);
        T FindById(string key, bool includeStale = false);
        IList<T> GetAll();
        IList<T> FindAll(bool includeStale = false);
        IList<T> Find(Expression<Func<T, bool>> predicate, bool includeStale = false);
        IList<string> FindAllKeys(bool includeStale = false);
        IList<string> GetAllKeys();
        int EmptyStale();
        int EmptyAll();

        bool Delete(string key);
    }
}
