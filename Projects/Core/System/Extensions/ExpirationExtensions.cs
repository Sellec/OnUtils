using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace System
{
    /// <summary>
    /// </summary>
    public static class ExpirationExtensions
    {
        /// <summary>
        /// Вызывает <see cref="IDictionary{TKey, TValue}.Add(TKey, TValue)"/> с указанием времени <paramref name="expirationTimeout"/>, 
        /// через которое элемент с ключом <paramref name="key"/> будет удален из коллекции.
        /// </summary>
        public static void AddWithExpiration<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, TimeSpan expirationTimeout)
        {
            dictionary.Add(key, value);
            var timer = new Timer((o) =>
            {
                try { if (dictionary.ContainsKey(key)) dictionary.Remove(key); }
                catch { }
            }, null, (int)expirationTimeout.TotalMilliseconds, Timeout.Infinite);
        }

        /// <summary>
        /// Вызывает <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, TValue)"/> с указанием времени <paramref name="expirationTimeout"/>, 
        /// через которое элемент с ключом <paramref name="key"/> будет удален из коллекции.
        /// </summary>
        public static TValue GetOrAddWithExpiration<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, TValue value, TimeSpan expirationTimeout)
        {
            return dictionary.GetOrAdd(key, (k) =>
            {
                var timer = new Timer((o) =>
                {
                    try
                    {
                        TValue tt;
                        if (dictionary.ContainsKey(key)) dictionary.TryRemove(key, out tt);
                    }
                    catch { }
                }, null, (int)expirationTimeout.TotalMilliseconds, Timeout.Infinite);
                return value;
            });
        }

        /// <summary>
        /// Вызывает <see cref="ConcurrentDictionary{TKey, TValue}.GetOrAdd(TKey, TValue)"/> с указанием времени <paramref name="expirationTimeout"/>, 
        /// через которое элемент с ключом <paramref name="key"/> будет удален из коллекции.
        /// </summary>
        public static TValue GetOrAddWithExpiration<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory, TimeSpan expirationTimeout)
        {
            return dictionary.GetOrAdd(key, (k) =>
            {
                var timer = new Timer((o) =>
                {
                    try
                    {
                        TValue tt;
                        if (dictionary.ContainsKey(key)) dictionary.TryRemove(key, out tt);
                    }
                    catch { }
                }, null, (int)expirationTimeout.TotalMilliseconds, Timeout.Infinite);
                return valueFactory(key);
            });
        }

        /// <summary>
        /// Выполняет присваивание через <see cref="IDictionary{TKey, TValue}.this[TKey]"/> с указанием времени <paramref name="expirationTimeout"/>, 
        /// через которое элемент с ключом <paramref name="key"/> будет удален из коллекции.
        /// </summary>
        public static void SetWithExpiration<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value, TimeSpan expirationTimeout)
        {
            dictionary[key] = value;
            var timer = new Timer((o) =>
            {
                try { if (dictionary.ContainsKey(key)) dictionary.Remove(key); }
                catch { }
            }, null, (int)expirationTimeout.TotalMilliseconds, Timeout.Infinite);
        }
    }
}
