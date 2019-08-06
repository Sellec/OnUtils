﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;
    using Configuration;
    using Data;
    using DB;
    using Journaling;

    /// <summary>
    /// Менеджер, управляющий модулями системы.
    /// Система разделена на модули с определенным функционалом, к модулям могут быть привязаны операции, доступные пользователю извне (для внешних запросов).
    /// Права доступа регистрируются на модуль.
    /// </summary>
    public class ModulesManager<TAppCoreSelfReference> : CoreComponentBase<TAppCoreSelfReference>, IComponentSingleton<TAppCoreSelfReference>, IAutoStart, IUnitOfWorkAccessor<CoreContext>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly ModulesManager<TAppCoreSelfReference> _manager;

            public InstanceActivatedHandlerImpl(ModulesManager<TAppCoreSelfReference> manager)
            {
                _manager = manager;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is ModuleCore<TAppCoreSelfReference> moduleCandidate)
                {
                    _manager.GetType().GetMethod(nameof(LoadModule), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TRequestedType)).Invoke(_manager, new object[] { instance });
                }
            }
        }

        private readonly bool _isStartModulesOnManagerStart = true;
        private readonly object _syncRoot = new object();
        private List<Tuple<Type, ModuleCore<TAppCoreSelfReference>>> _modules = new List<Tuple<Type, ModuleCore<TAppCoreSelfReference>>>();
        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;

        /// <summary>
        /// Создает новый экземпляр менеджера модулей.
        /// </summary>
        public ModulesManager()
        {
            DeprecatedSingletonInstances.Set<TAppCoreSelfReference>(this);
            _isStartModulesOnManagerStart = false;
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl(this);
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            AppCore.ObjectProvider.RegisterInstanceActivatedHandler(_instanceActivatedHandler);
            if (_isStartModulesOnManagerStart) StartModules();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        /// <summary>
        /// Ищет и запускает все модули, для которых зарегистрированы привязки типов.
        /// </summary>
        internal protected void StartModules()
        {
            lock (_syncRoot)
            {
                var moduleCoreType = typeof(ModuleCore<TAppCoreSelfReference>);

                // Сначала ищем список модулей.
                var filteredTypesList = AppCore.GetQueryTypes().Where(FilterModuleTypes);
                if (!filteredTypesList.IsNullOrEmpty() && filteredTypesList.Any(type => !typeof(ModuleCore<TAppCoreSelfReference>).IsAssignableFrom(type)))
                    throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, typeof(ModulesManager<TAppCoreSelfReference>), new ArgumentException());

                // todo добавить журналирование this.RegisterEvent(Journaling.EventType.Info, "Первичная загрузка списка модулей", $"Найдены следующие привязки модулей:\r\n - {string.Join(";\r\n - ", modulesTypesList.Keys.Select(x => x.FullName))}.");

                foreach (var moduleType in filteredTypesList)
                {
                    try
                    {
                        var moduleInstance = AppCore.Get<ModuleCore<TAppCoreSelfReference>>(moduleType);
                    }
                    catch (Exception ex)
                    {
                        if (typeof(ICritical).IsAssignableFrom(moduleType)) throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, moduleType, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Используется для определения того, является ли тип <paramref name="typeFromDI"/> типом модуля. По-умолчанию возвращает только типы, наследующие <see cref="ModuleCore{TAppCoreSelfReference, TSelfReference}"/>. Может быть перегружен для изменения поиска типов.
        /// свойство <see cref="ApplicationStartException.Step"/> будет равно <see cref="ApplicationStartStep.BindingsAutoStartCritical"/>, 
        /// свойство <see cref="ApplicationStartException.ContextType"/> будет равно <see cref="ModulesManager{TAppCoreSelfReference}"/>, 
        /// свойство <see cref="Exception.InnerException"/> будет содержать исключение <see cref="ArgumentException"/>.
        /// </summary>
        protected bool FilterModuleTypes(Type typeFromDI)
        {
            var moduleCoreType = typeof(ModuleCore<TAppCoreSelfReference>);
            return moduleCoreType.IsAssignableFrom(typeFromDI) && typeFromDI.GetCustomAttribute< ModuleCoreAttribute>() != null;
        }

        private void LoadModule<TModuleType>(TModuleType module)
        {
            GetType().GetMethod(nameof(LoadModuleCustom), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TModuleType)).Invoke(this, new object[] { module });
        }

        private void LoadModuleCustom<TModuleType>(TModuleType module)
            where TModuleType : ModuleCore<TAppCoreSelfReference, TModuleType>
        {
            var moduleType = typeof(TModuleType);
            var moduleCoreAttribute = moduleType.GetCustomAttribute<ModuleCoreAttribute>();

            var moduleRegisterHandlerTypes = AppCore.GetQueryTypes().Where(x => typeof(IModuleRegisteredHandler<TAppCoreSelfReference>).IsAssignableFrom(x)).ToList();
            var moduleRegisterHandlers = moduleRegisterHandlerTypes.Select(x => AppCore.Get<IModuleRegisteredHandler<TAppCoreSelfReference>>(x)).ToList();

            using (var db = this.CreateUnitOfWork())
            {
                var fullName = moduleType.FullName;
                if (moduleType.IsConstructedGenericType && moduleType.Assembly == typeof(ModuleCore<>).Assembly)
                {
                    if (moduleType.GenericTypeArguments.Length == 1)
                    {
                        var assemblyName = moduleType.GenericTypeArguments[0].Assembly.GetName();
                        fullName = fullName.Replace(", Version=" + assemblyName.Version.ToString(), "");
                        fullName = fullName.Replace(", Culture=" + (string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : assemblyName.CultureName), "");
                    }
                }

                var config = db.Module.Where(x => x.UniqueKey == fullName).FirstOrDefault();
                if (config == null)
                {
                    config = new ModuleConfig() { UniqueKey = fullName, DateChange = DateTime.Now };
                    db.Module.Add(config);
                    db.SaveChanges();
                }

                module.ID = config.IdModule;
                module._moduleCaption = moduleCoreAttribute.Caption;
                module._moduleUrlName = moduleCoreAttribute.DefaultUrlName;

                var configurationManipulator = new ModuleConfigurationManipulator<TAppCoreSelfReference, TModuleType>(module, CreateValuesProviderForModule(module));
                ((IComponentStartable<TAppCoreSelfReference>)configurationManipulator).Start(AppCore);
                module._configurationManipulator = configurationManipulator;

                var cfg = configurationManipulator.GetUsable<ModuleConfiguration<TAppCoreSelfReference, TModuleType>>();

                if (!string.IsNullOrEmpty(cfg.UrlName)) module._moduleUrlName = cfg.UrlName;
                module.InitModule();
                moduleRegisterHandlers.ForEach(x => x.OnModuleInitialized<TModuleType>(module));

                _modules.RemoveAll(x => x.Item1 == typeof(TModuleType));
                LoadModuleCallModuleStart(module);
                _modules.Add(new Tuple<Type, ModuleCore<TAppCoreSelfReference>>(typeof(TModuleType), module));

                AppCore.Get<JournalingManager<TAppCoreSelfReference>>().RegisterJournalTyped<TModuleType>("Журнал событий модуля '" + module.Caption + "'");

                this.RegisterEvent(
                     EventType.Info,
                    $"Загрузка модуля '{moduleType.FullName}'",
                    $"Модуль загружен на основе типа '{module.GetType().FullName}' с Id={config.IdModule}."
                );
            }
        }

        /// <summary>
        /// Может быть использован для вызова метода <see cref="ModuleCore{TAppCoreSelfReference}.OnModuleStart"/> в других реализациях менеджера модулей. 
        /// Метод <see cref="ModuleCore{TAppCoreSelfReference}.OnModuleStart"/> будет вызван только для новых модулей, т.е. модулей, отсутствующих в списке уже инициализированных.
        /// </summary>
        protected void LoadModuleCallModuleStart(ModuleCore<TAppCoreSelfReference> module)
        {
            if (!_modules.Any(x => x.Item2 == module))
            {
                module.OnModuleStart();
            }
        }

        /// <summary>
        /// Возвращает модуль указанного типа <typeparamref name="TModule"/>, если таковой имеется среди загруженных. 
        /// </summary>
        /// <typeparam name="TModule">Тип модуля. Это должен быть query-тип из привязок типов (см. описание <see cref="ApplicationCore{TAppCoreSelfReference}"/>).</typeparam>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public TModule GetModule<TModule>(bool isSearchNested = true) where TModule : ModuleCore<TAppCoreSelfReference>
        {
            lock (_syncRoot)
            {
                return _modules.Where(x => x.Item1 == typeof(TModule)).Select(x => x.Item2 as TModule).FirstOrDefault();
            }
        }

        /// <summary>
        /// Возращает список модулей, зарегистрированных в системе.
        /// </summary>
        public List<ModuleCore<TAppCoreSelfReference>> GetModules()
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => (ModuleCore<TAppCoreSelfReference>)x.Item2).
                    ToList();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с url-доступным именем <paramref name="urlName"/> (см. <see cref="ModuleCore{TAppCoreSelfReference}.UrlName"/>).
        /// </summary>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore<TAppCoreSelfReference> GetModule(string urlName)
        {
            lock (_syncRoot)
            {
                // Сначала ищем имя модуля среди сохраненных имен модулей. Если совсем ничего нет - то по UrlName, который в случае пустого сохраненного имени хранит гуид.
                var module =
                    _modules.
                    Select(x => x.Item2).
                    Where(x => !string.IsNullOrEmpty(x._moduleUrlName) && x._moduleUrlName.Equals(urlName, StringComparison.InvariantCultureIgnoreCase)).
                    FirstOrDefault() ??
                    _modules.
                    Select(x => x.Item2).
                    Where(x => !string.IsNullOrEmpty(x.UrlName) && x.UrlName.Equals(urlName, StringComparison.InvariantCultureIgnoreCase)).
                    FirstOrDefault();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с идентификатором <paramref name="moduleID"/> (см. <see cref="ModuleCore{TAppCoreSelfReference}.IdModule"/>). 
        /// </summary>
        /// <param name="moduleID">Идентификатор модуля.</param>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore<TAppCoreSelfReference> GetModule(int moduleID)
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => x.Item2).
                    Where(x => x.IdModule == moduleID).
                    FirstOrDefault();

                return module;
            }
        }

        /// <summary>
        /// Возвращает модуль с уникальным именем <paramref name="uniqueName"/> (см. <see cref="ModuleCore{TAppCoreSelfReference}.UniqueName"/>). 
        /// </summary>
        /// <param name="uniqueName">Уникальное имя модуля.</param>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public ModuleCore<TAppCoreSelfReference> GetModule(Guid uniqueName)
        {
            lock (_syncRoot)
            {
                var module = _modules.
                    Select(x => x.Item2).
                    Where(x => x.UniqueName == uniqueName).
                    FirstOrDefault();

                return module;
            }
        }

        internal ConfigurationValuesProvider CreateValuesProviderForModule(ModuleCore<TAppCoreSelfReference> module)
        {
            var configurationValuesProvider = new ConfigurationValuesProvider();
            using (var db = new CoreContext())
            {
                var config = db.Module.FirstOrDefault(x => x.IdModule == module.ID);
                if (config != null)
                {
                    configurationValuesProvider.Load(config.Configuration);
                }
            }
            return configurationValuesProvider;
        }

        internal ApplyConfigurationResult ApplyModuleConfiguration<TModule, TConfiguration>(TConfiguration configuration, ModuleConfigurationManipulator<TAppCoreSelfReference, TModule> moduleConfigurationManipulator, TModule module)
            where TModule : ModuleCore<TAppCoreSelfReference, TModule>
            where TConfiguration : ModuleConfiguration<TAppCoreSelfReference, TModule>, new()
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var context = AppCore.GetUserContextManager().GetCurrentUserContext();

            var permissionCheck = module.CheckPermission(context, ModulesConstants.PermissionSaveConfiguration);
            if (permissionCheck == CheckPermissionResult.Denied) return ApplyConfigurationResult.PermissionDenied;

            var moduleType = typeof(TModule);
            var moduleCoreAttribute = moduleType.GetCustomAttribute<ModuleCoreAttribute>();

            var urlNameEncoded = System.Web.HttpUtility.UrlEncode(configuration.UrlName);
            configuration.UrlName = urlNameEncoded;

            using (var db = this.CreateUnitOfWork())
            using (var scope = db.CreateScope())
            {
                var moduleConfig = db.Module.FirstOrDefault(x => x.IdModule == module.ID);
                if (moduleConfig == null)
                {
                    var fullName = typeof(TModule).FullName;
                    if (moduleType.IsConstructedGenericType && moduleType.Assembly == typeof(ModuleCore<>).Assembly)
                    {
                        if (moduleType.GenericTypeArguments.Length == 1)
                        {
                            var assemblyName = moduleType.GenericTypeArguments[0].Assembly.GetName();
                            fullName = fullName.Replace(", Version=" + assemblyName.Version.ToString(), "");
                            fullName = fullName.Replace(", Culture=" + (string.IsNullOrEmpty(assemblyName.CultureName) ? "neutral" : assemblyName.CultureName), "");
                        }
                    }

                    moduleConfig = new ModuleConfig() { UniqueKey = fullName, DateChange = DateTime.Now, IdUserChange = 0 };
                    db.Module.AddOrUpdate(moduleConfig);
                }

                moduleConfig.Configuration = configuration._valuesProvider.Save();
                moduleConfig.DateChange = DateTime.Now;
                moduleConfig.IdUserChange = context.IdUser;

                db.SaveChanges();
                scope.Commit();

                module.ID = moduleConfig.IdModule;
                module._moduleUrlName = string.IsNullOrEmpty(configuration.UrlName) ? moduleCoreAttribute.DefaultUrlName : configuration.UrlName;
                moduleConfigurationManipulator._valuesProviderUsable.Load(moduleConfig.Configuration);
            }

            return ApplyConfigurationResult.Success;
        }

        #endregion
    }
}

