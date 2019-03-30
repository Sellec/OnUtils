using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// </summary>
    public static class NameValueCollectionExtension
    {
        /// <summary>
        /// Проверяет, существует ли указанный ключ в коллекции.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool HasKey(this System.Collections.Specialized.NameValueCollection collection, string key)
        {
            var d = collection.AllKeys.Contains(key);

            return d;
        }

        /// <summary>
        /// Пытается десериализовать указанный ключ <paramref name="key"/> в коллекции в указанный тип <typeparamref name="T"/>.
        /// </summary>
        /// <returns>
        /// В случае успеха возвращает объект типа <typeparamref name="T"/>. 
        /// В противном случае возвращает значение по-умолчанию для <typeparamref name="T"/>.
        /// </returns>
        public static T TryGetValue<T>(this NameValueCollection collection, string key)
        {
            try
            {
                if (collection.HasKey(key))
                {
                    var str = collection[key];
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(str);
                }
                else
                {
                    var values = collection.GetValues(key);

                }

                return default(T);
            }
            catch { }

            return default(T);
        }

        /// <summary>
        /// Устанавливает новые значения <paramref name="values"/> для указаного ключа <paramref name="key"/> в коллекцию <paramref name="collection"/>.
        /// Все старые значения очищаются.
        /// </summary>
        public static void SetValues(this NameValueCollection collection, string key, IEnumerable<string> values)
        {
            collection.Remove(key);
            foreach (var value in values) collection.Add(key, value);
        }

        /// <summary>
        /// Устанавливает новые значения <paramref name="values"/> для указаного ключа <paramref name="key"/> в коллекцию <paramref name="collection"/>.
        /// Все старые значения очищаются.
        /// </summary>
        public static void SetValues(this NameValueCollection collection, string key, params string[] values)
        {
            SetValues(collection, key, values.AsEnumerable());
        }
    }
}