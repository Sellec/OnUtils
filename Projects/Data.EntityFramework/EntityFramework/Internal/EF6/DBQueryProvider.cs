using System;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OnUtils.Data.EntityFramework.Internal.DB
{
    // <summary>
    // A wrapping query provider that performs expression transformation and then delegates
    // to the <see cref="ObjectQuery" /> provider.  The <see cref="IQueryable" /> objects returned
    // are always instances of <see cref="DbQuery{TResult}" />. This provider is associated with
    // generic <see cref="DbQuery{T}" /> objects.
    // </summary>
    internal class DbQueryProvider : IQueryProvider
#if !NET40
, IDbAsyncQueryProvider
#endif
    {
        #region Fields and constructors
        private IQueryProvider _efQueryProvider = null;
        //private MethodInfo _method

        private readonly object _internalContext;
        private readonly object _internalQuery;

        private readonly RepositoryInternalBase _repository;

        private static Type internalQueryAdapterType = null;
        private static PropertyInfo internalQueryAdapterType_InternalQueryProperty = null;

        private static Type internalQueryType = null;
        private static PropertyInfo iInternalQueryType_ObjectQueryProviderProperty = null;
        private static PropertyInfo iInternalQueryType_ElementTypeProperty = null;

        static DbQueryProvider()
        {
            internalQueryAdapterType = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Internal.Linq.IInternalQueryAdapter");
            internalQueryAdapterType_InternalQueryProperty = internalQueryAdapterType.GetProperty("InternalQuery", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            internalQueryType = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Internal.Linq.InternalQuery`1");

            var iInternalQueryType = typeof(System.Data.Entity.DbContext).Assembly.GetType("System.Data.Entity.Internal.Linq.IInternalQuery");
            iInternalQueryType_ObjectQueryProviderProperty = iInternalQueryType.GetProperty("ObjectQueryProvider", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            iInternalQueryType_ElementTypeProperty = iInternalQueryType.GetProperty("ElementType", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }

        // <summary>
        // Creates a provider that wraps the given provider.
        // </summary>
        // <param name="internalQuery"> The internal query to wrap. </param>
        public DbQueryProvider(IQueryProvider efQueryProvider, RepositoryInternalBase repository)
        {
            _efQueryProvider = GiveUnderlyingQueryProvider(efQueryProvider);
            _repository = repository;

            var field_internalContext = _efQueryProvider.GetType().GetField("_internalContext", BindingFlags.Instance | BindingFlags.NonPublic);
            var field_internalQuery = _efQueryProvider.GetType().GetField("_internalQuery", BindingFlags.Instance | BindingFlags.NonPublic);

            _internalContext = field_internalContext.GetValue(efQueryProvider);
            _internalQuery = field_internalQuery.GetValue(efQueryProvider);
        }

        internal static IQueryProvider GiveUnderlyingQueryProvider(IQueryProvider provider)
        {
            if (provider is DbQueryProvider) return GiveUnderlyingQueryProvider((provider as DbQueryProvider)._efQueryProvider);
            else return provider;
        }

        internal static object GetIInternalQuery(IQueryable query)
        {
            if (internalQueryAdapterType.IsAssignableFrom(query.GetType()))
            {
                var internalQuery = internalQueryAdapterType_InternalQueryProperty.GetValue(query, null);
                return internalQuery;

            }
            else if (query is QueryInternal)
            {
                return GetIInternalQuery((query as QueryInternal)._query);
            }
            else if (query is RepositoryInternalBase)
            {
                return GetIInternalQuery((query as RepositoryInternalBase).GetQuery());
            }
            else if (query is Data.IQuery)
            {
                return GetIInternalQuery(((query as Data.IQuery).Repository as RepositoryInternalBase).GetQuery());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IQueryProvider Members

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider before wrapping
        // the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" />.
        // </summary>
        public virtual IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            //Check.NotNull(expression, "expression");

            var objectQuery = CreateObjectQuery(expression);

            // If the ElementType is different than the generic type then we need to use the ElementType
            // for the underlying type because then we can support covariance at the IQueryable level. That
            // is, it is possible to create IQueryable<object>.
            if (typeof(TElement) != ((IQueryable)objectQuery).ElementType)
            {
                return new QueryInternal<TElement>((IQueryable<TElement>)CreateQuery(objectQuery), _repository as Data.IRepository);
            }

            var internalQuery = Activator.CreateInstance(internalQueryType.MakeGenericType(typeof(TElement)), new object[] { _internalContext, objectQuery });

            var constructor = typeof(DbQuery<>).MakeGenericType(typeof(TElement)).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            var dbQuery = (IQueryable<TElement>)constructor.Invoke(new object[] { internalQuery });

            return new QueryInternal<TElement>(dbQuery, _repository as Data.IRepository);
        }

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider before wrapping
        // the returned <see cref="ObjectQuery" /> as a <see cref="DbQuery{T}" /> where T is determined
        // from the element type of the ObjectQuery.
        // </summary>
        public virtual IQueryable CreateQuery(Expression expression)
        {
            //Check.NotNull(expression, "expression");

            return new QueryInternal(CreateQuery(CreateObjectQuery(expression)), _repository as Data.IRepository);
        }

        #region Execute
        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        public virtual TResult Execute<TResult>(Expression expression)
        {
            return _efQueryProvider.Execute<TResult>(expression);
        }

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        public virtual object Execute(Expression expression)
        {
            return _efQueryProvider.Execute(expression);
        }
        #endregion

        #endregion

        #region IDbAsyncQueryProvider Members

#if !NET40

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        Task<TResult> IDbAsyncQueryProvider.ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        // <summary>
        // By default, calls the same method on the wrapped provider.
        // </summary>
        Task<object> IDbAsyncQueryProvider.ExecuteAsync(Expression expression, CancellationToken cancellationToken)
        {
            Check.NotNull(expression, "expression");

            cancellationToken.ThrowIfCancellationRequested();

            _internalContext.Initialize();

            return ((IDbAsyncQueryProvider)_internalQuery.ObjectQueryProvider).ExecuteAsync(expression, cancellationToken);
        }

#endif

        #endregion

        #region Helpers

        // <summary>
        // Creates an appropriate generic IQueryable using Reflection and the underlying ElementType of
        // the given ObjectQuery.
        // </summary>
        private IQueryable CreateQuery(ObjectQuery objectQuery)
        {
            var internalQuery = CreateInternalQuery(objectQuery);
            var internalQuery_ElementType = iInternalQueryType_ElementTypeProperty.GetValue(internalQuery, null) as Type;

            var genericDbQueryType = typeof(DbQuery<>).MakeGenericType(internalQuery_ElementType);
            var constructor = genericDbQueryType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Single();
            return (IQueryable)constructor.Invoke(new object[] { internalQuery });
        }

        // <summary>
        // Performs expression replacement and then delegates to the wrapped provider to create an
        // <see cref="ObjectQuery" />.
        // </summary>
        protected ObjectQuery CreateObjectQuery(Expression expression)
        {
            //DebugCheck.NotNull(expression);

            expression = new DbQueryVisitor().Visit(expression);

            var ObjectQueryProvider = iInternalQueryType_ObjectQueryProviderProperty.GetValue(_internalQuery, null) as IQueryProvider;

            return (ObjectQuery)ObjectQueryProvider.CreateQuery(expression);
        }

        // <summary>
        // Wraps the given <see cref="ObjectQuery" /> as a <see cref="InternalQuery{T}" /> where T is determined
        // from the element type of the ObjectQuery.
        // </summary>
        protected object CreateInternalQuery(ObjectQuery objectQuery)
        {
            //DebugCheck.NotNull(objectQuery);

            var genericInternalQueryType = internalQueryType.MakeGenericType(((IQueryable)objectQuery).ElementType);
            var constructor = genericInternalQueryType.GetConstructors().Where(x => x.GetParameters().Length == 2).FirstOrDefault();
            return constructor.Invoke(new object[] { _internalContext, objectQuery });
        }

        #endregion
    }
}
