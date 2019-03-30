using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    /// <summary>
    /// Менеджер, управляющий модулями системы.
    /// Система разделена на модули с определенным функционалом, к модулям могут быть привязаны операции, доступные пользователю извне (для внешних запросов).
    /// Права доступа регистрируются на модуль.
    /// </summary>
    public class ModulesManager<TApplication> : CoreComponentBase<TApplication>, IComponentSingleton<TApplication>, IAutoStart
        where TApplication : AppCore<TApplication>
    {
        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly ModulesManager<TApplication> _manager;

            public InstanceActivatedHandlerImpl(ModulesManager<TApplication> manager)
            {
                _manager = manager;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is ModuleBase<TApplication> moduleCandidate)
                {
                    _manager.GetType().GetMethod(nameof(LoadModule), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(typeof(TRequestedType)).Invoke(_manager, new object[] { instance });
                }
            }
        }

        private readonly bool _isStartModulesOnManagerStart = true;
        protected readonly object _syncRoot = new object();
        protected List<Tuple<Type, ModuleBase<TApplication>>> _modules = new List<Tuple<Type, ModuleBase<TApplication>>>();
        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;
        
        /// <summary>
        /// Создает новый экземпляр менеджера модулей.
        /// </summary>
        /// <param name="isStartModulesOnManagerStart">Указывает, следует ли запускать модули автоматически при старте менеджера или запуск модулей будет выполняться вручную в дальнейшем.</param>
        public ModulesManager(bool isStartModulesOnManagerStart = true)
        {
            _isStartModulesOnManagerStart = isStartModulesOnManagerStart;
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
        protected void StartModules()
        {
            lock (_syncRoot)
            {
                var moduleCoreType = typeof(ModuleBase<TApplication>);

                // Сначала ищем список модулей.
                var filteredTypesList = AppCore.GetQueryTypes().Where(FilterModuleTypes);
                if (!filteredTypesList.IsNullOrEmpty() && filteredTypesList.Any(type => !typeof(ModuleBase<TApplication>).IsAssignableFrom(type)))
                    throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, typeof(ModulesManager<TApplication>), new ArgumentException());

                // todo добавить журналирование this.RegisterEvent(Journaling.EventType.Info, "Первичная загрузка списка модулей", $"Найдены следующие привязки модулей:\r\n - {string.Join(";\r\n - ", modulesTypesList.Keys.Select(x => x.FullName))}.");

                foreach (var moduleType in filteredTypesList)
                {
                    try
                    {
                        var moduleInstance = AppCore.Get<ModuleBase<TApplication>>(moduleType);
                    }
                    catch (Exception ex)
                    {
                        if (typeof(ICritical).IsAssignableFrom(moduleType)) throw new ApplicationStartException(ApplicationStartStep.BindingsAutoStartCritical, moduleType, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Используется для определения того, является ли тип <paramref name="typeFromDI"/> типом модуля. По-умолчанию возвращает только типы, наследующие <see cref="ModuleBase{TApplication}"/>. Может быть перегружен для изменения поиска типов.
        /// свойство <see cref="ApplicationStartException.Step"/> будет равно <see cref="ApplicationStartStep.BindingsAutoStartCritical"/>, 
        /// свойство <see cref="ApplicationStartException.ContextType"/> будет равно <see cref="ModulesManager{TApplication}"/>, 
        /// свойство <see cref="Exception.InnerException"/> будет содержать исключение <see cref="ArgumentException"/>.
        /// </summary>
        protected virtual bool FilterModuleTypes(Type typeFromDI)
        {
            return typeof(ModuleBase<TApplication>).IsAssignableFrom(typeFromDI);
        }

        /// <summary>
        /// Вызывается для инициализации модуля <paramref name="module"/>, созданного в DI-контейнере. 
        /// </summary>
        /// <typeparam name="TModuleType">Query-тип из привязок типов в контейнере DI, на основании которого создан экземпляр объекта <paramref name="module"/>.</typeparam>
        /// <param name="module">Экземпляр модуля, созданный в контейнере DI.</param>
        protected virtual void LoadModule<TModuleType>(TModuleType module) where TModuleType : ModuleBase<TApplication>
        {
            _modules.RemoveAll(x => x.Item1 == typeof(TModuleType));
            module.OnModuleStart();
            _modules.Add(new Tuple<Type, ModuleBase<TApplication>>(typeof(TModuleType), module));
            // todo добавить журналирование this.RegisterEvent(Journaling.EventType.Error, $"Загрузка модуля '{moduleType.FullName}'", $"Модуль загружен на основе типа '{module.GetType().FullName}' с Id={config.m_id}.");
        }

        /// <summary>
        /// Может быть использован для вызова метода <see cref="ModuleBase{TApplication}.OnModuleStart"/> в других реализациях менеджера модулей. 
        /// Метод <see cref="ModuleBase{TApplication}.OnModuleStart"/> будет вызван только для новых модулей, т.е. модулей, отсутствующих в списке уже инициализированных.
        /// </summary>
        protected void LoadModuleCallModuleStart(ModuleBase<TApplication> module)
        {
            if (!_modules.Any(x=>x.Item2 == module))
            {
                module.OnModuleStart();
            }
        }

        /// <summary>
        /// Возращает список модулей, зарегистрированных в системе.
        /// </summary>
        protected List<ModuleBase<TApplication>> GetModules()
        {
            lock (_syncRoot) return _modules.Select(x => x.Item2).ToList();
        }

        /// <summary>
        /// Возвращает модуль указанного типа <typeparamref name="TModule"/>, если таковой имеется среди загруженных. 
        /// </summary>
        /// <typeparam name="TModule">Тип модуля. Это должен быть query-тип из привязок типов (см. описание <see cref="ApplicationBase{TSelfReference}"/>).</typeparam>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        public TModule GetModule<TModule>(bool isSearchNested = true) where TModule : ModuleBase<TApplication>
        {
            lock (_syncRoot)
            {
                return _modules.Where(x=>x.Item1 == typeof(TModule)).Select(x=>x.Item2 as TModule).FirstOrDefault();
            }
        }

        /// <summary>
        /// Возвращает модуль с идентификатором <paramref name="moduleID"/> (см. <see cref="ModuleBase{TApplication}.ModuleID"/>). 
        /// </summary>
        /// <param name="moduleID">Идентификатор модуля.</param>
        /// <returns>Объект модуля либо null, если подходящий модуль не найден.</returns>
        protected ModuleBase<TApplication> GetModule(Guid moduleID)
        {
            lock (_syncRoot)
            {
                return _modules.Where(x => x.Item2.ModuleID == moduleID).Select(x => x.Item2).FirstOrDefault();
            }
        }
        #endregion
    }
}
