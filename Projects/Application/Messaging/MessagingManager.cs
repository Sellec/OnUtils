using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;
    using MessageHandlers;
    using Journaling;
    using OnUtils.Types;

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

        private static MethodInfo _handlerCreateCall = null;
        private static ApplicationCore<TAppCoreSelfReference> _appCore = null;
        private volatile bool _incomingLock = false;
        private volatile bool _outcomingLock = false;

        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;
        private List<IMessageServiceInternal<TAppCoreSelfReference>> _services = new List<IMessageServiceInternal<TAppCoreSelfReference>>();

        private object _activeHandlersSyncRoot = new object();
        private List<IComponentTransient<TAppCoreSelfReference>> _activeHandlers = null;

        static MessagingManager()
        {
            _handlerCreateCall = typeof(MessagingManager<TAppCoreSelfReference>).GetMethod(nameof(InitHandler), BindingFlags.NonPublic | BindingFlags.Instance);
            if (_handlerCreateCall == null) throw new TypeInitializationException(typeof(MessagingManager<TAppCoreSelfReference>).FullName, new Exception($"Ошибка поиска метода '{nameof(InitHandler)}'"));
        }

        /// <summary>
        /// </summary>
        public MessagingManager()
        {
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl(this);
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
        internal static void CallServiceIncoming(Type serviceType)
        {
            var service = _appCore.Get<MessagingManager<TAppCoreSelfReference>>()._services.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareIncoming();
        }

        internal static void CallServiceOutcoming(Type serviceType)
        {
            var service = _appCore.Get<MessagingManager<TAppCoreSelfReference>>()._services.FirstOrDefault(x => x.GetType() == serviceType);
            service?.PrepareOutcoming();
        }

        /// <summary>
        /// Возвращает список обработчиков, поддерживающих обмен сообщениями указанного типа <typeparamref name="TMessage"/>.
        /// </summary>
        public IEnumerable<IMessageHandler<TAppCoreSelfReference, TMessage>> GetHandlersByMessageType<TMessage>() where TMessage : MessageBase, new()
        {
            lock (_activeHandlersSyncRoot)
                if (_activeHandlers == null)
                    UpdateHandlersFromSettings();

            return _activeHandlers.OfType<IMessageHandler<TAppCoreSelfReference, TMessage>>();
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
        /// Пересоздает текущий используемый список обработчиков с учетом настроек. Рекомендуется к использованию в случае изменения настроек.
        /// </summary>
        /// <see cref="Configuration.CoreConfiguration{TAppCoreSelfReference}.MessageHandlersSettings"/>
        public void UpdateHandlersFromSettings()
        {
            lock (_activeHandlersSyncRoot)
            {
                if (_activeHandlers != null)
                    _activeHandlers.ForEach(x =>
                    {
                        try
                        {
                            x.Stop();
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при закрытии обработчика", $"Возникла ошибка при выгрузке обработчика типа '{x.GetType().FullName}'.", null, ex);
                        }
                    });

                _activeHandlers = new List<IComponentTransient<TAppCoreSelfReference>>();

                var handlersSettings = AppCore.AppConfig.MessageHandlersSettings;
                if (handlersSettings != null)
                {
                    var types = AppCore.
                        GetQueryTypes().
                        Select(x => new { Type = x, Extracted = TypeHelpers.ExtractGenericInterface(x, typeof(IMessageHandler<,>)) }).
                        Where(x => x.Extracted != null).
                        Select(x => new { x.Type, MessageType = x.Extracted.GetGenericArguments()[1] }).
                        ToList();

                    foreach (var setting in handlersSettings)
                    {
                        var handlerType = types.FirstOrDefault(x => x.Type.FullName == setting.TypeFullName);
                        if (handlerType == null)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка при поиске обработчика", $"Не найден тип обработчика из настроек - '{setting.TypeFullName}'. Для стирания старых настроек следует зайти в настройку обработчиков и сделать сохранение.");
                            continue;
                        }

                        try
                        {
                            var handler = AppCore.Create<IComponentTransient<TAppCoreSelfReference>>(handlerType.Type);
                            var initResult = (bool)_handlerCreateCall.MakeGenericMethod(handlerType.MessageType).Invoke(this, new object[] { handler, setting.SettingsSerialized });
                            if (!initResult)
                            {
                                this.RegisterEvent(EventType.Error, "Отказ инициализации обработчика", $"Обработчик типа '{setting.TypeFullName}' ('{handler.GetType().FullName}') вернул отказ инициализации. См. журналы ошибок для поиска возможной информации.");
                                continue;
                            }

                            _activeHandlers.Add(handler);
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(EventType.Error, "Ошибка создания обработчика", $"Во время создания и инициализации обработчика типа '{setting.TypeFullName}' возникла неожиданная ошибка.", null, ex.InnerException);
                        }
                    }
                }
            }
        }

        private bool InitHandler<TMessage>(IMessageHandler<TAppCoreSelfReference, TMessage> handler, string serializedSettings) where TMessage : MessageBase, new()
        {
            return handler.Init(serializedSettings);
        }
        #endregion
    }
}
