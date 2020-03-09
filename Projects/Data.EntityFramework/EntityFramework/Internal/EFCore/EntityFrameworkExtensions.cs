using OnUtils.Data;
using OnUtils.Data.EntityFramework.Internal;
using OnUtils.Data.UnitOfWork;
using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// </summary>
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Предоставляет доступ к процессам создания и настройки модели контекста в EntityFramework Core через делегаты <paramref name="onConfiguringDelegate"/> и <paramref name="onModelCreatingDelegate"/>.
        /// </summary>
        public static void UseEntityFrameworkCore(this IModelAccessor modelAccessor, Action<DbContextOptionsBuilder> onConfiguringDelegate, Action<ModelBuilder> onModelCreatingDelegate)
        {
            if (modelAccessor is ModelAccessorInternal modelAccessorInternal)
            {
                modelAccessorInternal.ConfiguringDelegate = onConfiguringDelegate;
                modelAccessorInternal.ModelCreatingDelegate = onModelCreatingDelegate;
            }
            else
            {
                throw new InvalidOperationException("This can be used only for EF Core context.");
            }
        }

        /// <summary>
        /// Возвращает <see cref="DbSet{TEntity}"/>.
        /// </summary>
        public static DbSet<TEntity> GetDbSet<TEntity>(this IRepository<TEntity> repository)
            where TEntity : class
        {
            return (repository is RepositoryInternal<TEntity> repositoryInternal)
                ? repositoryInternal.DbSet
                : (repository.Repository is RepositoryInternal<TEntity> repositoryInternal2)
                    ? repositoryInternal2.DbSet
                    : throw new InvalidCastException("Cannot take inner DbSet instance.");
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
