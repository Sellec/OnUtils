using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Data
{
    /// <summary>
    /// Класс контейнера UnitOfWork для работы с одним типом объектов <typeparamref name="TEntity1"/>.
    /// Подробнее см. <see cref="UnitOfWorkBase"/>.
    /// </summary>
    /// <typeparam name="TEntity1"></typeparam>
    public class UnitOfWork<TEntity1> : UnitOfWorkBase
        where TEntity1 : class
    {
        /// <summary>
        /// Создает новый экземпляр контейнера.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен провайдер данных (см. <see cref="DataAccessManager.SetDefaultService(IDataService)"/>).</exception>
        public UnitOfWork()
        {
        }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity1"/>.
        /// </summary>
        public IRepository<TEntity1> Repo1 { get; }
    }

    /// <summary>
    /// Класс контейнера UnitOfWork для работы с типами объектов <typeparamref name="TEntity1"/>, <typeparamref name="TEntity2"/>.
    /// Подробнее см. <see cref="UnitOfWorkBase"/>.
    /// </summary>
    /// <typeparam name="TEntity1"></typeparam>
    /// <typeparam name="TEntity2"></typeparam>
    public class UnitOfWork<TEntity1, TEntity2> : UnitOfWorkBase
        where TEntity1 : class
        where TEntity2 : class
    {
        /// <summary>
        /// Создает новый экземпляр контейнера.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен провайдер данных (см. <see cref="DataAccessManager.SetDefaultService(IDataService)"/>).</exception>
        public UnitOfWork()
        {
        }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity1"/>.
        /// </summary>
        public IRepository<TEntity1> Repo1 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity2"/>.
        /// </summary>
        public IRepository<TEntity2> Repo2 { get; }
    }

    /// <summary>
    /// Класс контейнера UnitOfWork для работы с типами объектов <typeparamref name="TEntity1"/>, <typeparamref name="TEntity2"/>, <typeparamref name="TEntity3"/>.
    /// Подробнее см. <see cref="UnitOfWorkBase"/>.
    /// </summary>
    /// <typeparam name="TEntity1"></typeparam>
    /// <typeparam name="TEntity2"></typeparam>
    /// <typeparam name="TEntity3"></typeparam>
    public class UnitOfWork<TEntity1, TEntity2, TEntity3> : UnitOfWorkBase
        where TEntity1 : class
        where TEntity2 : class
        where TEntity3 : class
    {
        /// <summary>
        /// Создает новый экземпляр контейнера.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен провайдер данных (см. <see cref="DataAccessManager.SetDefaultService(IDataService)"/>).</exception>
        public UnitOfWork()
        {
        }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity1"/>.
        /// </summary>
        public IRepository<TEntity1> Repo1 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity2"/>.
        /// </summary>
        public IRepository<TEntity2> Repo2 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity3"/>.
        /// </summary>
        public IRepository<TEntity3> Repo3 { get; }
    }

    /// <summary>
    /// Класс контейнера UnitOfWork для работы с типами объектов <typeparamref name="TEntity1"/>, <typeparamref name="TEntity2"/>, <typeparamref name="TEntity3"/>, <typeparamref name="TEntity4"/>.
    /// Подробнее см. <see cref="UnitOfWorkBase"/>.
    /// </summary>
    /// <typeparam name="TEntity1"></typeparam>
    /// <typeparam name="TEntity2"></typeparam>
    /// <typeparam name="TEntity3"></typeparam>
    /// <typeparam name="TEntity4"></typeparam>
    public class UnitOfWork<TEntity1, TEntity2, TEntity3, TEntity4> : UnitOfWorkBase
        where TEntity1 : class
        where TEntity2 : class
        where TEntity3 : class
        where TEntity4 : class
    {
        /// <summary>
        /// Создает новый экземпляр контейнера.
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен провайдер данных (см. <see cref="DataAccessManager.SetDefaultService(IDataService)"/>).</exception>
        public UnitOfWork()
        {
        }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity1"/>.
        /// </summary>
        public IRepository<TEntity1> Repo1 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity2"/>.
        /// </summary>
        public IRepository<TEntity2> Repo2 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity3"/>.
        /// </summary>
        public IRepository<TEntity3> Repo3 { get; }

        /// <summary>
        /// Возвращает репозиторий для типа объектов <typeparamref name="TEntity4"/>.
        /// </summary>
        public IRepository<TEntity4> Repo4 { get; }
    }
}
