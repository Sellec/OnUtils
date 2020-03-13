using System;

namespace OnUtils.Data
{
    using UnitOfWork;

    /// <summary>
    /// Представляет сервис, обеспечивающий построение контекстов данных и репозиториев.
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// Выполняет инициализацию сервиса. Не должен вызываться из пользовательского кода, т.к. автоматически вызывается из <see cref="DataAccessManager"/>.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Возвращает новый контекст доступа к данным для списка типов <paramref name="entityTypes"/>.
        /// </summary>
        /// <param name="entityTypes">Список типов данных, зарегистрированных в контексте. Контекст сможет работать только с переданными типами данных.</param>
        /// <param name="modelAccessorDelegate">Используется для предоставления внешнему источнику доступа к процессу создания модели.</param>
        /// <returns></returns>
        IDataContext CreateDataContext(Action<IModelAccessor> modelAccessorDelegate, Type[] entityTypes);

        /// <summary>
        /// Возвращает новый репозиторий для объектов типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="context">Контекст доступа к данным, с которым будет работать репозиторий. Должен быть создан в том же провайдере данных, что и репозиторий.</param>
        /// <returns></returns>
        IRepository<TEntity> CreateRepository<TEntity>(IDataContext context) where TEntity : class;

    }
}
