using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Architecture.AppCore
{
    using DI;

    /// <summary>
    /// Ядро модульной архитектуры.
    /// По сути ядро похоже на DI-контейнер, но с некоторыми ограничениями. Подразумевается использование двух видов компонентов - transient-компонент и singleton-компонент.
    /// </summary>
    public abstract partial class AppCore<TAppCore> : IDisposable
        where TAppCore : AppCore<TAppCore>
    {
        class InstanceActivatingHandlerImpl : IInstanceActivatingHandler
        {
            private readonly TAppCore _core;

            public InstanceActivatingHandlerImpl(TAppCore core)
            {
                _core = core;
            }

            public void OnInstanceActivating<TRequestedType>(object instance)
            {
                if (_core.AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(InstanceActivatingHandlerImpl)}.{nameof(OnInstanceActivating)}: активация компонента '{instance.GetType().FullName}'.");

                if (instance is IComponent<TAppCore> coreComponent)
                {
                    if (!coreComponent.GetState().In(CoreComponentState.Started, CoreComponentState.Stopped))
                    {
                        if (_core.AppDebugLevel >= DebugLevel.Detailed)
                            Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(InstanceActivatingHandlerImpl)}.{nameof(OnInstanceActivating)}: компонент не запущен, состояние - {coreComponent.GetState()}");

                        if (coreComponent is IComponentStartable<TAppCore> coreComponentStartable)
                        {
                            if (_core.AppDebugLevel >= DebugLevel.Detailed)
                            {
                                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(InstanceActivatingHandlerImpl)}.{nameof(OnInstanceActivating)}: попытка запуска компонента.");
                                try
                                {
                                    coreComponentStartable.Start(_core);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(InstanceActivatingHandlerImpl)}.{nameof(OnInstanceActivating)}: ошибка запуска компонента");
                                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(InstanceActivatingHandlerImpl)}.{nameof(OnInstanceActivating)}: {ex}");
                                    throw;
                                }
                            }
                            else
                            {
                                coreComponentStartable.Start(_core);
                            }
                        }
                    }

                    if (_core.GetState() == CoreComponentState.Starting && coreComponent is Listeners.IAppCoreStartListener listenerComponent) _core._instancesActivatedDuringStartup.Add(listenerComponent);
                }

                if (instance is IComponentSingleton<TAppCore> coreComponentSingleton)
                {
                    _core._activatedSingletonInstances.Push(coreComponentSingleton);
                }
            }
        }

        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly TAppCore _core;

            public InstanceActivatedHandlerImpl(TAppCore core)
            {
                _core = core;
            }

            public void OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is CoreComponentBase<TAppCore> coreComponentBase) coreComponentBase.OnStarted();
            }
        }

        class BindingsResolverInternalImpl : BindingsResolverInternal
        {
            private static MethodInfo _methodResolveTypeSingleton = null;
            private static MethodInfo _methodResolveTypeTransient = null;

            static BindingsResolverInternalImpl()
            {
                _methodResolveTypeSingleton = typeof(BindingsResolverInternalImpl).GetMethod(nameof(ResolveTypeSingleton), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                _methodResolveTypeTransient = typeof(BindingsResolverInternalImpl).GetMethod(nameof(ResolveTypeTransient), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (_methodResolveTypeSingleton == null || _methodResolveTypeTransient == null) throw new InvalidProgramException();
            }

            public BindingsResolverInternalImpl()
            {
                BindingsCollectionFromLazy = new BindingsCollection<TAppCore>();
            }

            public override BindingDescription ResolveType<TRequestedType>(bool isSingleton)
            {
                return (BindingDescription)(isSingleton ?
                    _methodResolveTypeSingleton.MakeGenericMethod(typeof(TRequestedType)).Invoke(this, new object[] { }) :
                    _methodResolveTypeTransient.MakeGenericMethod(typeof(TRequestedType)).Invoke(this, new object[] { }));
            }

            public BindingDescription ResolveTypeSingleton<TRequstedType>() where TRequstedType : IComponentSingleton<TAppCore>
            {
                var typeRequested = typeof(TRequstedType);
                if (BindingsCollectionFromLazy._typesCollection.TryGetValue(typeRequested, out var valueFromLazy))
                {
                    return valueFromLazy.Item1;
                }
                else
                {
                    var bindingsCollection = new BindingsCollection<TAppCore>();
                    BindingsResolverFromProtected?.OnSingletonBindingResolve<TRequstedType>(bindingsCollection);
                    if (bindingsCollection._typesCollection.TryGetValue(typeof(TRequstedType), out var valueFromProtected))
                    {
                        return valueFromProtected.Item1;
                    }
                    else
                    {
                        BindingsResolverFromExternal?.OnSingletonBindingResolve<TRequstedType>(bindingsCollection);
                        return bindingsCollection._typesCollection.TryGetValue(typeof(TRequstedType), out var valueFromExternal) ? valueFromExternal?.Item1 : null;
                    }
                }
            }

            public BindingDescription ResolveTypeTransient<TRequstedType>() where TRequstedType : IComponentTransient<TAppCore>
            {
                var typeRequested = typeof(TRequstedType);
                if (BindingsCollectionFromLazy._typesCollection.TryGetValue(typeRequested, out var valueFromLazy))
                {
                    return valueFromLazy.Item1;
                }
                else
                {
                    var bindingsCollection = new BindingsCollection<TAppCore>();
                    BindingsResolverFromProtected?.OnTransientBindingResolve<TRequstedType>(bindingsCollection);
                    if (bindingsCollection._typesCollection.TryGetValue(typeof(TRequstedType), out var valueFromProtected))
                    {
                        return valueFromProtected.Item1;
                    }
                    else
                    {
                        BindingsResolverFromExternal?.OnTransientBindingResolve<TRequstedType>(bindingsCollection);
                        return bindingsCollection._typesCollection.TryGetValue(typeof(TRequstedType), out var valueFromExternal) ? valueFromExternal?.Item1 : null;
                    }
                }
            }

            public BindingsCollection<TAppCore> BindingsCollectionFromLazy { get; set; }

            public IBindingsResolver<TAppCore> BindingsResolverFromProtected { get; set; }

            public IBindingsResolver<TAppCore> BindingsResolverFromExternal { get; set; }
        }

        private bool _starting = false;
        private bool _started = false;
        private bool _stopped = false;
        private bool _bindingsPreparing = false;
        private ConcurrentStack<IComponentSingleton<TAppCore>> _activatedSingletonInstances = null;

        private readonly InstanceActivatingHandlerImpl _instanceActivatingHandler = null;
        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;

        private BindingsObjectProvider _objectProvider = new BindingsObjectProvider(new List<KeyValuePair<Type, BindingDescription>>());
        private List<Listeners.IAppCoreStartListener> _instancesActivatedDuringStartup = new List<Listeners.IAppCoreStartListener>();
        private BindingsResolverInternalImpl _bindingsResolver = null;

        /// <summary>
        /// Создает новый объект <see cref="AppCore{TAppCore}"/>. 
        /// </summary>
        protected AppCore()
        {
            if (!typeof(TAppCore).IsAssignableFrom(GetType()))
            {
                if (AppDebugLevel>= DebugLevel.Disabled) Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.new: Параметр-тип {nameof(TAppCore)} должен находиться в цепочке наследования текущего типа.");
                throw new TypeAccessException($"Параметр-тип {nameof(TAppCore)} должен находиться в цепочке наследования текущего типа.");
            }
            _instanceActivatingHandler = new InstanceActivatingHandlerImpl((TAppCore)(object)this);
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl((TAppCore)(object)this);
            _activatedSingletonInstances = new ConcurrentStack<IComponentSingleton<TAppCore>>();
        }

        /// <summary>
        /// Старт ядра.
        /// </summary>
        public void Start()
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: запуск");

            var startStep = ApplicationStartStep.PrepareAssemblyStandardList;

            try
            {
                _starting = true;

                var assemblyStartupList = GetAssemblyStartupList();
                if (AppDebugLevel >= DebugLevel.Detailed)
                {
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: список типов для создания привязок типов и выполнения действий при запуске:");
                    assemblyStartupList.ForEach(x => Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: {x.ObjectInstance.GetType()} (привязка типов - {(x.ConfigureBindings != null ? "да" : "нет")}, действия при запуске - {(x.ExecuteStart != null ? "да" : "нет")})"));
                }

                var assemblyLoadedDuringStartup = new List<Assembly>();
                var assemblyLoadedDuringStartupHandler = new AssemblyLoadEventHandler((e, args) =>
                {
                    if (!args.LoadedAssembly.ReflectionOnly)
                        assemblyLoadedDuringStartup.Add(args.LoadedAssembly);
                });
                AppDomain.CurrentDomain.AssemblyLoad += assemblyLoadedDuringStartupHandler;

                try
                {
                    startStep = ApplicationStartStep.BindingsRequired;

                    _bindingsPreparing = true;

                    var bindingsCollection = new BindingsCollection<TAppCore>();
                    if (AppDebugLevel >= DebugLevel.Common)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: выполнение {nameof(IConfigureBindings<TAppCore>)}.{nameof(IConfigureBindings<TAppCore>.ConfigureBindings)}");
                    assemblyStartupList.Where(x => x.ConfigureBindings != null).ForEach(x => x.ConfigureBindings.Invoke(x.ObjectInstance, new object[] { bindingsCollection }));
                    if (AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(OnBindingsRequired)}");
                    OnBindingsRequired(bindingsCollection);

                    startStep = ApplicationStartStep.BindingsApplying;

                    BindingsApply(bindingsCollection);
                    ((IBindingsObjectProvider)_objectProvider).RegisterInstanceActivatingHandler(_instanceActivatingHandler);
                    ((IBindingsObjectProvider)_objectProvider).RegisterInstanceActivatedHandler(_instanceActivatedHandler);
                    if (AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(OnBindingsApplied)}");
                    OnBindingsApplied();
                }
                finally
                {
                    _bindingsPreparing = false;
                }

                startStep = ApplicationStartStep.BindingsAutoStart;

                BindingsAutoStart(_objectProvider.GetQueryTypes());
                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(OnBindingsAutoStart)}");
                OnBindingsAutoStart();

                startStep = ApplicationStartStep.Start;

                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: выполнение {nameof(IExecuteStart<TAppCore>)}.{nameof(IExecuteStart<TAppCore>.ExecuteStart)}");
                assemblyStartupList.Where(x => x.ExecuteStart != null).ForEach(x => x.ExecuteStart.Invoke(x.ObjectInstance, new object[] { this }));
                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(OnStart)}");
                OnStart();

                _instancesActivatedDuringStartup.ForEach(x => x.OnAppCoreStarted());
                _instancesActivatedDuringStartup.Clear();
                _instancesActivatedDuringStartup = null;

                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: ленивая загрузка для сборок, загруженных дополнительно во время запуска приложения");

                AppDomain.CurrentDomain.AssemblyLoad += (e, args) => LazyAssemblyLoad(args.LoadedAssembly);
                AppDomain.CurrentDomain.AssemblyLoad -= assemblyLoadedDuringStartupHandler;
                assemblyLoadedDuringStartup.ForEach(x => LazyAssemblyLoad(x));

                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: успешный запуск");

                _starting = false;
                _started = true;

            }
            catch (ApplicationStartException)
            {
                _starting = false;
                _started = false;
                throw;
            }
            catch (Exception ex)
            {
                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: ошибка запуска.");
                if (AppDebugLevel >= DebugLevel.Detailed)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Start)}: {ex}");

                _starting = false;
                _started = false;
                throw new ApplicationStartException(startStep, Types.TypeHelpers.ExtractGenericType(GetType(), typeof(AppCore<TAppCore>)), ex);
            }
        }

        private void LazyAssemblyLoad(Assembly assembly)
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): ленивая загрузка");

            var assemblyStartupList = GetAssemblyStartupListLazy(assembly);
            if (AppDebugLevel >= DebugLevel.Detailed)
            {
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): список типов для создания привязок типов и выполнения действий при запуске:");
                assemblyStartupList.ForEach(x => Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): {x.Item1.GetType()} (привязка типов - {(x.Item2 != null ? "да" : "нет")}, действия при запуске - {(x.Item2 != null ? "да" : "нет")})"));
            }

            var bindingsCollection = new BindingsCollection<TAppCore>();
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): выполнение {nameof(IConfigureBindingsLazy<TAppCore>)}.{nameof(IConfigureBindingsLazy<TAppCore>.ConfigureBindingsLazy)}");
            assemblyStartupList.Where(x => x.Item2 != null).ForEach(x => x.Item2.Invoke(x.Item1, new object[] { bindingsCollection }));

            var queryTypesBound = new List<Type>();
            foreach (var pair in bindingsCollection._typesCollection)
            {
                if (_objectProvider.TryAppendBinding(pair.Key, pair.Value.Item1))
                {
                    queryTypesBound.Add(pair.Key);
                }
                else
                {
                    if (AppDebugLevel >= DebugLevel.Common)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): привязка для '{pair.Key}' уже существует.");
                }
            }

            BindingsAutoStart(queryTypesBound);

            if (AppDebugLevel >= DebugLevel.Detailed)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(LazyAssemblyLoad)}('{assembly.FullName}'): выполнение {nameof(IExecuteStartLazy<TAppCore>)}.{nameof(IExecuteStartLazy<TAppCore>.ExecuteStartLazy)}");
            assemblyStartupList.Where(x => x.Item3 != null).ForEach(x => x.Item3.Invoke(x.Item1, new object[] { this }));
        }

        /// <summary>
        /// Остановка ядра, остановка всех компонентов ядра.
        /// </summary>
        public void Stop()
        {
            if (_stopped) return;
            if (!_started)
            {
                if (AppDebugLevel > DebugLevel.Disabled) Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Stop)}: Ядро не запущено. Вызовите Start.");
                throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            }

            while (_activatedSingletonInstances.TryPop(out var componentSingleton))
            {
                try
                {
                    componentSingleton.Stop();
                }
                catch (Exception ex)
                {
                    if (AppDebugLevel >= DebugLevel.Common)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Stop)}: ошибка во время остановки компонента '{componentSingleton.GetType().FullName}'.");
                    if (AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(Stop)}: {ex}");
                }
            }

            try
            {
                OnStop();
            }
            finally
            {
                _stopped = true;
            }
        }

        /// <summary>
        /// Возвращает состояние ядра.
        /// </summary>
        public CoreComponentState GetState()
        {
            return _started ? CoreComponentState.Started : (_starting ? CoreComponentState.Starting : (_stopped ? CoreComponentState.Stopped : CoreComponentState.None));
        }

        #region Привязка типов
        private void BindingsApply(BindingsCollection<TAppCore> collection)
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsApply)}");

            if (!_bindingsPreparing)
            {
                if (AppDebugLevel >= DebugLevel.Common)
                    Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsApply)}: невозможно устанавливать привязки после запуска ядра.");

                throw new InvalidOperationException("Невозможно устанавливать привязки после запуска ядра.");
            }
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            _objectProvider = new BindingsObjectProvider(
                collection._typesCollection.
                    OrderBy(x => x.Value.Item2).
                    Select(x => new KeyValuePair<Type, BindingDescription>(x.Key, x.Value.Item1)).
                    ToList()
            );

            var bindingsResolver = new BindingsResolverInternalImpl() { BindingsResolverFromProtected = GetBindingsResolver(), BindingsResolverFromExternal = _bindingsResolver?.BindingsResolverFromExternal };
            _objectProvider.RegisterBindingsResolver(bindingsResolver);
            _bindingsResolver = bindingsResolver;
        }

        private void BindingsAutoStart(IEnumerable<Type> queryTypes)
        {
            if (AppDebugLevel >= DebugLevel.Common)
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}");

            var typesForAutoStart = queryTypes.
                Where(type => typeof(IAutoStart).IsAssignableFrom(type) && typeof(IComponentSingleton<TAppCore>).IsAssignableFrom(type)).
                ToList();

            if (AppDebugLevel >= DebugLevel.Detailed)
            {
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: несортированный список:");
                typesForAutoStart.ForEach(x => Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: {x}"));
            }

            var thisType = GetType();
            while (thisType != typeof(object))
            {
                var thisTypeAssembly = thisType.Assembly;

                var thisTypeAssemblyAutoStart = typesForAutoStart.Where(x => x.Assembly == thisTypeAssembly).ToList();
                thisTypeAssemblyAutoStart.OrderByDescending(x => x.FullName).ForEach(x =>
                {
                    typesForAutoStart.Remove(x);
                    typesForAutoStart.Insert(0, x);
                });

                thisType = thisType.BaseType;
            }

            if (AppDebugLevel >= DebugLevel.Detailed)
            {
                Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: сортированный список:");
                typesForAutoStart.ForEach(x => Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: {x}"));
            }

            foreach (var type in typesForAutoStart)
            {
                try
                {
                    if (AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: автозапуск типа '{type}'.");

                    var instance = Get<IComponentSingleton<TAppCore>>(type);
                }
                catch (Exception ex)
                {
                    if (AppDebugLevel >= DebugLevel.Common)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: ошибка автозапуска");
                    if (AppDebugLevel >= DebugLevel.Detailed)
                        Debug.WriteLine($"{nameof(AppCore<TAppCore>)}.{nameof(BindingsAutoStart)}: {ex}");

                    if (typeof(ICritical).IsAssignableFrom(type)) throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, type, ex);
                }
            }
        }

        private List<Internal.AppCoreStartupInfo> GetAssemblyStartupList()
        {
            var currentType = GetType();

            var assemblyPublicKeyTokenIgnored = new string[] { "b03f5f7f11d50a3a", "31bf3856ad364e35", "b77a5c561934e089", "71e9bce111e9429c" };

            var unsortedDictionary = new Dictionary<Assembly, List<Internal.AppCoreStartupInfo>>();
            var assembliesLoaded = new List<Assembly>();
            assembliesLoaded.AddRange(AppDomain.CurrentDomain.GetAssemblies());
            AppDomain.CurrentDomain.AssemblyLoad += (s, args) => assembliesLoaded.Add(args.LoadedAssembly);

            while (assembliesLoaded.Count > 0)
            {
                var assembliesLoadedCopy = assembliesLoaded.ToList();
                assembliesLoaded.Clear();

                var instances2 = assembliesLoadedCopy.
                    Where(assembly => !FilterAssemblyOnStartup(assembly) || !assemblyPublicKeyTokenIgnored.Contains(string.Join("", assembly.GetName().GetPublicKeyToken().Select(b => b.ToString("x2"))))).
                    Select(assembly =>
                    {
                        var assemblyStartupTypes = assembly.
                            GetTypes().
                            Where(type =>
                            {
                                if (!type.IsClass || type.IsAbstract) return false;

                                if (type.IsGenericType && type.IsGenericTypeDefinition)
                                {
                                    var d = currentType;
                                    if (type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == typeof(IConfigureBindings<>) || interfaceType.GetGenericTypeDefinition() == typeof(IExecuteStart<>))))
                                    {
                                        var arguments = type.GetGenericArguments();
                                        if (arguments.Length == 1 && arguments[0].IsGenericParameter)
                                        {
                                            var constraints = arguments[0].GetGenericParameterConstraints();
                                            if (constraints.Length == 1 && constraints[0].IsGenericType)
                                            {
                                                var inheritedType = Types.TypeHelpers.ExtractGenericType(currentType, constraints[0].GetGenericTypeDefinition());
                                                if (inheritedType != null) return true;
                                            }
                                        }
                                    }
                                    return false;
                                }

                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindings<int>))))
                                {
                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindings<>));
                                    if (interfaceTypes.Any(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))) return true;
                                }
                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                                {
                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStart<>));
                                    if (interfaceTypes.Any(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))) return true;
                                }
                                return false;
                            }).
                            Where(x => x.GetConstructor(new Type[] { }) != null).
                            Select(type =>
                            {
                                if (type.IsGenericType && type.IsGenericTypeDefinition)
                                {
                                    var d = currentType;
                                    if (type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == typeof(IConfigureBindings<>) || interfaceType.GetGenericTypeDefinition() == typeof(IExecuteStart<>))))
                                    {
                                        var arguments = type.GetGenericArguments();
                                        if (arguments.Length == 1 && arguments[0].IsGenericParameter)
                                        {
                                            var constraints = arguments[0].GetGenericParameterConstraints();
                                            if (constraints.Length == 1 && constraints[0].IsGenericType)
                                            {
                                                var inheritedType = Types.TypeHelpers.ExtractGenericType(currentType, constraints[0].GetGenericTypeDefinition());
                                                if (inheritedType != null)
                                                {
                                                    type = type.MakeGenericType(inheritedType.GetGenericArguments()[0]);

                                                    MethodInfo methodConfigureBindings2 = null;
                                                    if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindings<int>))))
                                                    {
                                                        var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindings<>));
                                                        var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                                        if (interfaceType2 != null)
                                                        {
                                                            methodConfigureBindings2 = interfaceType2.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings));
                                                        }
                                                    }

                                                    MethodInfo methodExecuteStart2 = null;
                                                    if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                                                    {
                                                        var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStart<>));
                                                        var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                                        if (interfaceType2 != null)
                                                        {
                                                            methodExecuteStart2 = interfaceType2.GetMethod(nameof(IExecuteStart<object>.ExecuteStart));
                                                        }
                                                    }

                                                    return new
                                                    {
                                                        Type = type,
                                                        ConfigureBindings = methodConfigureBindings2,
                                                        ExecuteStart = methodExecuteStart2
                                                    };

                                                //return new
                                                //{
                                                //    Type = type,
                                                //    ConfigureBindingsMethod = extractedInterfaceType.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings)),
                                                //    ExecuteStartMethod = extractedInterfaceType.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings)),
                                                //};
                                            }
                                            }
                                        }
                                    }
                                }

                                MethodInfo methodConfigureBindings = null;
                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindings<int>))))
                                {
                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindings<>));
                                    var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                    if (interfaceType2 != null)
                                    {
                                        methodConfigureBindings = interfaceType2.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings));
                                    }
                                }

                                MethodInfo methodExecuteStart = null;
                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                                {
                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStart<>));
                                    var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                    if (interfaceType2 != null)
                                    {
                                        methodExecuteStart = interfaceType2.GetMethod(nameof(IExecuteStart<object>.ExecuteStart));
                                    }
                                }

                                return new
                                {
                                    Type = type,
                                    ConfigureBindings = methodConfigureBindings,
                                    ExecuteStart = methodExecuteStart
                                };
                            }).
                            Where(x => x.ConfigureBindings != null || x.ExecuteStart != null).
                            OrderBy(x => x.Type.FullName).
                            ToList();

                        return new
                        {
                            assembly,
                            Instances = assemblyStartupTypes.
                                Select(x => new Internal.AppCoreStartupInfo
                                {
                                    ObjectInstance = Activator.CreateInstance(x.Type),
                                    ConfigureBindings = x.ConfigureBindings,
                                    ExecuteStart = x.ExecuteStart
                                }).
                                ToList()
                        };
                    }).
                    Where(x => x.Instances.Count > 0).
                    ToDictionary(x => x.assembly, x => x.Instances);
                instances2.ForEach(x => unsortedDictionary.Add(x.Key, x.Value));
            }

            var assembliesSorted = unsortedDictionary.Keys.ToList();
            for (int j = 0; j < assembliesSorted.Count - 1; j++)
            {
                var ass = assembliesSorted[j];
                var assReferenced = ass.GetReferencedAssemblies();
                var isMove = false;
                foreach (var assRef in assReferenced)
                {
                    for (int k = j + 1; k < assembliesSorted.Count; k++)
                    {
                        if (assembliesSorted[k].GetName().FullName == assRef.FullName)
                        {
                            isMove = true;
                            break;
                        }
                    }
                    if (isMove) break;
                }

                if (isMove)
                {
                    assembliesSorted.RemoveAt(j);
                    assembliesSorted.Add(ass);
                    j = j - 1;
                }
            }

            return assembliesSorted.SelectMany(x => unsortedDictionary[x]).ToList();
        }

        private List<Tuple<object, MethodInfo, MethodInfo>> GetAssemblyStartupListLazy(Assembly assemblyLazy)
        {
            var currentType = GetType();

            var assemblyPublicKeyTokenIgnored = new string[] { "b03f5f7f11d50a3a", "31bf3856ad364e35", "b77a5c561934e089", "71e9bce111e9429c" };

            var instances2 = assemblyLazy.ToEnumerable().
                Where(assembly => !assemblyPublicKeyTokenIgnored.Contains(string.Join("", assembly.GetName().GetPublicKeyToken().Select(b => b.ToString("x2"))))).
                Select(assembly =>
                {
                    var assemblyStartupTypes = assembly.
                        GetTypes().
                        Where(type =>
                        {
                            if (!type.IsClass || type.IsAbstract) return false;

                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                            {
                                var d = currentType;
                                if (type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == typeof(IConfigureBindingsLazy<>) || interfaceType.GetGenericTypeDefinition() == typeof(IExecuteStartLazy<>))))
                                {
                                    var arguments = type.GetGenericArguments();
                                    if (arguments.Length == 1 && arguments[0].IsGenericParameter)
                                    {
                                        var constraints = arguments[0].GetGenericParameterConstraints();
                                        if (constraints.Length == 1 && constraints[0].IsGenericType)
                                        {
                                            var inheritedType = Types.TypeHelpers.ExtractGenericType(currentType, constraints[0].GetGenericTypeDefinition());
                                            if (inheritedType != null) return true;
                                        }
                                    }
                                }
                                return false;
                            }

                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindingsLazy<int>))))
                            {
                                var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindingsLazy<>));
                                if (interfaceTypes.Any(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))) return true;
                            }
                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                            {
                                var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStartLazy<>));
                                if (interfaceTypes.Any(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))) return true;
                            }
                            return false;
                        }).
                        Where(x => x.GetConstructor(new Type[] { }) != null).
                        Select(type =>
                        {
                            if (type.IsGenericType && type.IsGenericTypeDefinition)
                            {
                                var d = currentType;
                                if (type.GetInterfaces().Any(interfaceType => interfaceType.IsGenericType && (interfaceType.GetGenericTypeDefinition() == typeof(IConfigureBindingsLazy<>) || interfaceType.GetGenericTypeDefinition() == typeof(IExecuteStartLazy<>))))
                                {
                                    var arguments = type.GetGenericArguments();
                                    if (arguments.Length == 1 && arguments[0].IsGenericParameter)
                                    {
                                        var constraints = arguments[0].GetGenericParameterConstraints();
                                        if (constraints.Length == 1 && constraints[0].IsGenericType)
                                        {
                                            var inheritedType = Types.TypeHelpers.ExtractGenericType(currentType, constraints[0].GetGenericTypeDefinition());
                                            if (inheritedType != null)
                                            {
                                                type = type.MakeGenericType(inheritedType.GetGenericArguments()[0]);

                                                MethodInfo methodConfigureBindings2 = null;
                                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindingsLazy<int>))))
                                                {
                                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindingsLazy<>));
                                                    var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                                    if (interfaceType2 != null)
                                                    {
                                                        methodConfigureBindings2 = interfaceType2.GetMethod(nameof(IConfigureBindingsLazy<object>.ConfigureBindingsLazy));
                                                    }
                                                }

                                                MethodInfo methodExecuteStart2 = null;
                                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStartLazy<int>))))
                                                {
                                                    var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStartLazy<>));
                                                    var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                                    if (interfaceType2 != null)
                                                    {
                                                        methodExecuteStart2 = interfaceType2.GetMethod(nameof(IExecuteStartLazy<object>.ExecuteStartLazy));
                                                    }
                                                }

                                                return new
                                                {
                                                    Type = type,
                                                    ConfigureBindingsLazy = methodConfigureBindings2,
                                                    ExecuteStartLazy = methodExecuteStart2
                                                };
                                            }
                                        }
                                    }
                                }
                            }

                            MethodInfo methodConfigureBindings = null;
                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IConfigureBindingsLazy<int>))))
                            {
                                var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IConfigureBindingsLazy<>));
                                var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                if (interfaceType2 != null)
                                {
                                    methodConfigureBindings = interfaceType2.GetMethod(nameof(IConfigureBindingsLazy<object>.ConfigureBindingsLazy));
                                }
                            }

                            MethodInfo methodExecuteStart = null;
                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStartLazy<int>))))
                            {
                                var interfaceTypes = Types.TypeHelpers.ExtractGenericInterfaces(type, typeof(IExecuteStartLazy<>));
                                var interfaceType2 = interfaceTypes.FirstOrDefault(interfaceType => typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]));
                                if (interfaceType2 != null)
                                {
                                    methodExecuteStart = interfaceType2.GetMethod(nameof(IExecuteStartLazy<object>.ExecuteStartLazy));
                                }
                            }

                            return new
                            {
                                Type = type,
                                ConfigureBindingsLazy = methodConfigureBindings,
                                ExecuteStartLazy = methodExecuteStart
                            };
                        }).
                        Where(x => x.ConfigureBindingsLazy != null || x.ExecuteStartLazy != null).
                        OrderBy(x => x.Type.FullName).
                        ToList();

                    return new
                    {
                        assembly,
                        Instances = assemblyStartupTypes.
                            Select(x => new
                            {
                                Instance = Activator.CreateInstance(x.Type),
                                x.ConfigureBindingsLazy,
                                x.ExecuteStartLazy
                            }).
                            ToList()
                    };
                }).
                Where(x => x.Instances.Count > 0).
                ToDictionary(x => x.assembly, x => x.Instances);

            var assembliesSorted = instances2.Keys.ToList();
            for (int j = 0; j < assembliesSorted.Count - 1; j++)
            {
                var ass = assembliesSorted[j];
                var assReferenced = ass.GetReferencedAssemblies();
                var isMove = false;
                foreach (var assRef in assReferenced)
                {
                    for (int k = j + 1; k < assembliesSorted.Count; k++)
                    {
                        if (assembliesSorted[k].GetName().FullName == assRef.FullName)
                        {
                            isMove = true;
                            break;
                        }
                    }
                    if (isMove) break;
                }

                if (isMove)
                {
                    assembliesSorted.RemoveAt(j);
                    assembliesSorted.Add(ass);
                    j = j - 1;
                }
            }

            return assembliesSorted.SelectMany(x => instances2[x]).Select(x => new Tuple<object, MethodInfo, MethodInfo>(x.Instance, x.ConfigureBindingsLazy, x.ExecuteStartLazy)).ToList();
        }

        /// <summary>
        /// Возвращает обработчик, разрешающий отсутствующие привязки типов.
        /// </summary>
        /// <returns></returns>
        protected virtual IBindingsResolver<TAppCore> GetBindingsResolver()
        {
            return null;
        }

        /// <summary>
        /// Определяет, следует ли игнорировать загрузку указанной сборки во время запуска ядра.
        /// </summary>
        protected virtual bool FilterAssemblyOnStartup(Assembly assembly)
        {
            return true;
        }

        /// <summary>
        /// Позволяет задать дополнительный обработчик, разрешающий отсутствующие привязки типов.
        /// </summary>
        /// <returns></returns>
        public void SetBindingsResolver(IBindingsResolver<TAppCore> resolver)
        {
            var bindingsResolver = new BindingsResolverInternalImpl() { BindingsResolverFromProtected = _bindingsResolver?.BindingsResolverFromProtected, BindingsResolverFromExternal = resolver };
            _objectProvider.RegisterBindingsResolver(bindingsResolver);
            _bindingsResolver = bindingsResolver;
        }
        #endregion

        #region Get/Create/Attach
        #region Get
        /// <summary>
        /// Возвращает singleton-компонент ядра на базе типа <typeparamref name="TQuery"/>. 
        /// </summary>
        /// <returns>Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        [System.Diagnostics.DebuggerStepThrough]
        public TQuery Get<TQuery>() where TQuery : class, IComponentSingleton<TAppCore>
        {
            return Get<TQuery>((Action<TQuery>)null);
        }

        /// <summary>
        /// Возвращает singleton-компонент ядра на базе типа <typeparamref name="TQuery"/>. 
        /// </summary>
        /// <param name="onGetAction">Метод, вызываемый перед возвратом компонента. Может быть null.</param>
        /// <returns>Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        [System.Diagnostics.DebuggerStepThrough]
        public TQuery Get<TQuery>(Action<TQuery> onGetAction) where TQuery : class, IComponentSingleton<TAppCore>
        {
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var instance = _objectProvider.GetInstances<TQuery>(true, true)?.FirstOrDefault();
            onGetAction?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Возвращает singleton-компонент ядра на базе типа <paramref name="queryType"/>, при этом тип <paramref name="queryType"/> должен наследоваться от <typeparamref name="TQueryBase"/>.
        /// </summary>
        /// <returns>Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента.</returns>
        /// <exception cref="ArgumentException">Возникает, тип <paramref name="queryType"/> не наследуется от <typeparamref name="TQueryBase"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        [System.Diagnostics.DebuggerStepThrough]
        public TQueryBase Get<TQueryBase>(Type queryType) where TQueryBase : class, IComponentSingleton<TAppCore>
        {
            return Get<TQueryBase>(queryType, null);
        }

        /// <summary>
        /// Возвращает singleton-компонент ядра на базе типа <paramref name="queryType"/>, при этом тип <paramref name="queryType"/> должен наследоваться от <typeparamref name="TQueryBase"/>.
        /// </summary>
        /// <param name="queryType">Это query-тип, для которого задаются привязки типов (см. <see cref="IBindingsCollection{TAppCore}"/>).</param>
        /// <param name="onGetAction">Метод, вызываемый перед возвратом компонента. Может быть null.</param>
        /// <returns>Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента.</returns>
        /// <exception cref="ArgumentException">Возникает, тип <paramref name="queryType"/> не наследуется от <typeparamref name="TQueryBase"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        [System.Diagnostics.DebuggerStepThrough]
        public TQueryBase Get<TQueryBase>(Type queryType, Action<TQueryBase> onGetAction) where TQueryBase : class, IComponentSingleton<TAppCore>
        {
            if (!typeof(TQueryBase).IsAssignableFrom(queryType)) throw new ArgumentException($"Тип {nameof(queryType)} должен наследоваться от {nameof(TQueryBase)}.", nameof(queryType));
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var instance = (TQueryBase)_objectProvider.GetInstances(queryType, true, true)?.FirstOrDefault();
            onGetAction?.Invoke(instance);
            return instance;
        }

        #endregion

        #region Create
        /// <summary>
        /// Возвращает новый экземпляр компонента ядра на базе типа <typeparamref name="TQuery"/>.
        /// </summary>
        /// <returns>
        /// Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращается экземпляр первого заданного типа (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public TQuery Create<TQuery>() where TQuery : class, IComponentTransient<TAppCore>
        {
            return Create<TQuery>((Action<TQuery>)null);
        }

        /// <summary>
        /// Возвращает новый экземпляр компонента ядра на базе типа <typeparamref name="TQuery"/>.
        /// </summary>
        /// <param name="onCreateAction">Метод, вызываемый перед возвратом компонента. Может быть null.</param>
        /// <returns>
        /// Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращается экземпляр первого заданного типа (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public TQuery Create<TQuery>(Action<TQuery> onCreateAction) where TQuery : class, IComponentTransient<TAppCore>
        {
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var instance = _objectProvider.GetInstances<TQuery>(false, true)?.FirstOrDefault();
            onCreateAction?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Возвращает новый экземпляр компонента ядра на базе типа <paramref name="queryType"/>, при этом тип <paramref name="queryType"/> должен наследоваться от <typeparamref name="TQueryBase"/>.
        /// </summary>
        /// <param name="queryType">Это query-тип, для которого задаются привязки типов (см. <see cref="IBindingsCollection{TAppCore}"/>).</param>
        /// <returns>
        /// Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращается экземпляр первого заданного типа (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="ArgumentException">Возникает, тип <paramref name="queryType"/> не наследуется от <typeparamref name="TQueryBase"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public TQueryBase Create<TQueryBase>(Type queryType) where TQueryBase : class, IComponentTransient<TAppCore>
        {
            return Create<TQueryBase>(queryType, null);
        }

        /// <summary>
        /// Возвращает новый экземпляр компонента ядра на базе типа <paramref name="queryType"/>, при этом тип <paramref name="queryType"/> должен наследоваться от <typeparamref name="TQueryBase"/>.
        /// </summary>
        /// <param name="queryType">Это query-тип, для которого задаются привязки типов (см. <see cref="IBindingsCollection{TAppCore}"/>).</param>
        /// <param name="onCreateAction">Метод, вызываемый перед возвратом компонента. Может быть null.</param>
        /// <returns>
        /// Возвращает экземпляр компонента ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращается экземпляр первого заданного типа (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="ArgumentException">Возникает, тип <paramref name="queryType"/> не наследуется от <typeparamref name="TQueryBase"/>.</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public TQueryBase Create<TQueryBase>(Type queryType, Action<TQueryBase> onCreateAction) where TQueryBase : class, IComponentTransient<TAppCore>
        {
            if (!typeof(TQueryBase).IsAssignableFrom(queryType)) throw new ArgumentException($"Тип {nameof(queryType)} должен наследоваться от {nameof(TQueryBase)}.", nameof(queryType));
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            var instance = (TQueryBase)_objectProvider.GetInstances(queryType, false, true)?.FirstOrDefault();
            onCreateAction?.Invoke(instance);
            return instance;
        }

        /// <summary>
        /// Возвращает список новых экземпляров компонентов ядра на базе типа <typeparamref name="TQuery"/>. 
        /// </summary>
        /// <returns>
        /// Возвращает перечисление экземпляров компонентов ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращаются экземпляры всех заданных типов (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public IEnumerable<TQuery> CreateAll<TQuery>() where TQuery : class, IComponentTransient<TAppCore>
        {
            return CreateAll<TQuery>(null);
        }

        /// <summary>
        /// Возвращает список новых экземпляров компонентов ядра на базе типа <typeparamref name="TQuery"/>. 
        /// </summary>
        /// <param name="onCreateAction">Метод, вызываемый перед возвратом списка компонентов. Может быть null.</param>
        /// <returns>
        /// Возвращает перечисление экземпляров компонентов ядра или null, если не задана привязка типа или не удалось создать экземпляр компонента. 
        /// Если была задана привязка нескольких типов, то возвращаются экземпляры всех заданных типов (см. <see cref="BindingsCollection{TAppCore}.SetTransient{TTransient}(Type[])"/>).
        /// </returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public IEnumerable<TQuery> CreateAll<TQuery>(Action<IEnumerable<TQuery>> onCreateAction) where TQuery : class, IComponentTransient<TAppCore>
        {
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            IEnumerable<TQuery> instances = _objectProvider.GetInstances<TQuery>(false, false);
            onCreateAction?.Invoke(instances);
            return instances;
        }
        #endregion

        /// <summary>
        /// Возвращает список типов, имеющих привязку к типу <typeparamref name="TQueryType"/> (см. описание методов <see cref="BindingsCollection{TAppCore}"/>). 
        /// </summary>
        /// <returns>Возвращает коллекцию типов или null, если для <typeparamref name="TQueryType"/> не найдено привязок.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public IEnumerable<Type> GetBindedTypes<TQueryType>() where TQueryType : class
        {
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            return _objectProvider.GetBindedTypes(typeof(TQueryType));
        }

        /// <summary>
        /// Возвращает список типов, зарегистрированных в качестве queryType (см. описание методов <see cref="BindingsCollection{TAppCore}"/>). 
        /// </summary>
        /// <returns>Возвращает коллекцию типов.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public IEnumerable<Type> GetQueryTypes()
        {
            if (!_started && !_starting) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            return _objectProvider.GetQueryTypes();
        }

        #endregion

        #region Для перегрузки в наследниках.
        /// <summary>
        /// Вызывается при запуске ядра после определения всех зависимостей и автозагружаемых объектов (см. <see cref="IAutoStart"/>).
        /// </summary>
        protected virtual void OnStart()
        {
        }

        /// <summary>
        /// Вызывается, когда необходимо предоставить привязки типов в коллекцию <see cref="BindingsCollection{TAppCore}"/> для дальнейшего использования при запуске ядра.
        /// </summary>
        protected virtual void OnBindingsRequired(IBindingsCollection<TAppCore> bindingsCollection)
        {

        }

        /// <summary>
        /// Вызывается после вызова <see cref="OnStart"/> после применения привязок типов.
        /// </summary>
        protected virtual void OnBindingsApplied()
        {

        }

        /// <summary>
        /// Вызывается после вызова <see cref="OnBindingsApplied"/> после запуска компонентов, для которых queryType наследует интерфейс <see cref="IAutoStart"/>.
        /// </summary>
        protected virtual void OnBindingsAutoStart()
        {

        }

        /// <summary>
        /// Вызывается при остановке ядра. Остановка может быть вызвана как прямым вызовом <see cref="Stop"/>, так и через использование <see cref="IDisposable.Dispose"/>. 
        /// </summary>
        protected virtual void OnStop()
        {

        }
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            Stop();
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает провайдер объектов на основе привязок типов.
        /// </summary>
        public IBindingsObjectProvider ObjectProvider
        {
            get => _objectProvider;
        }

        /// <summary>
        /// Возвращает или задает уровень отладочной информации, выводимый в лог (см. <see cref="Debug.WriteLine(object)"/>).
        /// </summary>
        public DebugLevel AppDebugLevel
        {
            get;
            set;
        }

        #endregion
    }
}
