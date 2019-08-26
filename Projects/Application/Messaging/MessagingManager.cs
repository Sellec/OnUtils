using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;
    using Components;
    using Journaling;
    using OnUtils.Types;
    using Messages;

    /// <summary>
    /// Представляет менеджер, управляющий обменом сообщениями - уведомления, электронная почта, смс и прочее.
    /// </summary>
    public sealed class MessagingManager<TAppCoreSelfReference> : 
        CoreComponentBase<TAppCoreSelfReference>, 
        IComponentSingleton<TAppCoreSelfReference>, 
        IAutoStart,
        ITypedJournalComponent<MessagingManager<TAppCoreSelfReference>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly MessagingManager<TAppCoreSelfReference> _manager;

            public InstanceActivatedHandlerImpl(MessagingManager<TAppCoreSelfReference> manager)
            {
                _manager = manager;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is IMessageServiceInternal<TAppCoreSelfReference> service)
                {
                    if (!_manager._services.Contains(service)) _manager._services.Add(service);
                }
            }
        }

        private static MethodInfo _componentCreateCall = null;
        private static ApplicationCore<TAppCoreSelfReference> _appCore = null;

        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;
        private List<IMessageServiceInternal<TAppCoreSelfReference>> _services = new List<IMessageServiceInternal<TAppCoreSelfReference>>();

        private object _activeComponentsSyncRoot = new object();
        private List<IComponentTransient<TAppCoreSelfReference>> _activeComponents = null;
        private List<IComponentTransient<TAppCoreSelfReference>> _registeredComponents = null;

        static MessagingManager()
        {
            _componentCreateCall = typeof(MessagingManager<TAppCoreSelfReference>).GetMethod(nameof(InitComponent), BindingFlags.NonPublic | BindingFlags.Instance);
            if (_componentCreateCall == null) throw new TypeInitializationException(typeof(MessagingManager<TAppCoreSelfReference>).FullName, new Exception($"Ошибка поиска метода '{nameof(InitComponent)}'"));
        }

        /// <summary>
        /// </summary>
        public MessagingManager()
        {
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl(this);
            _registeredComponents = new List<IComponentTransient<TAppCoreSelfReference>>();
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            this.RegisterJournal("Менеджер сообщений");

            _appCore = AppCore;
            AppCore.ObjectProvider.RegisterInstanceActivatedHandler(_instanceActivatedHandler);

            // Попытка инициализировать все сервисы отправки сообщений, наследующиеся от IMessagingService.
            var types = AppCore.GetQueryTypes().Where(x => x.GetInterfaces().Contains(typeof(IMessageServiceInternal<TAppCoreSelfReference>))).ToList();
            foreach (var type in types)
            {
                try
                {
                    var instance = AppCore.Get<IMessageServiceInternal<TAppCoreSelfReference>>(type);
                    if (instance != null && !_services.Contains(instance)) _services.Add(instance);
                }
                catch { }
            }
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Методы
        internal static void CallServiceIncomingHandle(Type serviceType)
        {
            var service = _appCore.Get<MessagingManager<TAppCoreSelfReference>>()._services.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareIncomingHandle();
        }

        internal static void CallServiceIncomingReceive(Type serviceType)
        {
            var service = _appCore.Get<MessagingManager<TAppCoreSelfReference>>()._services.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareIncomingReceive();
        }

        internal static void CallServiceOutcoming(Type serviceType)
        {
            var service = _appCore.Get<MessagingManager<TAppCoreSelfReference>>()._services.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareOutcoming();
        }

        /// <summary>
        /// Регистрирует новый компонент сервиса обработки сообщений.
        /// </summary>
        public void RegisterComponent<TMessage>(IMessageServiceComponent<TAppCoreSelfReference, TMessage> component)
            where TMessage : MessageBase, new()
        {
            if (component == null) return;
            if (_registeredComponents.Contains(component)) return;
            _registeredComponents.Add(component);
        }

        /// <summary>
        /// Возвращает список компонентов, поддерживающих обмен сообщениями указанного типа <typeparamref name="TMessage"/>.
        /// </summary>
        public IEnumerable<IMessageServiceComponent<TAppCoreSelfReference, TMessage>> GetComponentsByMessageType<TMessage>() where TMessage : MessageBase, new()
        {
            lock (_activeComponentsSyncRoot)
                if (_activeComponents == null)
                    UpdateComponentsFromSettings();

            var active = _activeComponents.OfType<IMessageServiceComponent<TAppCoreSelfReference, TMessage>>();
            var registered = _registeredComponents.OfType<IMessageServiceComponent<TAppCoreSelfReference, TMessage>>();

            return active.Union(registered);
        }

        /// <summary>
        /// Возвращает список сервисов-получателей критических сообщений.
        /// </summary>
        public IEnumerable<ICriticalMessagesReceiver> GetCriticalMessagesReceivers()
        {
            return _services.OfType<ICriticalMessagesReceiver>();
        }

        /// <summary>
        /// Возвращает список сервисов обмена сообщениями.
        /// </summary>
        public IEnumerable<IMessageService<TAppCoreSelfReference>> GetMessagingServices()
        {
            return _services.OfType<IMessageService<TAppCoreSelfReference>>().ToList();
        }

        /// <summary>
        /// Пересоздает текущий используемый список компонентов с учетом настроек. Рекомендуется к использованию в случае изменения настроек.
        /// </summary>
        /// <see cref="Configuration.CoreConfiguration{TAppCoreSelfReference}.MessageServicesComponentsSettings"/>
        public void UpdateComponentsFromSettings()
        {
            lock (_activeComponentsSyncRoot)
            {
                if (_activeComponents != null)
                    _activeComponents.ForEach(x =>
                    {
                        try
                        {
                            x.Stop();
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при закрытии компонента", $"Возникла ошибка при выгрузке компонента типа '{x.GetType().FullName}'.", null, ex);
                        }
                    });

                _activeComponents = new List<IComponentTransient<TAppCoreSelfReference>>();

                var settings = AppCore.AppConfig.MessageServicesComponentsSettings;
                if (settings != null)
                {
                    var types = AppCore.
                        GetQueryTypes().
                        Select(x => new { Type = x, Extracted = TypeHelpers.ExtractGenericInterface(x, typeof(IMessageServiceComponent<,>)) }).
                        Where(x => x.Extracted != null).
                        Select(x => new { x.Type, MessageType = x.Extracted.GetGenericArguments()[1] }).
                        ToList();

                    foreach (var setting in settings)
                    {
                        var type = types.FirstOrDefault(x => x.Type.FullName == setting.TypeFullName);
                        if (type == null)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при поиске компонента", $"Не найден тип компонента из настроек - '{setting.TypeFullName}'. Для стирания старых настроек следует зайти в настройку компонентов и сделать сохранение.");
                            continue;
                        }

                        try
                        {
                            var instance = AppCore.Create<IComponentTransient<TAppCoreSelfReference>>(type.Type);
                            var initResult = (bool)_componentCreateCall.MakeGenericMethod(type.MessageType).Invoke(this, new object[] { instance, setting.SettingsSerialized });
                            if (!initResult)
                            {
                                this.RegisterEvent(EventType.Error, "Отказ инициализации компонента", $"Компонент типа '{setting.TypeFullName}' ('{instance.GetType().FullName}') вернул отказ инициализации. См. журналы ошибок для поиска возможной информации.");
                                continue;
                            }

                            _activeComponents.Add(instance);
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка создания компонента", $"Во время создания и инициализации компонента типа '{setting.TypeFullName}' возникла неожиданная ошибка.", null, ex.InnerException);
                        }
                    }
                }
            }
        }

        private bool InitComponent<TMessage>(IMessageServiceComponent<TAppCoreSelfReference, TMessage> instance, string serializedSettings) where TMessage : MessageBase, new()
        {
            return instance.Init(serializedSettings);
        }
        #endregion
    }
}
