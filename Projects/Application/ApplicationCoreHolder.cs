using System;
using System.Linq;
using System.Collections.Concurrent;

namespace OnUtils.Application
{
    static class ApplicationCoreHolder
    {
        private static object SyncRoot = new object();
        private static ConcurrentDictionary<Type, object> _applicationCoreInstances = new ConcurrentDictionary<Type, object>();

        public static TAppCoreSelfReference Get<TAppCoreSelfReference>()
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            lock (SyncRoot)
            {
                return _applicationCoreInstances.Where(x => x.Key == typeof(TAppCoreSelfReference)).Select(x => (TAppCoreSelfReference)x.Value).FirstOrDefault();
            }
        }

        public static void Set<TAppCoreSelfReference>(TAppCoreSelfReference applicationCore)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            lock (SyncRoot)
            {
                _applicationCoreInstances.AddOrUpdate(
                    typeof(TAppCoreSelfReference),
                    key => applicationCore,
                    (key, old) =>
                    {
                        if (old != applicationCore) Debug.WriteLine($"Установлен новый экземпляр приложения '{applicationCore.GetType().FullName}' на базе '{typeof(TAppCoreSelfReference).FullName}'. Возможны проблемы с определением активного модуля при создании экземпляров ItemBase. Для корректной работы убедитесь, что предыдущее зарегистрированное ядро такого типа было остановлено.");
                        return applicationCore;
                    }
                );
            }
        }

        public static void Remove<TAppCoreSelfReference>(TAppCoreSelfReference applicationCore)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            lock (SyncRoot)
            {
                if (_applicationCoreInstances.TryRemove(typeof(TAppCoreSelfReference), out var instance) && instance != applicationCore)
                {
                    _applicationCoreInstances.TryAdd(typeof(TAppCoreSelfReference), instance);
                }
            }
        }


    }
}
