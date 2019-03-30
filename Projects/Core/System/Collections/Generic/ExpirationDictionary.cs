using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    class ExpirationDictionary<TKey, TValue> : ExpirationDictionaryBase<Dictionary<TKey, TValue>, TKey, TValue>
    {
        public ExpirationDictionary(TimeSpan expirationTimeout) : base(expirationTimeout)
        {

        }
    }
}
