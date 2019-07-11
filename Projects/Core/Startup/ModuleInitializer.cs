using OnUtils.Startup;
using System;
using System.Linq;
using System.Reflection;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        try
        {
            var IsDeveloperRuntime = StartupFactory.IsDeveloperRuntime();

            if (IsDeveloperRuntime)
            {
                var assemblyReferences = typeof(StartupFactory).Assembly.GetReferencedAssemblies();
                foreach (var assembly in assemblyReferences)
                {
                    try { Assembly.Load(assembly.FullName); }
                    catch { }
                }
            }


            bool IsNeedStartup = !IsDeveloperRuntime;   //Для сред разработки НЕ надо грузить библиотеки.

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                var attr = entryAssembly.GetCustomAttributes(typeof(StartupBehaviourAttribute), true).FirstOrDefault();
                if (attr != null)
                {
                    IsNeedStartup = (attr as StartupBehaviourAttribute).IsNeedStartupFactoryAuto;
                }
            }

            if (IsNeedStartup) StartupFactory.Startup();
        }
        catch (Exception ex) { Debug.WriteLine("Core.ModuleInitializer Error: {0}", ex.ToString()); }
    }
}
