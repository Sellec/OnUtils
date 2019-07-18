using System;
using System.Linq;
using System.Collections.Concurrent;

namespace OnUtils.Application
{
    /// <summary>
    /// Будет удалено в будущих версиях.
    /// </summary>
    public static class DeprecatedSingletonInstances
    {
        private static ConcurrentDictionary<Type, object> _modulesManagers = new ConcurrentDictionary<Type, object>();

        [Obsolete("Будет удалено в будущих версиях.")]
        public static Modules.ModulesManager<TAppCoreSelfReference> Get<TAppCoreSelfReference>()
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return _modulesManagers.Where(x => x.Key == typeof(TAppCoreSelfReference)).Select(x => x.Value as Modules.ModulesManager<TAppCoreSelfReference>).FirstOrDefault();
        }

        [Obsolete("Будет удалено в будущих версиях.")]
        public static void Set<TAppCoreSelfReference>(Modules.ModulesManager<TAppCoreSelfReference> manager)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            _modulesManagers[typeof(TAppCoreSelfReference)] = manager;
        }

    }
}
