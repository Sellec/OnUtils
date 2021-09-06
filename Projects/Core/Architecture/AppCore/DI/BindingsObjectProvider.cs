using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OnUtils.Architecture.AppCore.DI
{
    using BindingDescriptionOrdered = Tuple<BindingDescription, int>;
#if NET40
    using TypesCollection = Dictionary<Type, Tuple<BindingDescription, int>>;
#else
    using TypesCollection = ReadOnlyDictionary<Type, Tuple<BindingDescription, int>>;
#endif

    class BindingsObjectProvider : IBindingsObjectProvider
    {
        class EqualityComparer : IEqualityComparer<KeyValuePair<Type, BindingDescriptionOrdered>>
        {
            bool IEqualityComparer<KeyValuePair<Type, BindingDescriptionOrdered>>.Equals(KeyValuePair<Type, BindingDescriptionOrdered> x, KeyValuePair<Type, BindingDescriptionOrdered> y)
            {
                return x.Key.Equals(y.Key);
            }

            int IEqualityComparer<KeyValuePair<Type, BindingDescriptionOrdered>>.GetHashCode(KeyValuePair<Type, BindingDescriptionOrdered> obj)
            {
                return obj.Key.GetHashCode();
            }
        }

        private static MethodInfo _methodGetInstances = null;
        private readonly TypesCollection _typesCollection = new TypesCollection(new Dictionary<Type, BindingDescriptionOrdered>());
        private readonly Dictionary<Type, BindingDescriptionOrdered> _typesCollectionResolved = new Dictionary<Type, BindingDescriptionOrdered>(new Dictionary<Type, BindingDescriptionOrdered>());
        private List<IInstanceActivatingHandler> _activatingHandlers = new List<IInstanceActivatingHandler>();
        private List<IInstanceActivatedHandler> _activatedHandlers = new List<IInstanceActivatedHandler>();
        private BindingsResolverInternal _bindingsResolver = null;

        static BindingsObjectProvider()
        {
            _methodGetInstances = typeof(BindingsObjectProvider).GetMethod(nameof(GetInstances), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(bool), typeof(bool) }, null);
            if (_methodGetInstances == null) throw new InvalidProgramException();
        }

        public BindingsObjectProvider(List<KeyValuePair<Type, BindingDescription>> source)
        {
            var sourceDictionary = new Dictionary<Type, BindingDescriptionOrdered>();
            for (int i = 0; i < source.Count; i++)
            {
                sourceDictionary.Add(source[i].Key, new BindingDescriptionOrdered(source[i].Value, i));
            }
            _typesCollection = new TypesCollection(sourceDictionary);
        }

        public bool TryAppendBinding(Type queryType, BindingDescription bindingDescription)
        {
            if (queryType == null) throw new ArgumentNullException(nameof(queryType));
            if (bindingDescription == null) throw new ArgumentNullException(nameof(bindingDescription));
            if (_typesCollection.ContainsKey(queryType) || _typesCollectionResolved.ContainsKey(queryType)) return false;

            _typesCollectionResolved[queryType] = new BindingDescriptionOrdered(bindingDescription, _typesCollectionResolved.Count);
            return true;
        }

        public IEnumerable<T> GetInstances<T>(bool storeInstance, bool useSingleInstance) where T : class
        {
            if (!_typesCollection.TryGetValue(typeof(T), out var bindingDescription))
            {
                if (!_typesCollectionResolved.TryGetValue(typeof(T), out bindingDescription))
                {
                    var resolver = _bindingsResolver;
                    if (resolver == null) return null;

                    var bindingDescriptionPossible = resolver.ResolveType<T>(storeInstance);
                    if (bindingDescriptionPossible != null)
                    {
                        // здесь должны быть проверки.
                        _typesCollectionResolved[typeof(T)] = new BindingDescriptionOrdered(bindingDescriptionPossible, _typesCollectionResolved.Count);
                        bindingDescription = new BindingDescriptionOrdered(bindingDescriptionPossible, 0);
                    }
                    else
                    {
                        // Повторная проверка - во время распознавания типа могли быть загружены дополнительные сборки, предоставившие новые привязки.
                        if (!_typesCollectionResolved.TryGetValue(typeof(T), out bindingDescription)) return null;
                    }
                }
            }
            if (bindingDescription == null) return null;

            if (!bindingDescription.Item1.Instances.IsNullOrEmpty())
            {
                return bindingDescription.Item1.Instances.Select(x => (T)x);
            }
            else
            {
                lock (bindingDescription.Item1.InstancesSetSyncRoot)
                {
                    if (!bindingDescription.Item1.Instances.IsNullOrEmpty())
                    {
                        return bindingDescription.Item1.Instances.Select(x => (T)x);
                    }
                    else if (bindingDescription.Item1.BindedTypes.Count > 0)
                    {
                        var typesToCreateInstance = useSingleInstance ? bindingDescription.Item1.BindedTypes.Take(1) : bindingDescription.Item1.BindedTypes;
                        var createdInstances = typesToCreateInstance.Select(x => (T)x.Activator()).ToList();

                        var activatingHadlersSnapshot = _activatingHandlers?.ToList();
                        if (activatingHadlersSnapshot != null) createdInstances.ForEach(instance => activatingHadlersSnapshot.ForEach(handler => handler.OnInstanceActivating<T>(instance)));

                        if (storeInstance) bindingDescription.Item1.Instances = createdInstances;

                        var activatedHadlersSnapshot = _activatedHandlers?.ToList();
                        if (activatedHadlersSnapshot != null) createdInstances.ForEach(instance => activatedHadlersSnapshot.ForEach(handler => handler.OnInstanceActivated<T>(instance)));

                        return createdInstances;
                    }
                }
            }
            return null;
        }

        public IEnumerable<object> GetInstances(Type queryType, bool storeInstance, bool useSingleInstance)
        {
            return (IEnumerable<object>)_methodGetInstances.MakeGenericMethod(queryType).Invoke(this, new object[] { storeInstance, useSingleInstance });
        }

        public IEnumerable<Type> GetBindedTypes<T>()
        {
            return GetBindedTypes(typeof(T));
        }

        public IEnumerable<Type> GetBindedTypes(Type queryType)
        {
            if (_typesCollection.TryGetValue(queryType, out var bindingDescription))
            {
                return bindingDescription.Item1.BindedTypes.Select(x => x.Type).ToList();
            }
            else if (_typesCollectionResolved.TryGetValue(queryType, out bindingDescription))
            {
                return bindingDescription.Item1.BindedTypes.Select(x => x.Type).ToList();
            }
            else
            {
                return null;
            }
        }

        public List<Type> GetQueryTypes()
        {
            return _typesCollection.
                Union(_typesCollectionResolved, new EqualityComparer()).
                OrderBy(x => x.Value.Item2).
                Select(x => x.Key).
                ToList();
        }

        void IBindingsObjectProvider.RegisterInstanceActivatingHandler(IInstanceActivatingHandler handler)
        {
            if (!_activatingHandlers.Contains(handler)) _activatingHandlers.Add(handler);
        }

        void IBindingsObjectProvider.RegisterInstanceActivatedHandler(IInstanceActivatedHandler handler)
        {
            if (!_activatedHandlers.Contains(handler)) _activatedHandlers.Add(handler);
        }

        public void RegisterBindingsResolver(BindingsResolverInternal resolver)
        {
            _bindingsResolver = resolver;
        }

    }
}
