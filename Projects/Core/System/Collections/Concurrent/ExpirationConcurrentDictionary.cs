using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Concurrent
{
    class ExpirationConcurrentDictionary<TKey, TValue> : ExpirationDictionaryBase<ConcurrentDictionary<TKey, TValue>, TKey, TValue>
    {
        public ExpirationConcurrentDictionary(TimeSpan expirationTimeout) : base(expirationTimeout)
        {

        }
    }
}
