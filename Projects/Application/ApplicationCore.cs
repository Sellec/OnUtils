using System;

namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;
    using Configuration;
    using Modules;

    /// <summary>
    /// Ядро приложения.
    /// </summary>
    public abstract class ApplicationCore<TAppCoreSelfReference> : AppCore<TAppCoreSelfReference> 
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        private CoreConfiguration<TAppCoreSelfReference> _configurationAccessor = null;

        /// <summary>
        /// </summary>
        public ApplicationCore(string physicalApplicationPath)
        {
            try
            {
                LibraryEnumeratorFactory.LibraryDirectory = physicalApplicationPath;
                ApplicationWorkingFolder = physicalApplicationPath;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error init ApplicationCore: {0}", ex.ToString());
                if (ex.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner: {0}", ex.InnerException.Message);
                if (ex.InnerException?.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner inner: {0}", ex.InnerException.InnerException.Message);
                if (ex.InnerException?.InnerException?.InnerException != null) Debug.WriteLine("Error init ApplicationCore inner inner inner: {0}", ex.InnerException.InnerException.InnerException.Message);

                throw;
            }
        }

        #region Методы
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            OnApplicationStartBase();
            OnApplicationStart();
        }

        private void OnApplicationStartBase()
        {

        }

        /// <summary>
        /// Вызывается единственный раз при запуске ядра.
        /// </summary>
        protected virtual void OnApplicationStart()
        {
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnBindingsApplied"/>.
        /// </summary>
        protected sealed override void OnBindingsApplied()
        {
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnBindingsRequired(IBindingsCollection{TAppCore})"/>.
        /// </summary>
        protected override void OnBindingsRequired(IBindingsCollection<TAppCoreSelfReference> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ApplicationLauncher<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Items.ItemsManager<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Journaling.JournalingManager<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Messaging.MessagingManager<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Languages.Manager<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Modules.ModulesManager<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Modules.ModulesLoadStarter<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Users.UserContextManager<TAppCoreSelfReference>>();
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnInstanceActivated{TRequestedType}(IComponent{TAppCore})"/>.
        /// </summary>
        protected override void OnInstanceActivated<TRequestedType>(IComponent<TAppCoreSelfReference> instance)
        {
         
        }
        #endregion

        #region Упрощение доступа
        /// <summary>
        /// Возвращает менеджер модулей для приложения.
        /// </summary>
        public ModulesManager<TAppCoreSelfReference> GetModulesManager()
        {
            return Get<ModulesManager<TAppCoreSelfReference>>();
        }

        /// <summary>
        /// Возвращает менеджер контекстов пользователя для приложения.
        /// </summary>
        public Users.UserContextManager<TAppCoreSelfReference> GetUserContextManager()
        {
            return Get<Users.UserContextManager<TAppCoreSelfReference>>();
        }

        private Modules.CoreModule.CoreModule<TAppCoreSelfReference> GetCoreModule()
        {
            return GetModulesManager().GetModule<Modules.CoreModule.CoreModule<TAppCoreSelfReference>>();
        }

        #endregion

        #region Управление настройками.
        /// <summary>
        /// Возвращает значение конфигурационной опции. Если значение не найдено, то возвращается <paramref name="defaultValue"/>.
        /// </summary>
        public T ConfigurationOptionGet<T>(string name, T defaultValue = default(T))
        {
            if (AppConfig.ContainsKey(name))
            {
                return AppConfig.Get<T>(name, defaultValue);
            }
            return defaultValue;
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Основные настройки приложения.
        /// </summary>
        public CoreConfiguration<TAppCoreSelfReference> AppConfig
        {
            get
            {
                if (_configurationAccessor == null) _configurationAccessor = GetCoreModule().GetConfiguration<CoreConfiguration<TAppCoreSelfReference>>();
                return _configurationAccessor;
            }
        }

        /// <summary>
        /// Возвращает рабочую директорию приложения. 
        /// </summary>
        public string ApplicationWorkingFolder { get; private set; }
        #endregion
    }
}
