using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.UnitOfWork
{
    class RepositoryPropertyWrapper<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private UnitOfWorkBase _container = null;

        internal RepositoryPropertyWrapper(UnitOfWorkBase container)
        {
            if (container == null) throw new NullReferenceException("Не указан контейнер UnitOfWork.");
            _container = container;
        }

        private IRepository<TEntity> Repository
        {
            get => _container.Get<TEntity>();
        }

        public Type ElementType
        {
            get => Repository.ElementType;
        }

        public Expression Expression
        {
            get => Repository.Expression;
        }

        public IQueryProvider Provider
        {
            get => Repository.Provider;
        }

        public bool IsReadonly
        {
            get => Repository.IsReadonly;
        }

        public IDataContext DataContext
        {
            get => Repository.DataContext;
        }

        IRepository IQuery.Repository
        {
            get => Repository.Repository;
        }

        public void Add(params TEntity[] items)
        {
            Repository.Add(items);
        }

        public void AddOrUpdate(params TEntity[] items)
        {
            Repository.AddOrUpdate(items);
        }

        public void AddOrUpdate(Expression<Func<TEntity, object>> identifierExpression, params TEntity[] items)
        {
            Repository.AddOrUpdate(identifierExpression, items);
        }

        public int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, params UpsertField[] updateFields)
        {
            return Repository.InsertOrDuplicateUpdate(objectsIntoQuery, updateFields);
        }

        public int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, out object lastIdentity, params UpsertField[] updateFields)
        {
            return Repository.InsertOrDuplicateUpdate(objectsIntoQuery, out lastIdentity, updateFields);
        }

        public int InsertOrDuplicateUpdate(string insertQuery, params UpsertField[] updateFields)
        {
            return Repository.InsertOrDuplicateUpdate(insertQuery, updateFields);
        }

        public void Delete(TEntity item)
        {
            Repository.Delete(item);
        }

        public IEnumerator<TEntity> GetEnumerator()
        {
            return Repository.GetEnumerator();
        }

        public bool HasChanges()
        {
            return Repository.HasChanges();
        }

        public IEnumerable<TEntity> FromCache(Expression<Func<TEntity, bool>> condition)
        {
            return Repository.FromCache(condition);
        }

        public void ClearCache()
        {
            Repository.ClearCache();
        }

        public bool HasChanges(TEntity item)
        {
            return Repository.HasChanges(item);
        }

        public System.ComponentModel.BindingList<TEntity> AsBindingList()
        {
            return Repository.AsBindingList();
        }

        public IQueryable<TEntity> AsNoTracking()
        {
            return Repository.AsNoTracking();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Repository.GetEnumerator();
        }

        public bool IsNoTracking()
        {
            return false;
        }

        IQuery<TEntity> IQuery<TEntity>.AsNoTracking()
        {
            return Repository.AsNoTracking();
        }

        public IQuery<TEntity> Include(string path)
        {
            return Repository.Include(path);
        }

        public IQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            return Repository.Include<TProperty>(path);
        }

        public string GetTableName()
        {
            return Repository.GetTableName();
        }
    }
}
