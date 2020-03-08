using System;
using System.Linq;
using System.Reflection;

namespace OnUtils.Data.EntityFramework
{
    using Data;
    using Data.UnitOfWork;

    //  using CqtExpression = System.Data.Entity.Core.Common.CommandTrees.DbExpression;

    /// <summary>
    /// Сервис, обеспечивающий построение контекстов данных и репозиториев на основе библиотеки EntityFramework.
    /// </summary>
    public class DataService : IDataService
    {
        /// <summary>
        /// Создает новый экземпляр объекта.
        /// </summary>
        public DataService()
        {
        }

        void IDataService.Initialize()
        {

        }

        IDataContext IDataService.CreateDataContext(Action<IModelAccessor> modelAccessorDelegate, Type[] entityTypes)
        {
            return new Internal.DataContextInternal(modelAccessorDelegate, entityTypes);
        }

        IRepository<TEntity> IDataService.CreateRepository<TEntity>(IDataContext context)
        {
            if (context is Internal.DataContextInternal)
                return new Internal.RepositoryInternal<TEntity>(context as Internal.DataContextInternal);
            else
                throw new ArgumentException("Неправильный контекст данных. Он должен принадлежать этому же провайдеру данных.", nameof(context));
        }
    }
}