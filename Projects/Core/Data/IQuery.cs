using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections;
using System.Linq.Expressions;

namespace OnUtils.Data
{
    /// <summary>
    /// Представляет запрос LINQ to Entities к контексту <see cref="IDataContext"/>.
    /// </summary>
    public interface IQuery : IQueryable
    {
        /// <summary>
        /// Возвращает репозиторий, породивший запрос.
        /// </summary>
        IRepository Repository { get; }
    }

    /// <summary>
    /// Представляет запрос LINQ to Entities к контексту <see cref="IDataContext"/> для типа данных <typeparamref name="TEntity"/>.
    /// </summary>
    public interface IQuery<TEntity> : IQuery, IQueryable<TEntity>
    {
        /// <summary>
        /// Отключает кеширование для нового запроса к репозиторию.
        /// </summary>
        /// <returns>Новый запрос с атрибутом NoTracking или исходный запрос, если атрибут NoTracking не поддерживается</returns>
        IQuery<TEntity> AsNoTracking();

        /// <summary>
        /// Указывает, является ли данный запрос некешируемым (т.е. к которому был применен метод <see cref="AsNoTracking"/>).
        /// </summary>
        /// <returns></returns>
        bool IsNoTracking();

        /// <summary>
        /// Задает связанные объекты, включаемые в результаты запроса.
        /// </summary>
        /// <param name="path">Разделенный точками список связанных объектов, включаемых в результаты запроса.</param>
        /// <returns>Новый запрос <see cref="IQuery{TEntity}"/> с определенным путем запроса.</returns>
        IQuery<TEntity> Include(string path);

        /// <summary>
        /// Задает связанные объекты, включаемые в результаты запроса.
        /// </summary>
        /// <param name="path">Выражение, описывающее связанные объекты, включаемые в результаты запроса.</param>
        /// <returns>Новый запрос <see cref="IQuery{TEntity}"/> с определенным путем запроса.</returns>
        IQuery<TEntity> Include<TProperty>(Expression<Func<TEntity, TProperty>> path);

    }

}
