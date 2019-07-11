﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;
    using Connectors;
    using OnUtils.Types;
    using Tasks;

    /// <summary>
    /// Представляет менеджер, управляющий обменом сообщениями - уведомления, электронная почта, смс и прочее.
    /// </summary>
    public class MessagingManager : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>, IAutoStart
    {
        class InstanceActivatedHandlerImpl : IInstanceActivatedHandler
        {
            private readonly MessagingManager _manager;

            public InstanceActivatedHandlerImpl(MessagingManager manager)
            {
                _manager = manager;
            }

            void IInstanceActivatedHandler.OnInstanceActivated<TRequestedType>(object instance)
            {
                if (instance is IMessagingService service)
                {
                    if (!_manager._services.Contains(service)) _manager._services.Add(service);
                }
            }
        }

        class MessageFake : MessageBase
        {

        }

        private static MethodInfo _connectorCreateCall = null;
        private static ApplicationCore _appCore = null;
        private volatile bool _incomingLock = false;
        private volatile bool _outcomingLock = false;

        private readonly InstanceActivatedHandlerImpl _instanceActivatedHandler = null;
        private List<IMessagingService> _services = new List<IMessagingService>();

        private object _activeConnectorsSyncRoot = new object();
        private List<IComponentTransient<ApplicationCore>> _activeConnectors = null;

        static MessagingManager()
        {
            _connectorCreateCall = typeof(MessagingManager).GetMethod(nameof(InitConnector), BindingFlags.NonPublic | BindingFlags.Instance);
            if (_connectorCreateCall == null) throw new TypeInitializationException(typeof(MessagingManager).FullName, new Exception($"Ошибка поиска метода '{nameof(InitConnector)}'"));
        }

        public MessagingManager()
        {
            _instanceActivatedHandler = new InstanceActivatedHandlerImpl(this);
        }

        #region CoreComponentBase
        protected sealed override void OnStart()
        {
            _appCore = AppCore;
            AppCore.ObjectProvider.RegisterInstanceActivatedHandler(_instanceActivatedHandler);

            // Попытка инициализировать все сервисы отправки сообщений, наследующиеся от IMessagingService.
            var types = AppCore.GetQueryTypes().Where(x => x.GetInterfaces().Contains(typeof(IMessagingService))).ToList();
            foreach (var type in types)
            {
                try
                {
                    var instance = AppCore.Get<IMessagingService>(type);
                    if (instance != null && !_services.Contains(instance)) _services.Add(instance);
                }
                catch { }
            }

            TasksManager.SetTask(typeof(MessagingManager).FullName + "_" + nameof(PrepareIncoming) + "_minutely1", Cron.MinuteInterval(1), () => PrepareIncomingTasks());
            TasksManager.SetTask(typeof(MessagingManager).FullName + "_" + nameof(PrepareOutcoming) + "_minutely1", Cron.MinuteInterval(1), () => PrepareOutcomingTasks());
        }

        protected sealed override void OnStop()
        {
            TasksManager.RemoveTask(typeof(MessagingManager).FullName + "_" + nameof(PrepareIncoming) + "_minutely1");
            TasksManager.RemoveTask(typeof(MessagingManager).FullName + "_" + nameof(PrepareOutcoming) + "_minutely1");
        }
        #endregion

        #region Методы
        #region Отправка/получение
        public static void PrepareIncomingTasks()
        {
            _appCore?.Get<MessagingManager>()?.PrepareIncoming();
        }

        public void PrepareIncoming()
        {
            if (_incomingLock) return;

            try
            {
                _incomingLock = true;

                foreach (var provider in _services.Where(x => x.IsSupportsIncoming && x is IMessagingServiceBackgroundOperations).Select(x => x as IMessagingServiceBackgroundOperations))
                {
                    try
                    {
                        provider.ExecuteIncoming();
                    }
                    catch (Exception ex) { Debug.WriteLine("Ошибка обработки входящих сообщений для сервиса '{0}': {1}", provider.ServiceName, ex.Message); }
                }

            }
            catch (Exception ex) { Debug.WriteLine("Ошибка обработки входящих сообщений: {0}", ex.Message); }
            finally { _incomingLock = false; }
        }

        public static void PrepareOutcomingTasks()
        {
            _appCore?.Get<MessagingManager>()?.PrepareOutcoming();
        }

        public void PrepareOutcoming()
        {
            if (_outcomingLock) return;

            try
            {
                _outcomingLock = true;
                foreach (var provider in _services.Where(x => x.IsSupportsOutcoming && x is IMessagingServiceBackgroundOperations).Select(x => x as IMessagingServiceBackgroundOperations))
                {
                    try
                    {
                        provider.ExecuteOutcoming();
                    }
                    catch (Exception ex) { Debug.WriteLine("Ошибка обработки исходящих сообщений для сервиса '{0}': {1}", provider.ServiceName, ex.GetMessageExtended()); }
                }

            }
            catch (Exception ex) { Debug.WriteLine("Ошибка обработки исходящих сообщений: {0}", ex.GetMessageExtended()); }
            finally { _outcomingLock = false; }
        }
        #endregion

        /// <summary>
        /// Возвращает список коннекторов, поддерживающих обмен сообщениями указанного типа <typeparamref name="TMessage"/>.
        /// </summary>
        public IEnumerable<IConnectorBase<TMessage>> GetConnectorsByMessageType<TMessage>() where TMessage : MessageBase, new()
        {
            lock (_activeConnectorsSyncRoot)
                if (_activeConnectors == null)
                    UpdateConnectorsFromSettings();

            return _activeConnectors.OfType<IConnectorBase<TMessage>>();
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
        public IEnumerable<IMessagingService> GetMessagingServices()
        {
            return _services.ToList();
        }

        /// <summary>
        /// Пересоздает текущий используемый список коннекторов с учетом настроек коннекторов. Рекомендуется к использованию в случае изменения настроек коннекторов.
        /// </summary>
        public void UpdateConnectorsFromSettings()
        {
            lock (_activeConnectorsSyncRoot)
            {
                if (_activeConnectors != null)
                    _activeConnectors.ForEach(x =>
                    {
                        try
                        {
                            x.Stop();
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(Journaling.EventType.Error, "Ошибка при закрытии коннектора", $"Возникла ошибка при выгрузке коннектора типа '{x.GetType().FullName}'.", null, ex);
                        }
                    });

                _activeConnectors = new List<IComponentTransient<ApplicationCore>>();

                var connectorsSettings = AppCore.Config.ConnectorsSettings;
                if (connectorsSettings != null)
                {
                    var types = AppCore.
                        GetQueryTypes().
                        Select(x => new { Type = x, Extracted = TypeHelpers.ExtractGenericInterface(x, typeof(IConnectorBase<>)) }).
                        Where(x => x.Extracted != null).
                        Select(x => new { x.Type, MessageType = x.Extracted.GetGenericArguments()[0] }).
                        ToList();

                    foreach (var setting in connectorsSettings)
                    {
                        var connectorType = types.FirstOrDefault(x => x.Type.FullName == setting.ConnectorTypeName);
                        if (connectorType == null)
                        {
                            this.RegisterEvent(Journaling.EventType.Error, "Ошибка при поиске коннектора", $"Не найден тип коннектора из настроек - '{setting.ConnectorTypeName}'. Для стирания старых настроек следует зайти в настройку коннекторов и сделать сохранение.");
                            continue;
                        }

                        try
                        {
                            var connector = AppCore.Create<IComponentTransient<ApplicationCore>>(connectorType.Type);
                            var initResult = (bool)_connectorCreateCall.MakeGenericMethod(connectorType.MessageType).Invoke(this, new object[] { connector, setting.SettingsSerialized });
                            if (!initResult)
                            {
                                this.RegisterEvent(Journaling.EventType.Error, "Отказ инициализации коннектора", $"Коннектор типа '{setting.ConnectorTypeName}' ('{connector.GetType().FullName}') вернул отказ инициализации. См. журналы ошибок для поиска возможной информации.");
                                continue;
                            }

                            _activeConnectors.Add(connector);
                        }
                        catch (Exception ex)
                        {
                            this.RegisterEvent(Journaling.EventType.Error, "Ошибка создания коннектора", $"Во время создания и инициализации коннектора типа '{setting.ConnectorTypeName}' возникла неожиданная ошибка.", null, ex.InnerException);
                        }
                    }
                }
            }
        }

        private bool InitConnector<TMessage>(IConnectorBase<TMessage> connector, string serializedSettings) where TMessage : MessageBase, new()
        {
            return connector.Init(serializedSettings);
        }
        #endregion
    }
}
