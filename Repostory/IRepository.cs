using Core;
using DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Repository
{
    public interface IRepository<T> where T : BaseEntity
    {
        IQueryable<T> Table { get; }
        IQueryable<T> TableNoTracking { get; }
        T GetById(object id);
        void Insert(T entity);
        void Insert(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
        void UpdateMatchEntity(T updateEntity, T setEntity);
        void Delete(T entity);
        void Delete(IEnumerable<T> entities);
        IQueryable<T> IncludeMany(params Expression<Func<T, object>>[] includes);
        IEnumerable<T> GetSql(string sql);
    }
}
