using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using OnUtils.Data;

namespace System
{
    #pragma warning disable
    /// <summary>
    /// </summary>
    public static class RepositoryAndQueryExtensions
    {
        /// <summary>
        /// Помечает объекты, полученные в результате запроса <paramref name="source"/>, на удаление.
        /// </summary>
        /// <param name="source"></param>
        public static int Delete<T>(this IQueryable<T> source) where T : class
        {
            var query = GetRepositoryQuery(source);

            //if (query.IsNoTracking()) throw new Exception("Запрос является неотслеживаемым (AsNoTracking), для него невозможно удаление сущностей.");

            int counter = 0;
            if (query != null)
            {
                foreach (var item in query)
                {
                    (query.Repository as IRepository<T>).Delete(item);
                    counter++;
                }
            }

            return counter;
        }

        /// <summary>
        /// Задает связанные объекты, включаемые в результаты запроса.
        /// </summary>
        /// <param name="source">Исходный запрос.</param>
        /// <param name="path">Разделенный точками список связанных объектов, включаемых в результаты запроса.</param>
        /// <returns>Новый запрос <see cref="IQueryable{T}"/> с определенным путем запроса.</returns>
        public static IQueryable<T> Include<T>(this IQueryable<T> source, string path) where T : class
        {
            var query = GetRepositoryQuery(source);
            return query.Include(path);
        }

        /// <summary>
        /// Задает связанные объекты, включаемые в результаты запроса.
        /// </summary>
        /// <param name="source">Исходный запрос.</param>
        /// <param name="path">Выражение, описывающее связанные объекты, включаемые в результаты запроса.</param>
        /// <returns>Новый запрос <see cref="IQueryable{T}"/> с определенным путем запроса.</returns>
        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> path) where T : class
        {
            var query = GetRepositoryQuery(source);
            return query.Include(path);
        }

        /// <summary>
        /// Выполняет запрос <paramref name="source"/>, выполняет перечисление всех объектов, полученных в запросе и для каждого вызывает <paramref name="action"/>.
        /// </summary>
        /// <returns>Возвращает количество записей, обработанных в перечислении.</returns>
        public static int ForEach<T>(this IQueryable<T> source, Action<T> action) where T : class
        {
            int counter = 0;
            foreach (var item in source)
            {
                action(item);
                counter++;
            }
            return counter;
        }

        private static IQuery<T> GetRepositoryQuery<T>(IQueryable<T> source) where T : class
        {
            var d = source as IQuery<T>;
            if (d == null)
            {
                var dt = source.GetType();
                throw new System.Data.EvaluateException("Данный запрос, к которому Вы пытаетесь применить метод расширения, не относится к механизму репозиториев OnUtils.Data.");
            }

            return d;
        }
    }
    #pragma warning restore
}