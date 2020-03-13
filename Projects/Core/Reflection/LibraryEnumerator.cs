using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OnUtils.Reflection
{
    class LibraryEnumerator
    {
        private static List<string> _costuraFodyLoaded = new List<string>();
        private static object _costuraFodyLoadedSyncRoot = new object();

        private Reflection.AssemblyNameEqualityComparer _comparer = new Reflection.AssemblyNameEqualityComparer();

        private ConcurrentDictionary<AssemblyName, DateTime> _preparedAssemblies = new ConcurrentDictionary<AssemblyName, DateTime>(new Reflection.AssemblyNameEqualityComparer());
        private ConcurrentDictionary<string, DateTime> _preparedCosturaAssemblies = new ConcurrentDictionary<string, DateTime>();
        private ConcurrentDictionary<AssemblyName, DateTime> _scheduledAssemblies = new ConcurrentDictionary<AssemblyName, DateTime>();

        private ConcurrentDictionary<AssemblyName, Assembly> _assembliesToLoad = new ConcurrentDictionary<AssemblyName, Assembly>(new Reflection.AssemblyNameEqualityComparer());

        private Action<Assembly> _callbackAfterLoad = null;
        private Func<string, bool> _callbackBeforeLoad = null;

        private Func<string, bool> _globalAssemblyFilter = null;
        private LibraryEnumeratorFactory.eLoggingOptions _loggingOptions = LibraryEnumeratorFactory.eLoggingOptions.None;
        private string _nameForLogging = null;

        private IEnumerable<Assembly> _traceOnRun = null;
        private List<string> _assemblyFolders = new List<string>();

        private Guid _enumerationGuid = Guid.NewGuid();

        private bool _tasksAllowed = true;

        private LibraryEnumeratorFactory.EnumerateAttrs _enumerateAttrs = LibraryEnumeratorFactory.EnumerateAttrs.Default;

        public LibraryEnumerator(Action<Assembly> callbackAfterLoad,
                                         Func<string, bool> callbackBeforeLoad,
                                         LibraryEnumeratorFactory.EnumerateAttrs enumerateAttrs,
                                         Func<string, bool> globalAssemblyFilter,
                                         LibraryEnumeratorFactory.eLoggingOptions loggingOptions,
                                         string nameForLogging = null,
                                         bool tasksAllowed = true)
        {
            _callbackAfterLoad = callbackAfterLoad;
            _callbackBeforeLoad = callbackBeforeLoad;
            _enumerateAttrs = enumerateAttrs;
            _globalAssemblyFilter = globalAssemblyFilter;
            _loggingOptions = loggingOptions;
            _nameForLogging = nameForLogging;
            _tasksAllowed = tasksAllowed;

            _traceOnRun = new System.Diagnostics.StackTrace().GetFrames().Select(x => x.GetMethod().Module.Assembly).ToList();
        }

        public void Enumerate()
        {
            var measure1 = new MeasureTime();
            EnumerateList(AppDomain.CurrentDomain.GetAssemblies());
            EnumerateFolder();
            EnumerateScheduled();
            var t1 = measure1.Calculate();

            var measure2 = new MeasureTime();
            foreach (var pair in _assembliesToLoad)
            {
                try
                {
                    _callbackAfterLoad(pair.Value);
                }
                catch { }
            }
            var t2 = measure2.Calculate();

            if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.EnumerationSummary))
                Debug.WriteLine("{2}EnumerationSummary: libs - {0}ms, callback - {1}ms", t1.TotalMilliseconds, t2.TotalMilliseconds, NameForLogging);
        }

        public void Enumerate(IEnumerable<Assembly> listAssemblySource)
        {
            var measure1 = new MeasureTime();
            EnumerateList(listAssemblySource);
            EnumerateScheduled();
            var t1 = measure1.Calculate();

            var measure2 = new MeasureTime();
            foreach (var pair in _assembliesToLoad)
            {
                try
                {
                    _callbackAfterLoad(pair.Value);
                }
                catch { }
            }
            var t2 = measure2.Calculate();

            if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.EnumerationSummary))
                Debug.WriteLine("{2}EnumerationSummarySource: libs - {0}ms, callback - {1}ms", t1.TotalMilliseconds, t2.TotalMilliseconds, NameForLogging);
        }

        private void EnumerateList(IEnumerable<Assembly> listAssemblySource, bool isInTask = false, int level = 0)
        {
            try
            {
                var manifestList = new List<string>();

                var listAssembly = listAssemblySource.Where(x => !x.ReflectionOnly);
                var listAssemblyNames = listAssembly.Select(x => x.GetName(false)).ToList();

                var tasks = new List<Task>();

                foreach (var assembly in listAssembly)
                {
                    try
                    {
                        var measure1 = new MeasureTime();
                        var measure1Time = measure1.Calculate().TotalMilliseconds;

                        if (!FilterDevelopmentRuntime(assembly.FullName, _globalAssemblyFilter)) continue;
                        if (assembly.IsDynamic) continue;

                        var name = assembly.GetName(false);
                        if (CheckPreparedAssemblyAndAdd(name)) continue;

                        var runSynchronized = _tasksAllowed ? _traceOnRun.Where(x => _comparer.Equals(x.GetName(false), name)).Count() > 0 : true;
                        Action<object> task = (state) =>
                        {
                            var _assembly = (Assembly)state;

                            PrepareCosturaReferences(_assembly, isInTask ? true : !runSynchronized, level);
                            EnumerateReferences(_assembly.GetReferencedAssemblies(), isInTask ? true : !runSynchronized, level + 1);

                            if (!FilterAssembly(_assembly)) return;

                            var _location = _assembly.IsDynamic ? _assembly.FullName : _assembly.Location;

                            if (_callbackBeforeLoad == null || _callbackBeforeLoad(_location)) _assembliesToLoad.TryAdd(name, _assembly);
                        };

                        measure1Time = measure1.Calculate().TotalMilliseconds;

                        if (!runSynchronized) tasks.Add(Task.Factory.StartNew(task, assembly, isInTask ? TaskCreationOptions.AttachedToParent : TaskCreationOptions.None));
                        else task(assembly);
                    }
                    catch (Exception ex) { ErrorHandled("EnumerateList.Load", ex); }
                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex) { ErrorHandled("EnumerateList", ex); }
        }

        private void EnumerateFolder()
        {
            try
            {
                //var location = Path.GetDirectoryName(LibraryEnumerator.LibraryDirectory);
                var location = LibraryEnumeratorFactory.LibraryDirectory;
                if (!location.EndsWith("bin") && Directory.Exists(Path.Combine(location, "bin"))) location = Path.Combine(location, "bin");

                var files = Directory.GetFiles(location, "*.dll", SearchOption.AllDirectories)
                                .Select(x => new { Path = x, Uri = new Uri(x), File = Path.GetFileName(x) })
                                .Where(x => !x.File.StartsWith("mscorlib") &&
                                            !x.File.StartsWith("System.") &&
                                            !x.File.StartsWith("System,") &&
                                            !x.File.StartsWith("DevExpress") &&
                                            !x.File.StartsWith("CrystalDecisions") &&
                                            !x.File.StartsWith("BusinessObjects") &&
                                            !x.File.StartsWith("stdole") &&
                                            !x.File.StartsWith("System,") &&
                                            !x.File.StartsWith("SNI.") &&
                                            !x.File.StartsWith("Microsoft."))
                                .Where(x => _preparedAssemblies.Where(y => x.Uri.AbsoluteUri.Equals(y.Key.CodeBase, StringComparison.CurrentCultureIgnoreCase)).Count() == 0)
                                .ToList();

                var listAssembliesReflection = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies().Select(x => x.IsDynamic ? x.FullName : x.CodeBase.ToLower()).ToList();
                var listAssemblies = AppDomain.CurrentDomain.GetAssemblies().Select(x => x.IsDynamic ? x.FullName : x.CodeBase.ToLower()).ToList();

                files = files
                            .Where(x => !listAssembliesReflection.Contains(x.Uri.AbsoluteUri.ToLower()))
                            .Where(x => !listAssemblies.Contains(x.Uri.AbsoluteUri.ToLower()))
                            .Where(x => !x.Uri.AbsoluteUri.ToLower().Contains(".resources.dll"))
                            .ToList();

                foreach (var uri in files)
                {
                    var dll = uri.Uri.LocalPath;
                    if (dll.Contains("Symlinks")) continue;
                    if (!FilterDevelopmentRuntime(Path.GetFileName(dll), _globalAssemblyFilter)) continue;

                    try
                    {
                        var reflectionAssembly = Assembly.ReflectionOnlyLoadFrom(dll);

                        var name = reflectionAssembly.GetName(false);
                        if (CheckPreparedAssemblyAndAdd(name)) continue;

                        if (!FilterAssembly(Path.GetFileName(dll))) continue;

                        if (Path.GetFileName(dll).ToLower().StartsWith("managedinjector")) continue;

                        var assembly = Assembly.LoadFrom(dll);
                        if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.LoadAssembly))
                            Debug.WriteLine("{2}LoadAssembly: '{0}' from '{1}'", assembly.FullName, dll, NameForLogging);

                        PrepareCosturaReferences(assembly);
                        EnumerateReferences(assembly.GetReferencedAssemblies());

                        if (_callbackBeforeLoad == null || _callbackBeforeLoad(dll)) _assembliesToLoad.TryAdd(name, assembly);
                    }
                    catch (System.IO.FileLoadException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        ErrorHandled($"EnumerateFolder.Load '{dll}'", ex);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(dll);
                        Debug.WriteLine(ex.ToString());
                        ErrorHandled($"EnumerateFolder.Load '{dll}'", ex);
                    }
                }
            }
            catch (Exception ex) { ErrorHandled("EnumerateFolder", ex); }
        }

        private void EnumerateScheduled()
        {
            if (_scheduledAssemblies.Count > 0)
            {
                var localScheduled = _scheduledAssemblies.Keys.ToList();
                _scheduledAssemblies.Clear();
                EnumerateReferences(localScheduled, false, 0);
            }

            if (_scheduledAssemblies.Count > 0) EnumerateScheduled();
        }

        private void EnumerateReferences(IEnumerable<AssemblyName> listAssemblyNamesSource, bool isInTask = false, int level = 0)
        {
            try
            {
                var tasks = new List<Task>();

                foreach (var assemblyName in listAssemblyNamesSource)
                {
                    if (CheckPreparedAssemblyAndAdd(assemblyName)) continue;

                    if (!FilterDevelopmentRuntime(assemblyName.FullName, _globalAssemblyFilter)) continue;

                    if (assemblyName.FullName.StartsWith("System.")) continue;
                    if (assemblyName.FullName.StartsWith("Newtonsoft.Json.Net20,")) continue;
                    if (assemblyName.FullName.StartsWith("BusinessObjects.Enterprise.Sdk")) continue;
                    if (assemblyName.FullName.ToLower().StartsWith("managedinjector")) continue;


                    var runSynchronized = _tasksAllowed ? _traceOnRun.Where(x => _comparer.Equals(x.GetName(false), assemblyName)).Count() > 0 : true;
                    var checkName = "TraceWeb";
                    Action<object> task = (state) =>
                    {
                        try
                        {
                            var assName = (AssemblyName)state;
                            if (assName.FullName.StartsWith(checkName))
                            { }

                            var assemblyLoaded = AppDomain.CurrentDomain.GetAssemblies().OrderBy(x => x.FullName);
                            var assemblyCompared = assemblyLoaded.Where(x => _comparer.Equals(x.GetName(), assName)).ToList();
                            var assembly = assemblyCompared.FirstOrDefault();
                            if (assembly == null)
                            {
                                assembly = Assembly.Load(assName);
                                if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.LoadAssembly))
                                    Debug.WriteLine("{1}LoadAssembly: '{0}'", assName, NameForLogging);
                            }
                            var _location = assembly.IsDynamic ? assembly.FullName : assembly.Location;

                            PrepareCosturaReferences(assembly, isInTask ? true : !runSynchronized, level);
                            EnumerateReferences(assembly.GetReferencedAssemblies(), isInTask ? true : !runSynchronized, level + 1);

                            if (!FilterAssembly(assName.FullName)) return;

                            if (_callbackBeforeLoad == null || _callbackBeforeLoad(_location)) _assembliesToLoad.TryAdd(assName, assembly);
                        }
                        catch (Exception ex) { ErrorHandled("EnumerateReferences.Enumerate", ex); }
                    };

                    if (assemblyName.FullName.StartsWith(checkName))
                    { }

                    if (!runSynchronized) tasks.Add(Task.Factory.StartNew(task, assemblyName, isInTask ? TaskCreationOptions.AttachedToParent : TaskCreationOptions.None));
                    else
                    {
                        if (isInTask)
                        {
                            //Если эту сборку надо обработать в основном потоке, но мы находимся в задаче, то получим deadlock.
                            DateTime d;
                            _scheduledAssemblies.TryAdd(assemblyName, DateTime.Now);
                            _preparedAssemblies.TryRemove(assemblyName, out d);
                        }
                        else task(assemblyName);
                    }
                }

                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception ex) { ErrorHandled("EnumerateReferences", ex); }
        }

        private void PrepareCosturaReferences(Assembly assembly, bool isInTask = false, int level = 0)
        {
            var _location = assembly.IsDynamic ? assembly.FullName : assembly.Location;

            if (assembly.IsDynamic) return;
            if (!FilterDevelopmentRuntime(assembly.FullName, _globalAssemblyFilter)) return;

            try
            {
                var type = assembly.GetType("Costura.AssemblyLoader");
                if (type != null)
                {
                    //lock (_costuraFodyLoadedSyncRoot)
                    {
                        if (_costuraFodyLoaded.Contains(assembly.FullName)) return;

                        var resolveAssemblyMethodOld = type.GetMethod("ResolveAssembly", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string) }, null);
                        var resolveAssemblyMethodNew = type.GetMethod("ResolveAssembly", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(object), typeof(ResolveEventArgs) }, null);
                        if (resolveAssemblyMethodOld != null || resolveAssemblyMethodNew != null)
                        {
                            //Попытка загрузить сборки, прикрепленные в качестве Embedded через Costura.Fody БЕЗ флага CreateTemporaryAssemblies=true.
                            var namesMethod = type.GetField("assemblyNames", BindingFlags.Static | BindingFlags.NonPublic);
                            if (namesMethod != null)
                            {
                                var names = (Dictionary<string, string>)namesMethod.GetValue(null);

                                var listEmbeddedAssemblies = new List<Assembly>();
                                foreach (var name in names.Keys)
                                {
                                    if (name.ToLower().StartsWith("managedinjector")) continue;

                                    var key_ = assembly.FullName + "_" + name;
                                    if (_preparedCosturaAssemblies.TryAdd(key_, DateTime.Now))
                                    {
                                        Assembly assemblyAnother = null;
                                        try
                                        {
                                            assemblyAnother = Assembly.Load(name);
                                        }
                                        catch
                                        {
                                            if (resolveAssemblyMethodOld != null) assemblyAnother = (Assembly)resolveAssemblyMethodOld.Invoke(null, new object[] { name });
                                            else if (resolveAssemblyMethodNew != null) assemblyAnother = (Assembly)resolveAssemblyMethodNew.Invoke(null, new object[] { null, new ResolveEventArgs(name) });
                                        }


                                        if (assemblyAnother != null) listEmbeddedAssemblies.Add(assemblyAnother);
                                    }
                                }
                                EnumerateList(listEmbeddedAssemblies, isInTask, level + 1);
                            }

                            //Попытка загрузить сборки, прикрепленные в качестве Embedded через Costura.Fody С флагом CreateTemporaryAssemblies=true.
                            var preloadListMethod = type.GetField("preloadList", BindingFlags.Static | BindingFlags.NonPublic);
                            if (preloadListMethod != null)
                            {
                                var resourceNameToPathMethod = type.GetMethod("ResourceNameToPath", BindingFlags.Static | BindingFlags.NonPublic);
                                if (resourceNameToPathMethod != null)
                                {
                                    var listEmbeddedAssemblies = new List<Assembly>();

                                    //var references = assembly.GetReferencedAssemblies().OrderBy(x => x.FullName).ToList();
                                    //foreach (var reference in references)
                                    //{
                                    //    Assembly assemblyAnother = null;
                                    //    if (resolveAssemblyMethodOld != null) assemblyAnother = (Assembly)resolveAssemblyMethodOld.Invoke(null, new object[] { reference.FullName });
                                    //    else if (resolveAssemblyMethodNew != null) assemblyAnother = (Assembly)resolveAssemblyMethodNew.Invoke(null, new object[] { null, new ResolveEventArgs(reference.FullName) });
                                    //    if (assemblyAnother != null) listEmbeddedAssemblies.Add(assemblyAnother);
                                    //}

                                    var tempBasePathMethod = type.GetField("tempBasePath", BindingFlags.Static | BindingFlags.NonPublic);
                                    if (tempBasePathMethod != null)
                                    {
                                        var preloadList = (List<string>)preloadListMethod.GetValue(null);
                                        var tempBasePath = (string)tempBasePathMethod.GetValue(null);

                                        foreach (var name in preloadList.ToList())
                                        {
                                            try
                                            {
                                                var key_ = assembly.FullName + "_" + name;
                                                if (_preparedCosturaAssemblies.TryAdd(key_, DateTime.Now))
                                                {
                                                    var name2 = (string)resourceNameToPathMethod.Invoke(null, new object[] { name });
                                                    if (name2 != null)
                                                    {
                                                        if (name2.EndsWith(".dll"))
                                                        {
                                                            var assemblyTempFilePath = Path.Combine(tempBasePath, name2);

                                                            if (assemblyTempFilePath.IndexOf("entityframework", StringComparison.InvariantCultureIgnoreCase) >= 0)
                                                            { }

                                                            if (Path.GetFileName(assemblyTempFilePath).ToLower().StartsWith("managedinjector")) continue;

                                                            //var assemblyAnother = (Assembly)resolveAssemblyMethod.Invoke(null, new object[] { name2 });
                                                            var assemblyAnother = Assembly.LoadFile(assemblyTempFilePath);
                                                            // if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.LoadAssembly))
                                                            //    Debug.WriteLine("{2}LoadAssembly: '{0}' from Costura '{1}'", assembly.FullName, assemblyTempFilePath, NameForLogging);
                                                            if (assemblyAnother != null) listEmbeddedAssemblies.Add(assemblyAnother);
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        EnumerateList(listEmbeddedAssemblies, isInTask, level + 1);
                                    }
                                }
                            }

                            var runtimeVersion = IntPtr.Size == 4 ? "32" : "64";
                            //Попытка загрузить unmanaged сборки, прикрепленные в качестве Embedded через Costura.Fody С флагом CreateTemporaryAssemblies=true.
                            preloadListMethod = type.GetField($"preload{runtimeVersion}List", BindingFlags.Static | BindingFlags.NonPublic);
                            if (preloadListMethod != null)
                            {
                                var resourceNameToPathMethod = type.GetMethod("ResourceNameToPath", BindingFlags.Static | BindingFlags.NonPublic);
                                if (resourceNameToPathMethod != null)
                                {
                                    var listEmbeddedAssemblies = new List<Assembly>();

                                    //var references = assembly.GetReferencedAssemblies().OrderBy(x => x.FullName).ToList();
                                    //foreach (var reference in references)
                                    //{
                                    //    Assembly assemblyAnother = null;
                                    //    if (resolveAssemblyMethodOld != null) assemblyAnother = (Assembly)resolveAssemblyMethodOld.Invoke(null, new object[] { reference.FullName });
                                    //    else if (resolveAssemblyMethodNew != null) assemblyAnother = (Assembly)resolveAssemblyMethodNew.Invoke(null, new object[] { null, new ResolveEventArgs(reference.FullName) });
                                    //    if (assemblyAnother != null) listEmbeddedAssemblies.Add(assemblyAnother);
                                    //}

                                    var tempBasePathMethod = type.GetField("tempBasePath", BindingFlags.Static | BindingFlags.NonPublic);
                                    if (tempBasePathMethod != null)
                                    {
                                        var preloadList = (List<string>)preloadListMethod.GetValue(null);
                                        var tempBasePath = (string)tempBasePathMethod.GetValue(null);

                                        foreach (var name in preloadList.ToList())
                                        {
                                            try
                                            {
                                                var key_ = assembly.FullName + "_" + name;
                                                if (_preparedCosturaAssemblies.TryAdd(key_, DateTime.Now))
                                                {
                                                    var name2 = (string)resourceNameToPathMethod.Invoke(null, new object[] { name });
                                                    if (name2 != null)
                                                    {
                                                        if (name2.EndsWith(".dll"))
                                                        {
                                                            var assemblyTempFilePath = Path.Combine(tempBasePath, name2);

                                                            if (assemblyTempFilePath.IndexOf("entityframework", StringComparison.InvariantCultureIgnoreCase) >= 0)
                                                            { }

                                                            if (Path.GetFileName(assemblyTempFilePath).ToLower().StartsWith("managedinjector")) continue;

                                                            //var assemblyAnother = (Assembly)resolveAssemblyMethod.Invoke(null, new object[] { name2 });
                                                            var assemblyAnother = Assembly.LoadFile(assemblyTempFilePath);
                                                            // if (_loggingOptions.HasFlag(LibraryEnumeratorFactory.eLoggingOptions.LoadAssembly))
                                                            //    Debug.WriteLine("{2}LoadAssembly: '{0}' from Costura '{1}'", assembly.FullName, assemblyTempFilePath, NameForLogging);
                                                            if (assemblyAnother != null) listEmbeddedAssemblies.Add(assemblyAnother);
                                                        }
                                                    }
                                                }
                                            }
                                            catch { }
                                        }
                                        EnumerateList(listEmbeddedAssemblies, isInTask, level + 1);
                                    }
                                }
                            }

                        }

                        _costuraFodyLoaded.Add(assembly.FullName);
                    }
                }
            }
            catch (FileLoadException)
            {
                //if (Version.Parse(assembly.ImageRuntimeVersion) < Version.Parse("4.0"))
                //{

                //}
            }
        }

        private void ErrorHandled(string section, Exception ex)
        {
            var tt = ex.GetType();

            if (ex is ReflectionTypeLoadException)
            {
                var ex2 = ex as ReflectionTypeLoadException;
                Debug.WriteLine($"{NameForLogging}Error1 {section}: \r\n" + string.Join("\r\n", ex2.LoaderExceptions.Select(x => x.Message)).Trim());
            }
            else if (ex is FileLoadException)
            {
                Debug.WriteLine($"{NameForLogging}Error2 {section}: {ex.Message}".Trim());
            }
            else if (ex is FileNotFoundException)
            {
                Debug.WriteLine($"{NameForLogging}Error3 {section}: {ex.Message}".Trim());
                Debug.WriteLine($"{NameForLogging}Error4 {ex.ToString()}".Trim());
            }
            else Debug.WriteLine($"{NameForLogging}Error {section}: {ex.Message}");
        }

        private bool FilterAssembly(Assembly assembly)
        {
            return FilterAssembly(assembly.FullName);
        }

        private bool FilterAssembly(string assemblyName)
        {
            if (_enumerateAttrs.HasFlag(LibraryEnumeratorFactory.EnumerateAttrs.ExcludeSystem))
            {
                if (assemblyName.StartsWith("System.") ||
                    assemblyName.StartsWith("System,") ||
                    assemblyName.StartsWith("stdole") ||
                    assemblyName.StartsWith("vshost") ||
                    assemblyName.StartsWith("mscorlib")) return false;
            }

            if (_enumerateAttrs.HasFlag(LibraryEnumeratorFactory.EnumerateAttrs.ExcludeMicrosoft))
            {
                if (assemblyName.StartsWith("Microsoft.") || assemblyName.StartsWith("Microsoft,")) return false;
            }

            if (_enumerateAttrs.HasFlag(LibraryEnumeratorFactory.EnumerateAttrs.ExcludeKnownExternal))
            {
                if (assemblyName.StartsWith("CrystalDecisions.") ||
                    assemblyName.StartsWith("BusinessObjects.Enterprise") ||
                    assemblyName.StartsWith("nunit.framework, ")
                    ) return false;
            }

            if (_globalAssemblyFilter != null && !_globalAssemblyFilter(assemblyName)) return false;

            return true;
        }

        public static bool FilterDevelopmentRuntime(string assemblyName, Func<string, bool> globalAssemblyFilter)
        {
            if (assemblyName.StartsWith("Microsoft.", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyName.StartsWith("ApexSQL", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyName.StartsWith("mscorlib", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyName.StartsWith("vshost", StringComparison.InvariantCultureIgnoreCase) ||
                assemblyName.StartsWith("Microsoft.VsHub", StringComparison.InvariantCultureIgnoreCase)
                ) return false;

            if (globalAssemblyFilter != null && !globalAssemblyFilter(assemblyName)) return false;

            return true;
        }

        /// <summary>
        /// Возвращает true, если эта сборка уже была обработана. Для сборок с публичным ключом обрабатывает только одну версию сборки.
        /// </summary>
        private bool CheckPreparedAssemblyAndAdd(AssemblyName assemblyName)
        {
            if (assemblyName == null) return true;
            if (!_preparedAssemblies.TryAdd(assemblyName, DateTime.Now)) return true;

            var bytes = assemblyName.GetPublicKeyToken();
            if (bytes != null && bytes.Length > 0)
            {
                var publicKeyString = assemblyName.FullName.Substring(assemblyName.FullName.LastIndexOf(", PublicKeyToken="));
                var assemblies = _preparedAssemblies.Keys.Where(x => x.FullName.EndsWith(publicKeyString) && x.Name == assemblyName.Name && x.Version != assemblyName.Version).ToList();

                if (assemblies.Count > 0) return true;
            }
            

            return false;
        }

        public IEnumerable<AssemblyName> NamesList
        {
            get { return _preparedAssemblies.Keys.OrderBy(x => x.ToString()); }
        }

        private string NameForLogging
        {
            get { return (_nameForLogging == null ? _enumerationGuid.ToString() : _nameForLogging) + "."; }
        }

    }
}