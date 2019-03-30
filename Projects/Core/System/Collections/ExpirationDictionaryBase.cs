using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections
{
    /// <summary>
    /// Базовый класс для коллекций с истечением срока давности.
    /// Содержимое таких коллекций очищается автоматически по таймауту.
    /// </summary>
    abstract class ExpirationDictionaryBase<TDictionary, TKey, TValue> : IDictionary<TKey, TValue> where TDictionary : IDictionary<TKey, TValue>, new()
    {
        private TimeSpan _timeout = TimeSpan.MinValue;
        private DateTime _clearTime = DateTime.MinValue;

        private TDictionary _sourceDictionary = new TDictionary();

        /// <summary>
        /// </summary>
        /// <param name="expirationTimeout">Время, в течении которого должны храниться данные в коллекции.</param>
        public ExpirationDictionaryBase(TimeSpan expirationTimeout)
        {
            _timeout = expirationTimeout;
        }

        private void CheckExpiration()
        {
            if (_clearTime <= DateTime.Now && _sourceDictionary.Count > 0) _sourceDictionary.Clear();
            _clearTime = DateTime.Now.Add(_timeout);
        }

        public bool ContainsKey(TKey key)
        {
            CheckExpiration();
            return _sourceDictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            CheckExpiration();
            _sourceDictionary.Add(key, value);
        }

        public bool Remove(TKey key)
        {
            return _sourceDictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            CheckExpiration();
            return _sourceDictionary.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            CheckExpiration();
            _sourceDictionary.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _sourceDictionary.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            CheckExpiration();
            return _sourceDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            CheckExpiration();
            ((IDictionary<TKey,TValue>)_sourceDictionary).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _sourceDictionary.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            CheckExpiration();
            return _sourceDictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            CheckExpiration();
            return _sourceDictionary.GetEnumerator();
        }

        public ICollection<TKey> Keys
        {
            get
            {
                CheckExpiration();
                return _sourceDictionary.Keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                CheckExpiration();
                return _sourceDictionary.Values;
            }
        }

        public int Count
        {
            get
            {
                CheckExpiration();
                return _sourceDictionary.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return ((IDictionary<TKey, TValue>)_sourceDictionary).IsReadOnly;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                CheckExpiration();
                return _sourceDictionary[key];
            }

            set
            {
                CheckExpiration();
                _sourceDictionary[key] = value;
            }
        }

    }
}
