using Microsoft.EntityFrameworkCore;
using System;

namespace OnUtils.Data.EntityFramework
{
    using UnitOfWork;

    /// <summary>
    /// </summary>
    public static class EntityFrameworkExtensions
    {
        /// <summary>
        /// Предоставляет доступ к процессам создания и настройки модели контекста в EntityFramework Core через делегаты <paramref name="onConfiguringDelegate"/> и <paramref name="onModelCreatingDelegate"/>.
        /// </summary>
        public static void UseEntityFrameworkCore(this IModelAccessor modelAccessor, Action<DbContextOptionsBuilder> onConfiguringDelegate, Action<ModelBuilder> onModelCreatingDelegate)
        {
            if (modelAccessor is Internal.ModelAccessorInternal modelAccessorInternal)
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
