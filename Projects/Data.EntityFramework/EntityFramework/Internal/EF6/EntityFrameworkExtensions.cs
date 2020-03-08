using System;
using System.Data.Entity;

namespace OnUtils.Data.EntityFramework
{
    using UnitOfWork;

    /// <summary>
    /// </summary>
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Предоставляет доступ к процессу создания модели в EntityFramework через делегат <paramref name="modelBuilderDelegate"/>.
        /// </summary>
        public static void UseEntityFramework(this IModelAccessor modelAccessor, Action<DbModelBuilder> modelBuilderDelegate)
        {
            if (modelAccessor is Internal.ModelAccessorInternal modelAccessorInternal)
            {
                modelAccessorInternal.ModelBuilderDelegate = modelBuilderDelegate;
            }
            else
            {
                throw new InvalidOperationException("This can be used only for EF context.");
            }
        }

        /// <summary>
        /// Возвращает <see cref="DbSet{TEntity}"/>.
        /// </summary>
        public static DbSet<TEntity> GetDbSet<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
        {
            return ((Internal.RepositoryInternal<TEntity>)repository).DbSet;
        }

        /// <summary>
        /// Возвращает <see cref="DbContext"/>.
        /// </summary>
        public static DbContext GetDbContext(this IDataContext dataContext)
        {
            return (DbContext)dataContext;
        }
    }
}
