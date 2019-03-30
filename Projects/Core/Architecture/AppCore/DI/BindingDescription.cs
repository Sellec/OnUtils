using System;
using System.Collections.Generic;

namespace OnUtils.Architecture.AppCore.DI
{
    struct BindedType
    {
        public Type Type;

        public Func<object> Activator;
    }

    class BindingDescription
    {
        public BindingDescription(Type activatedType, Func<object> activator) : this(new BindedType() { Type = activatedType, Activator = activator })
        {
            if (activatedType == null) throw new ArgumentNullException(nameof(activatedType));
        }

        public BindingDescription(BindedType bindedType) : this(bindedType.ToEnumerable())
        {
            if (bindedType.Activator == null) throw new ArgumentNullException(nameof(bindedType.Activator));
            if (bindedType.Type == null) throw new ArgumentNullException(nameof(bindedType.Type));
        }

        public BindingDescription(IEnumerable<BindedType> bindedTypes)
        {
            if (bindedTypes.IsNullOrEmpty()) throw new ArgumentOutOfRangeException(nameof(bindedTypes));

            BindedTypes = new List<BindedType>(bindedTypes);
            InstancesSetSyncRoot = new object();
        }

        public readonly List<BindedType> BindedTypes;

        public IEnumerable<object> Instances;

        public object InstancesSetSyncRoot;
    }

}
