using System;
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
        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly TAppCore _core;

            public InstanceActivatedHandlerImpl(TAppCore core)
            {
                _core = core;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is IComponent<TAppCore> coreComponent)
                {
                    if (!coreComponent.GetState().In(CoreComponentState.Started, CoreComponentState.Stopped))
                    {
                        if (coreComponent is CoreComponentBase<TAppCore> coreComponentBase) coreComponentBase.Start(_core);
                        //coreComponent.Start(_core);
                    }
                    _core.OnInstanceActivated<TRequestedType>(coreComponent);
                }

                if (instance is IComponentSingleton<TAppCore> coreComponentSingleton)
                {
                    _core._activatedSingletonInstances.Add(coreComponentSingleton);
                }
            }
        }

        private bool _started = false;
        private bool _stopped = false;
        private bool _bindingsPreparing = false;
        private List<IComponentSingleton<TAppCore>> _activatedSingletonInstances = null;

        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;
        private BindingsObjectProvider _objectProvider = new BindingsObjectProvider(Enumerable.Empty<KeyValuePair<Type, BindingDescription>>());

        /// <summary>
        /// Создает новый объект <see cref="AppCore{TAppCore}"/>. 
        /// </summary>
        protected AppCore()
        {
            if (!typeof(TAppCore).IsAssignableFrom(this.GetType())) throw new TypeAccessException($"Параметр-тип {nameof(TAppCore)} должен находиться в цепочке наследования текущего типа.");
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl((TAppCore)(object)this);
            _activatedSingletonInstances = new List<IComponentSingleton<TAppCore>>();
        }

        /// <summary>
        /// Старт ядра.
        /// </summary>
        public void Start()
        {
            var startStep = ApplicationStartStep.PrepareAssemblyStandardList;

            try
            {
                _started = true;

                var assemblyStartupList = GetAssemblyStartupList();

                try
                {
                    startStep = ApplicationStartStep.BindingsRequired;

                    _bindingsPreparing = true;

                    var bindingsCollection = new BindingsCollection<TAppCore>();
                    assemblyStartupList.Where(x => x.Item2 != null).ForEach(x => x.Item2.Invoke(x.Item1, new object[] { bindingsCollection }));
                    OnBindingsRequired(bindingsCollection);

                    startStep = ApplicationStartStep.BindingsApplying;

                    BindingsApply(bindingsCollection);
                    ((DI.IBindingsObjectProvider)_objectProvider).RegisterInstanceActivatedHandler(_instanceActivatedHandler);
                    OnBindingsApplied();
                }
                finally
                {
                    _bindingsPreparing = false;
                }

                startStep = ApplicationStartStep.BindingsAutoStart;

                BindingsAutoStart();
                OnBindingsAutoStart();

                startStep = ApplicationStartStep.Start;

                assemblyStartupList.Where(x => x.Item3 != null).ForEach(x => x.Item3.Invoke(x.Item1, new object[] { this }));
                OnStart();
            }
            catch (ApplicationStartException)
            {
                _started = false;
                throw;
            }
            catch (Exception ex)
            {
                _started = false;
                throw new ApplicationStartException(startStep, Types.TypeHelpers.ExtractGenericType(this.GetType(), typeof(AppCore<TAppCore>)), ex);
            }
        }

        /// <summary>
        /// Остановка ядра, остановка всех компонентов ядра.
        /// </summary>
        public void Stop()
        {
            if (_stopped) return;
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");

            foreach(var componentSingleton in _activatedSingletonInstances)
            {
                try
                {
                    componentSingleton.Stop();
                }
                catch(Exception ex)
                {
                    Debug.WriteLineNoLog($"Ошибка во время остановки компонента '{componentSingleton.GetType().FullName}': {ex.Message}");
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
            return _started ? CoreComponentState.Started : (_stopped ? CoreComponentState.Stopped : CoreComponentState.None);
        }

        #region Привязка типов
        private void BindingsApply(BindingsCollection<TAppCore> collection)
        {
            if (!_bindingsPreparing) throw new InvalidOperationException("Невозможно устанавливать привязки после запуска ядра.");
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            _objectProvider = new BindingsObjectProvider(collection._typesCollection.ToList());
        }

        private void BindingsAutoStart()
        {
            var typesForAutoStart = _objectProvider.
                GetQueryTypes().
                Where(type => typeof(IAutoStart).IsAssignableFrom(type) && typeof(IComponentSingleton<TAppCore>).IsAssignableFrom(type)).
                ToList();

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

            foreach (var type in typesForAutoStart)
            {
                try
                {
                    var instance = this.Get<IComponentSingleton<TAppCore>>(type);
                }
                catch (Exception ex)
                {
                    if (typeof(ICritical).IsAssignableFrom(type)) throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, type, ex);
                }
            }
        }

        private List<Tuple<object, MethodInfo, MethodInfo>> GetAssemblyStartupList()
        {
            var currentType = this.GetType();

            var assemblyPublicKeyTokenIgnored = new string[] { "b03f5f7f11d50a3a", "31bf3856ad364e35", "b77a5c561934e089", "71e9bce111e9429c" };

            var instances2 = AppDomain.CurrentDomain.
                GetAssemblies().
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
                                var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IConfigureBindings<>));
                                if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0])) return true;
                            }
                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                            {
                                var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IExecuteStart<>));
                                if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0])) return true;
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
                                                    var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IConfigureBindings<>));
                                                    if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
                                                    {
                                                        methodConfigureBindings2 = interfaceType.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings));
                                                    }
                                                }

                                                MethodInfo methodExecuteStart2 = null;
                                                if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                                                {
                                                    var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IExecuteStart<>));
                                                    if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
                                                    {
                                                        methodExecuteStart2 = interfaceType.GetMethod(nameof(IExecuteStart<object>.ExecuteStart));
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
                                var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IConfigureBindings<>));
                                if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
                                {
                                    methodConfigureBindings = interfaceType.GetMethod(nameof(IConfigureBindings<object>.ConfigureBindings));
                                }
                            }

                            MethodInfo methodExecuteStart = null;
                            if (type.GetInterfaces().Any(x => x.Name.Contains(nameof(IExecuteStart<int>))))
                            {
                                var interfaceType = Types.TypeHelpers.ExtractGenericInterface(type, typeof(IExecuteStart<>));
                                if (interfaceType != null && typeof(TAppCore).IsAssignableFrom(interfaceType.GetGenericArguments()[0]))
                                {
                                    methodExecuteStart = interfaceType.GetMethod(nameof(IExecuteStart<object>.ExecuteStart));
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
                            Select(x => new
                            {
                                Instance = Activator.CreateInstance(x.Type),
                                x.ConfigureBindings,
                                x.ExecuteStart
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

            return assembliesSorted.SelectMany(x => instances2[x]).Select(x => new Tuple<object, MethodInfo, MethodInfo>(x.Instance, x.ConfigureBindings, x.ExecuteStart)).ToList();
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
        public TQuery Get<TQuery>(Action<TQuery> onGetAction) where TQuery : class, IComponentSingleton<TAppCore>
        {
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
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
        public TQueryBase Get<TQueryBase>(Type queryType, Action<TQueryBase> onGetAction) where TQueryBase : class, IComponentSingleton<TAppCore>
        {
            if (!typeof(TQueryBase).IsAssignableFrom(queryType)) throw new ArgumentException($"Тип {nameof(queryType)} должен наследоваться от {nameof(TQueryBase)}.", nameof(queryType));
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
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
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
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
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
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
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            IEnumerable<TQuery> instances = _objectProvider.GetInstances<TQuery>(false, false);
            onCreateAction?.Invoke(instances);
            return instances;
        }
        #endregion

        ///// <summary>
        ///// Присоединяет компонент <paramref name="component"/> к ядру и запускает его (<see cref="ICoreComponent.Start(TAppCore)"/>). 
        ///// Присоединить можно только не присоединенный к другому ядру компонент, в противном случае будет сгенерировано исключение.
        ///// </summary>
        ///// <exception cref="ArgumentNullException">Генерируется, если <paramref name="component"/> равен null.</exception>
        ///// <exception cref="InvalidOperationException">Генерируется, если компонент уже присоединен к другому ядру.</exception>
        //public void Attach<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponent
        //{
        //    if (component == null) throw new ArgumentNullException(nameof(component));
        //    if (component.GetAppCore() != this) throw new InvalidOperationException("Компонент уже присоединен к другому ядру.");

        //    if (component.GetState() == CoreComponentState.None) component.Start((TAppCore)this);
        //}

        ///// <summary>
        ///// Пытается присоединить компонент <paramref name="component"/> к ядру и запустить его (<see cref="ICoreComponent.Start(TAppCore)"/>). 
        ///// Присоединить можно только не присоединенный к другому ядру компонент, в противном случае будет возвращено значение false.
        ///// </summary>
        ///// <returns>Возвращает false, если <paramref name="component"/> равен null или компонент уже присоединен к другому ядру.</returns>
        //public bool TryAttach<TCoreComponent>(TCoreComponent component) where TCoreComponent : class, ICoreComponent
        //{
        //    if (component == null) return false;
        //    if (component.GetAppCore() != this) return false;

        //    if (component.GetState() == CoreComponentState.None) component.Start((TAppCore)this);
        //    return true;
        //}

        /// <summary>
        /// Возвращает список типов, имеющих привязку к типу <typeparamref name="TQueryType"/> (см. описание методов <see cref="BindingsCollection{TAppCore}"/>). 
        /// </summary>
        /// <returns>Возвращает коллекцию типов или null, если для <typeparamref name="TQueryType"/> не найдено привязок.</returns>
        /// <exception cref="InvalidOperationException">Возникает, если ядро не было запущено (не был вызван метод <see cref="Start"/>).</exception>
        /// <exception cref="InvalidOperationException">Возникает, если ядро было остановлено (был вызван метод <see cref="Stop"/>).</exception>
        public IEnumerable<Type> GetBindedTypes<TQueryType>() where TQueryType : class
        {
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
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
            if (!_started) throw new InvalidOperationException("Ядро не запущено. Вызовите Start.");
            if (_stopped) throw new InvalidOperationException("Ядро остановлено, повторный запуск и использование невозможны.");

            return _objectProvider.GetQueryTypes();
        }

        #endregion

        #region Для перегрузки в наследниках.
        /// <summary>
        /// Вызывается при запуске ядра.
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

        /// <summary>
        /// Вызывается, когда создан новый экземпляр компонента <paramref name="instance"/> на основании затребованного типа <typeparamref name="TRequestedType"/>.
        /// </summary>
        protected virtual void OnInstanceActivated<TRequestedType>(IComponent<TAppCore> instance)
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
        public DI.IBindingsObjectProvider ObjectProvider
        {
            get => _objectProvider;
        }
        #endregion
    }
}
