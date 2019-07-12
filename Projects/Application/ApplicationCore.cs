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
    public abstract class ApplicationCore : AppCore<ApplicationCore> 
    {
        private CoreConfiguration _configurationAccessor = null;

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
        protected override void OnBindingsRequired(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ApplicationLauncher>();
            bindingsCollection.SetSingleton<Items.ItemsManager>();
            bindingsCollection.SetSingleton<Journaling.JournalingManager>();
            bindingsCollection.SetSingleton<Messaging.MessagingManager>();
            bindingsCollection.SetSingleton<Languages.Manager>();
            bindingsCollection.SetSingleton<Modules.ModulesManager>();
            bindingsCollection.SetSingleton<Modules.ModulesLoadStarter>();
            bindingsCollection.SetSingleton<ServiceMonitor.Monitor>();
            bindingsCollection.SetSingleton<Users.UserContextManager>();
        }

        /// <summary>
        /// См. <see cref="AppCore{TAppCore}.OnInstanceActivated{TRequestedType}(IComponent{TAppCore})"/>.
        /// </summary>
        protected override void OnInstanceActivated<TRequestedType>(IComponent<ApplicationCore> instance)
        {
         
        }
        #endregion

        #region Упрощение доступа
        /// <summary>
        /// Возвращает менеджер модулей для приложения.
        /// </summary>
        public ModulesManager GetModulesManager()
        {
            return Get<ModulesManager>();
        }

        /// <summary>
        /// Возвращает менеджер контекстов пользователя для приложения.
        /// </summary>
        public Users.UserContextManager GetUserContextManager()
        {
            return Get<Users.UserContextManager>();
        }

        private Modules.CoreModule.CoreModule GetCoreModule()
        {
            return GetModulesManager().GetModule<Modules.CoreModule.CoreModule>();
        }

        #endregion

        #region Управление настройками.
        /// <summary>
        /// Возвращает значение конфигурационной опции. Если значение не найдено, то возвращается <paramref name="defaultValue"/>.
        /// </summary>
        public T ConfigurationOptionGet<T>(string name, T defaultValue = default(T))
        {
            if (Config.ContainsKey(name))
            {
                return Config.Get<T>(name, defaultValue);
            }
            return defaultValue;
        }
        #endregion

        #region Свойства


        /// <summary>
        /// Основные настройки приложения.
        /// </summary>
        public CoreConfiguration Config
        {
            get
            {
                if (_configurationAccessor == null) _configurationAccessor = GetCoreModule().GetConfiguration<CoreConfiguration>();
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
