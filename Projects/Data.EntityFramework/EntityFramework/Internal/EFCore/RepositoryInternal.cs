using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OnUtils.Data.EntityFramework.Internal
{
    using Data;

    abstract class RepositoryInternalBase : IQueryProvider
    {
        protected DataContextInternal _context = null;
        private static MethodInfo createQueryInternalMethod = null;

        static RepositoryInternalBase()
        {
            createQueryInternalMethod = typeof(RepositoryInternalBase).GetMethod(nameof(CreateQueryInternal), BindingFlags.NonPublic | BindingFlags.Instance);
            if (createQueryInternalMethod == null) throw new TypeInitializationException(typeof(RepositoryInternalBase).FullName, new Exception($"Не удается получить метод {nameof(CreateQueryInternal)}."));
        }

        /// <summary>
        /// </summary>
        /// <param name="context">См. описание <see cref="Context"/></param>
        public RepositoryInternalBase(DataContextInternal context = null)
        {
            if (context == null) throw new ArgumentNullException(nameof(context), "Указан пустой контекст.");
            _context = context;
        }

        #region Свойства
        /// <summary>
        /// Возвращает контекст доступа к БД.
        /// Передается в конструкторе. Если не задан в конструкторе - на момент запроса создается новый.
        /// Чтобы избежать кросс-контекстного запроса (и ошибки), надо делать UnitOfWork
        /// </summary>
        protected DataContextInternal Context
        {
            get => _context;
        }
        #endregion

        #region IQueryProvider
        protected abstract IQueryProvider GetProvider();

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
            //var query = GetProvider().CreateQuery(expression);
            //return new QueryInternal((DbQuery)query, this as IRepository);
        }

        IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
        {
            return (IQueryable<TElement>)createQueryInternalMethod.MakeGenericMethod(typeof(TElement)).Invoke(this, new object[] { expression });
        }

        private IQueryable<TElement> CreateQueryInternal<TElement>(Expression expression) where TElement : class
        {
            var query = GetProvider().CreateQuery<TElement>(expression);
            var queryT = query.GetType();

            if (query is QueryInternal<TElement>)
            {
                return query;
            }
            else if (query is DbQuery<TElement> query1)
            {
                return new QueryInternal<TElement>(query1, this as IRepository);
            }
            else if (query is Microsoft.EntityFrameworkCore.Query.Internal.EntityQueryable<TElement> query2)
            {
                return new QueryInternal<TElement>(query2, this as IRepository);
            }
            else
            {
                var g = query.GetType();
                throw new Exception($"Неизвестный тип запроса '{g.FullName}'.");
            }
        }

        object IQueryProvider.Execute(Expression expression)
        {
            return GetProvider().Execute(expression);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return GetProvider().Execute<TResult>(expression);
        }

        #endregion

        internal abstract IQueryable GetQuery();
    }

    class RepositoryInternal<TEntity> : RepositoryInternalBase, IRepository<TEntity> where TEntity : class
    {
        private DbSet<TEntity> _dbSet = null;

        public RepositoryInternal(DataContextInternal context = null) : base(context)
        {
        }

        #region Свойства
        /// <summary>
        /// См. <see cref="IRepository.DataContext"/>.
        /// </summary>
        public IDataContext DataContext
        {
            get => Context as DataContextInternal;
        }

        /// <summary>
        /// </summary>
        internal DbSet<TEntity> DbSet
        {
            get
            {
                if (_dbSet == null) _dbSet = Context.Set<TEntity>();
                return _dbSet;
            }
        }
        #endregion

        #region IRepository<TEntity>
        /// <summary>
        /// См. <see cref="IRepository{TEntity}.Add(TEntity[])"/>.
        /// </summary>
        public void Add(params TEntity[] items)
        {
            if (IsReadonly) throw new Exception("Репозиторий работает в режиме 'только чтение', добавление объектов невозможно.");

            DbSet.AddRange(items);
        }

        private TEntity AddOrUpdateInternal(TEntity entity)
        {
            var entityEntry = Context.Entry(entity);

            var primaryKeyName = entityEntry.Context.Model.FindEntityType(typeof(TEntity)).FindPrimaryKey().Properties
                .Select(x => x.Name).Single();

            var primaryKeyField = entity.GetType().GetProperty(primaryKeyName);

            var t = typeof(TEntity);
            if (primaryKeyField == null)
            {
                throw new Exception($"{t.FullName} does not have a primary key specified. Unable to exec AddOrUpdate call.");
            }
            var keyVal = primaryKeyField.GetValue(entity);
            var dbVal = DbSet.Find(keyVal);

            if (dbVal != null)
            {
                Context.Entry(dbVal).CurrentValues.SetValues(entity);
                DbSet.Update(dbVal);

                entity = dbVal;
            }
            else
            {
                DbSet.Add(entity);
            }

            return entity;
        }

        private TEntity AddOrUpdateInternal(Expression<Func<TEntity, object>> identifierExpression, TEntity entity)
        {
            if (identifierExpression == null)
                throw new ArgumentNullException(nameof(identifierExpression));
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var keyObject = identifierExpression.Compile()(entity);
            var parameter = Expression.Parameter(typeof(TEntity), "p");

            var lambda = Expression.Lambda<Func<TEntity, bool>>(
                Expression.Equal(
                    ReplaceParameter(identifierExpression.Body, parameter),
                    Expression.Constant(keyObject)),
                parameter);

            var dbVal = DbSet.FirstOrDefault(lambda.Compile());

            if (dbVal != null)
            {
                Context.Entry(dbVal).CurrentValues.SetValues(entity);
                DbSet.Update(dbVal);

                entity = dbVal;
            }
            else
            {
                DbSet.Add(entity);
            }

            return entity;
        }

        private static Expression ReplaceParameter(Expression oldExpression, ParameterExpression newParameter)
        {
            switch (oldExpression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var m = (MemberExpression)oldExpression;
                    return Expression.MakeMemberAccess(newParameter, m.Member);
                case ExpressionType.New:
                    var newExpression = (NewExpression)oldExpression;
                    var arguments = new List<Expression>();
                    foreach (var a in newExpression.Arguments)
                        arguments.Add(ReplaceParameter(a, newParameter));
                    var returnValue = Expression.New(newExpression.Constructor, arguments.ToArray());
                    return returnValue;
                default:
                    throw new NotSupportedException("Unknown expression type for AddOrUpdate: " + oldExpression.NodeType);
            }
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.AddOrUpdate(TEntity[])"/>.
        /// </summary>
        public void AddOrUpdate(params TEntity[] items)
        {
            items.ForEach(x => AddOrUpdateInternal(x));
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.AddOrUpdate(Expression{Func{TEntity, object}}, TEntity[])"/>.
        /// </summary>
        public void AddOrUpdate(Expression<Func<TEntity, object>> identifierExpression, params TEntity[] items)
        {
            items.ForEach(x => AddOrUpdateInternal(identifierExpression, x));
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.InsertOrDuplicateUpdate(IEnumerable{TEntity}, UpsertField[])"/>.
        /// </summary>
        public int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, params UpsertField[] updateFields)
        {
            object lastIdentity = null;
            return _context.InsertOrDuplicateUpdate<TEntity>(objectsIntoQuery, out lastIdentity, updateFields);
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.InsertOrDuplicateUpdate(IEnumerable{TEntity}, out object, UpsertField[])"/>.
        /// </summary>
        public int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, out object lastIdentity, params UpsertField[] updateFields)
        {
            return _context.InsertOrDuplicateUpdate<TEntity>(objectsIntoQuery, out lastIdentity, updateFields);
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.InsertOrDuplicateUpdate(string, UpsertField[])"/>.
        /// </summary>
        public int InsertOrDuplicateUpdate(string insertQuery, params UpsertField[] updateFields)
        {
            return _context.InsertOrDuplicateUpdate<TEntity>(insertQuery, updateFields);
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.Delete(TEntity)"/>.
        /// </summary>
        public void Delete(TEntity item)
        {
            if (IsReadonly) throw new Exception("Репозиторий работает в режиме 'только чтение', удаление объектов невозможно.");

            var entry = _context.Entry(item);

            //Если объект не прикреплен к контексту (получен через AsNoTracking запрос), то пытаемся прикрепить. 
            if (entry.State == EntityState.Detached) _context.Set<TEntity>().Attach(item);

            //Сначала пытаемся удалить традиционным способом
            try { _context.Set<TEntity>().Remove(item); } catch { }

            //Если объект прикреплен к контексту, то пытаемся удалить. 
            if (entry.State != EntityState.Detached) entry.State = EntityState.Deleted;
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.HasChanges(TEntity)"/>.
        /// </summary>
        public bool HasChanges(TEntity item)
        {
            if (IsReadonly) return false;

            var entry = _context.Entry(item);
            if (entry != null)
            {
                if (entry.State != EntityState.Unchanged)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// См. <see cref="IRepository.HasChanges"/>.
        /// </summary>
        public bool HasChanges()
        {
            if (IsReadonly) return false;
            return _context.ChangeTracker.HasChanges();
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.FromCache(Expression{Func{TEntity, bool}})"/>
        /// </summary>
        public IEnumerable<TEntity> FromCache(Expression<Func<TEntity, bool>> condition)
        {
            return condition == null ? DbSet.Local : DbSet.Local.Where(condition.Compile());
        }

        /// <summary>
        /// См. <see cref="IRepository.ClearCache"/>.
        /// </summary>
        public void ClearCache()
        {
            DbSet.Local.ToList().ForEach(x =>
            {
                _context.Entry(x).State = EntityState.Detached;
                x = null; // <- this doesn't seem to be required for garbage collection
            });
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.AsBindingList"/>.
        /// </summary>
        public System.ComponentModel.BindingList<TEntity> AsBindingList()
        {
            return DbSet.Local.ToBindingList();
        }

        /// <summary>
        /// См. <see cref="IRepository{TEntity}.IsReadonly"/>.
        /// </summary>
        public bool IsReadonly
        {
            get => DataContext.IsReadonly;
        }

        string IRepository<TEntity>.GetTableName()
        {
            var table = _context.Model.FindEntityType(typeof(TEntity)).GetTableName();
            return table;
        }

        protected override IQueryProvider GetProvider()
        {
            return new DB.DbQueryProvider((DbSet as IQueryable).Provider, this);
        }

        internal override IQueryable GetQuery()
        {
            return (DbSet);
        }
        #endregion

        #region IQuery
        /// <summary>
        /// См. <see cref="IQuery.Repository"/>.
        /// </summary>
        public IRepository Repository
        {
            get => this;
        }
        #endregion

        #region IQuery<TEntity>
        /// <summary>
        /// См. <see cref="IQuery{TEntity}.AsNoTracking"/>.
        /// </summary>
        public IQuery<TEntity> AsNoTracking()
        {
            return new QueryInternal<TEntity>(DbSet.AsNoTracking(), this);
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.IsNoTracking"/>.
        /// Всегда возвращает true, т.к. репозиторий по-умолчанию является кешируемым.
        /// </summary>
        public bool IsNoTracking()
        {
            return false;
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.Include(string)"/>.
        /// </summary>
        public IQuery<TEntity> Include(string path)
        {
            return new QueryInternal<TEntity>(EntityFrameworkQueryableExtensions.Include(DbSet, path), this);
        }

        /// <summary>
        /// См. <see cref="IQuery{TEntity}.Include{TProperty}(Expression{Func{TEntity, TProperty}})"/>.
        /// </summary>
        public IQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            //Check.NotNull<IQueryable<T>>(source, "source");
            //Check.NotNull<Expression<Func<T, TProperty>>>(path, "path");
            string text;
            if (!DbHelpers.TryParsePath(path.Body, out text) || text == null)
            {
                throw new ArgumentException("Strings.DbExtensions_InvalidIncludePathExpression", nameof(path));
            }
            return new QueryInternal<TEntity>(EntityFrameworkQueryableExtensions.Include(DbSet, text), this);
        }
        #endregion

        #region IQueryable
        Type IQueryable.ElementType
        {
            get => (DbSet as IQueryable).ElementType;
        }

        Expression IQueryable.Expression
        {
            get => (DbSet as IQueryable).Expression;
        }

        IQueryProvider IQueryable.Provider
        {
            get => this as IQueryProvider;
        }

        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (DbSet as IEnumerable).GetEnumerator();
        }

        IEnumerator<TEntity> IEnumerable<TEntity>.GetEnumerator()
        {
            return (DbSet as IEnumerable<TEntity>).GetEnumerator();
        }
        #endregion
    }
}
