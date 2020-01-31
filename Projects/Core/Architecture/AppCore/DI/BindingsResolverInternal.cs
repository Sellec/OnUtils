using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.AppCore.DI
{
    abstract class BindingsResolverInternal
    {
        public abstract BindingDescription ResolveType<TRequestedType>(bool isSingleton);
    }
}
