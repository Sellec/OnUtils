using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Startup
{
    /// <summary>
    /// Инициализатор библиотеки OnUtils.Core.
    /// Во время вызова <see cref="Startup"/> выполняется ряд инициализирующих действий. Метод вызывается только один, все повторные вызовы игнорируются. Вызов потокобезопасный - параллельное выполнение блокируется.
    /// Действия во время инициализации:
    /// 1) Выполняется создание экземпляров типов, реализующих <see cref="IStartup"/> и вызов их методов <see cref="IStartup.Startup"/>;
    /// </summary>
    public static class StartupFactory
    {
        private static bool _isInitialized = false;
        private static object SyncRoot = new object();
        private static ConcurrentDictionary<Assembly, DateTime> _preparedAssemblyList = new ConcurrentDictionary<Assembly, DateTime>();

        /// <summary>
        /// </summary>
        public static bool Startup()
        {
            try
            {
                lock (SyncRoot)
                {
                    if (!_isInitialized)
                    {
                        var measure = new MeasureTime();

                        var entryAssembly = Assembly.GetEntryAssembly();
                        Debug.WriteLine("StartupFactory={0}", entryAssembly?.FullName);

                        if (IsDeveloperRuntime())
                        {
                            Debug.WriteLine("StartupEntryAssembly=null. Считаем, что загрузка произошла в VisualStudio и прекращаем привязку. Далее часть отладочной информации.");
                            _isInitialized = true;
                            return false;
                        }

                        var loadedAssemblies = AppDomain.
                            CurrentDomain.GetAssemblies().
                            Where(x => Reflection.LibraryEnumerator.FilterDevelopmentRuntime(x.FullName, null));

                        PrepareAssembly(loadedAssemblies.ToArray());

                        Debug.WriteLine("Startup load assemblies ends with {0}ms", measure.Calculate().TotalMilliseconds);

                        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;

                        _isInitialized = true;

                    }
                }
            }
            catch { }

            return true;
        }

        private static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            PrepareAssembly(args.LoadedAssembly);
        }

        private static void PrepareAssembly(params Assembly[] assemblyList)
        {
            var listToPrepare = new List<Assembly>();
            foreach (var _assembly in assemblyList)
                if (_preparedAssemblyList.TryAdd(_assembly, DateTime.Now))
                    listToPrepare.Add(_assembly);

            if (listToPrepare.Count > 0)
            {
                var numerator = new Reflection.LibraryEnumerator((assembly) =>
                {
                    try
                    {
                        if (Global.CheckIfIgnoredAssembly(assembly)) return;

                        var tStartup = typeof(IStartup);
                        var startupList = assembly.GetTypes().Where(x => x.IsClass && x.IsPublic && tStartup.IsAssignableFrom(x));
                        var constructors = startupList.Select(x => new { Type = x, Constructor = x.GetConstructor(new Type[] { }) }).ToList();

                        foreach (var pair in constructors)
                        {
                            try
                            {
                                if (pair.Constructor == null) throw new TypeInitializationException(pair.Type.FullName, new Exception($"Для типа '{pair.Type.FullName}', объявленного как инициализатор через интерфейс '{typeof(IStartup).FullName}', отсутствует открытый беспараметрический конструктор"));

                                var startup = pair.Constructor.Invoke(null) as IStartup;
                                startup.Startup();
                            }
                            catch (Exception ex) { RaiseStartupError(assembly, pair.Type, ex); }
                        }
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.WriteLine("PrepareAssembly error: {0}, {1}", assembly.FullName, ex.Message);
                        foreach (var ex2 in ex.LoaderExceptions)
                            Debug.WriteLine("StartupFactory.PrepareAssembly error2: {0}", ex2);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("PrepareAssembly error: {0}, {1}", assembly.FullName, ex.Message);
                    }
                }, null, LibraryEnumeratorFactory.EnumerateAttrs.ExcludeMicrosoft | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeSystem | LibraryEnumeratorFactory.EnumerateAttrs.ExcludeKnownExternal, LibraryEnumeratorFactory.GlobalAssemblyFilter, !_isInitialized ? LibraryEnumeratorFactory.LoggingOptions : LibraryEnumeratorFactory.eLoggingOptions.None, "StartupFactory.PrepareAssembly", false);

                numerator.Enumerate(listToPrepare);
            }
        }

        private static void RaiseStartupError(Assembly assembly, Type startupType, Exception ex)
        {
            foreach(var _delegate in StartupError.GetInvocationList())
            {
                try
                {
                    _delegate.DynamicInvoke(assembly, startupType, ex);
                }
                catch { }
            }
        }

        static event Action<Assembly, Type, Exception> StartupError;

        /// <summary>
        /// </summary>
        public static bool IsDeveloperRuntime()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly == null)
            {
                //Проверяем, что это не ASP.NET
                var stackTrace = new global::System.Diagnostics.StackTrace();

                if (System.Diagnostics.Process.GetCurrentProcess().ProcessName == "Microsoft.VisualStudio.Web.Host")
                {
                    return true;
                }
                else
                {
                    var firstMethod = stackTrace.GetFrames().Last().GetMethod();
                    if (firstMethod.Name == "Initialize" && firstMethod.Module.Name.ToLower() == "system.web.dll") { }
                    else if (firstMethod.Name == "InitializeApplication" && firstMethod.Module.Name.ToLower() == "system.web.dll") { }
                    else
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// </summary>
    //Не удалять! Нужен для moduleinjector для web
    public class ModuleInjector
    {
        /// <summary>
        /// </summary>
        public static void InjectorLoader()
        {
            Debug.WriteLineNoLog("Injected Core library into runtime!");
        }
    }
}
