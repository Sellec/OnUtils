using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Аналог <see cref="Lazy{T}"/> без кеширования исключений, могущих возникнуть во время создания значения.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LazyWithoutExceptionCaching<T>
    {
        private readonly Func<T> _valueFactory;
        private Lazy<T> _lazy;

        /// <summary>
        /// </summary>
        public LazyWithoutExceptionCaching(Func<T> valueFactory)
        {
            _valueFactory = valueFactory;
            _lazy = new Lazy<T>(valueFactory);
        }

        /// <summary>
        /// См. <see cref="Lazy{T}.Value"/> 
        /// </summary>
        public T Value
        {
            get
            {
                try
                {
                    return _lazy.Value;
                }
                catch (Exception)
                {
                    _lazy = new Lazy<T>(_valueFactory);
                    throw;
                }
            }
        }

        /// <summary>
        /// См. <see cref="Lazy{T}.IsValueCreated"/> 
        /// </summary>
        public bool IsValueCreated
        {
            get
            {
                return _lazy.IsValueCreated;
            }
        }
    }
}
