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
        /// Возвращает копию объекта <paramref name="source"/> и добавляет к ней содержимое коллекций, переданных в аргументах.
        /// Содержимое коллекций добавляется с заменой содержимого при совпадении ключей. Например, если в качестве аргументов передана одна коллекция, содержащая пару 1 : "один" и <paramref name="source"/> содержит пару 1 : "one", то значение "one" будет заменено на "один".
        /// </summary>
        /// <param name="source">Исходный объект</param>
        /// <param name="collectionsToAdd">Коллекции, содержимое которых следует объединить с содержимым <paramref name="source"/>.</param>
        public static TDictionary Merge<TDictionary, TKey, TValue>(this TDictionary source, params IDictionary<TKey, TValue>[] collectionsToAdd) where TDictionary : class, IDictionary<TKey, TValue>
        {
            return (source as IDictionary<TKey, TValue>).Merge(collectionsToAdd) as TDictionary;
        }

        /// <summary>
        /// Аналог <see cref="Merge{TDictionary, TKey, TValue}(TDictionary, IDictionary{TKey, TValue}[])"/>.
        /// </summary>
        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> source, params IDictionary<TKey, TValue>[] collectionsToAdd)
        {
            var dictionary = source.Clone();

            foreach (var toAdd in collectionsToAdd)
                foreach (var pair in toAdd)
                    dictionary[pair.Key] = pair.Value;

            return dictionary;
        }

        /// <summary>
        /// Возвращает копию объекта <paramref name="source"/> и добавляет к ней содержимое коллекций, переданных в аргументах.
        /// </summary>
        /// <param name="source">Исходный объект</param>
        /// <param name="collectionsToAdd">Коллекции, содержимое которых следует объединить с содержимым <paramref name="source"/>.</param>
        public static TCollection Merge<TCollection, TItem>(this TCollection source, params IEnumerable<TItem>[] collectionsToAdd) where TCollection : class, ICollection<TItem>
        {
            return (source as ICollection<TItem>).Merge(collectionsToAdd) as TCollection;
        }

        /// <summary>
        /// Аналог <see cref="Merge{TCollection, TItem}(TCollection, IEnumerable{TItem}[])"/>.
        /// </summary>
        public static ICollection<TItem> Merge<TItem>(this ICollection<TItem> source, params IEnumerable<TItem>[] collectionsToAdd)
        {
            var collection = source.Clone();

            foreach (var toAdd in collectionsToAdd)
                foreach (var item in toAdd)
                    if (!collection.Contains(item))
                        collection.Add(item);

            return collection;
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