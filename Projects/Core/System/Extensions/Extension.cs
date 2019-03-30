using System.Collections.Generic;
using System.Linq;

namespace System
{
    /// <summary>
    /// </summary>
    public static class Extension
    {
        /// <summary>
        /// </summary>
        public static Exception GetLowLevelException(this Exception ex)
        {
            if (ex.InnerException != null) return GetLowLevelException(ex.InnerException);
            return ex;
        }

        /// <summary>
        /// Проверяет, соответствует ли проверяемый объект списку значений.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static bool In<T>(this T obj, params T[] args)
        {
            return args.Contains(obj);
        }

        /// <summary>
        /// Возвращает <see cref="IEnumerable{T}"/> на основе текущего объекта.
        /// </summary>
        public static IEnumerable<T> ToEnumerable<T>(this T obj)
        {
            return new List<T>() { obj };
        }
    }
}