using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    /// <summary>
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Добавляет список объектов <paramref name="items"/> в коллекцию <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<TItem>(this ICollection<TItem> collection, params TItem[] items)
        {
            if (items != null) foreach (var item in items) collection.Add(item);
        }

        /// <summary>
        /// Добавляет список объектов <paramref name="items"/> в коллекцию <paramref name="collection"/>.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="collection"></param>
        /// <param name="items"></param>
        public static void AddRange<TItem>(this ICollection<TItem> collection, ICollection<TItem> items)
        {
            if (items != null) foreach (var item in items) collection.Add(item);
        }

        /// <summary>
        /// Добавляет объекты <paramref name="items"/> в коллекцию <paramref name="collection"/>. Если объекты уже добавлены в коллекцию, то ничего не делает.
        /// Аналог конструкции if (!collection.Contains(item)) collection.Add(item);
        /// </summary>
        public static void AddIfNotExists<TItem>(this ICollection<TItem> collection, params TItem[] items)
        {
            foreach (var item in items)
                if (!collection.Contains(item))
                    collection.Add(item);
        }

        /// <summary>
        /// Возвращает значение из словаря <paramref name="dictionary"/> по ключу <paramref name="key"/>. Если такой ключ отсутствует, то вернет значение <paramref name="defaultValue"/>.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue = default(TValue))
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : defaultValue;
        }

        /// <summary>
        /// Возвращает значение из словаря <paramref name="dictionary"/> по ключу <paramref name="key"/>. Если такой ключ отсутствует, то вернет значение, полученное вызовом <paramref name="defaultValueProvider"/>.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> defaultValueProvider)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : (defaultValueProvider != null ? defaultValueProvider(key) : default(TValue));
        }

        /// <summary>
        /// Возвращает значение из словаря <paramref name="dictionary"/> по ключу <paramref name="key"/>. 
        /// Если такой ключ отсутствует, то вернет значение, полученное вызовом <paramref name="defaultValueProvider"/>.
        /// В <paramref name="defaultValueProvider"/> передается ключ <paramref name="key"/> и дополнительная информация <paramref name="createNewInfo"/>.
        /// </summary>
        public static TValue GetValueOrDefault<TKey, TValue, TInfo>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TInfo, TValue> defaultValueProvider, TInfo createNewInfo)
        {
            return dictionary.TryGetValue(key, out TValue value) ? value : (defaultValueProvider != null ? defaultValueProvider(key, createNewInfo) : default(TValue));
        }

    }
}