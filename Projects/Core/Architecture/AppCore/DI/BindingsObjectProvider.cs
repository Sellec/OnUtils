﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OnUtils.Architecture.AppCore.DI
{
#if NET40
    using TypesCollection = Dictionary<Type, BindingDescription>;
#else
    using TypesCollection = ReadOnlyDictionary<Type, BindingDescription>;
#endif

    class BindingsObjectProvider : IBindingsObjectProvider
    {
        private static MethodInfo _methodGetInstances = null;
        private readonly TypesCollection _typesCollection = new TypesCollection(new Dictionary<Type, BindingDescription>());
        private readonly Dictionary<Type, BindingDescription> _typesCollectionResolved = new Dictionary<Type, BindingDescription>(new Dictionary<Type, BindingDescription>());
        private List<IInstanceActivatedHandler> _activatedHandlers = new List<IInstanceActivatedHandler>();
        private BindingsResolverInternal _bindingsResolver = null;

        static BindingsObjectProvider()
        {
            _methodGetInstances = typeof(BindingsObjectProvider).GetMethod(nameof(GetInstances), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(bool), typeof(bool) }, null);
            if (_methodGetInstances == null) throw new InvalidProgramException();
        }

        public BindingsObjectProvider(IEnumerable<KeyValuePair<Type, BindingDescription>> source)
        {
            var sourceDictionary = new Dictionary<Type, BindingDescription>();
            source.ForEach(x => sourceDictionary.Add(x.Key, x.Value));
            _typesCollection = new TypesCollection(sourceDictionary);
        }

        public IEnumerable<T> GetInstances<T>(bool storeInstance, bool useSingleInstance) where T : class
        {
            if (!_typesCollection.TryGetValue(typeof(T), out BindingDescription bindingDescription))
            {
                if (!_typesCollectionResolved.TryGetValue(typeof(T), out bindingDescription))
                {
                    var resolver = _bindingsResolver;
                    if (resolver == null) return null;

                    var bindingDescriptionPossible = resolver.ResolveType<T>(useSingleInstance);
                    if (bindingDescriptionPossible == null) return null;
                    // здесь должны быть проверки.
                    _typesCollectionResolved[typeof(T)] = bindingDescriptionPossible;
                    bindingDescription = bindingDescriptionPossible;
                }
            }

            if (bindingDescription == null) return null;

            lock (bindingDescription.InstancesSetSyncRoot)
            {
                if (!bindingDescription.Instances.IsNullOrEmpty())
                {
                    return bindingDescription.Instances.Select(x => (T)x);
                }
                else if (bindingDescription.BindedTypes.Count > 0)
                {
                    var typesToCreateInstance = useSingleInstance ? bindingDescription.BindedTypes.Take(1) : bindingDescription.BindedTypes;
                    var createdInstances = typesToCreateInstance.Select(x => (T)x.Activator()).ToList();
                    if (_activatedHandlers != null)
                    {
                        var activatedHadlersSnapshot = _activatedHandlers.ToList();
                        createdInstances.ForEach(instance => activatedHadlersSnapshot.ForEach(handler => handler.OnInstanceActivated<T>(instance)));
                    }
                    if (storeInstance) bindingDescription.Instances = createdInstances;
                    return createdInstances;
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
            if (_typesCollection.TryGetValue(queryType, out BindingDescription bindingDescription))
            {
                return bindingDescription.BindedTypes.Select(x => x.Type).ToList();
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<Type> GetQueryTypes()
        {
            return _typesCollection.Keys.ToList();
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
