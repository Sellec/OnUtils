using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Data;
    using Items;
    using ServiceMonitor;

    /// <summary>
    /// Предпочтительная базовая реализация сервиса отправки-приема сообщений для приложения.
    /// </summary>
    /// <typeparam name="TMessageType">Тип сообщения, с которым работает сервис.</typeparam>
    /// <typeparam name="TAppCoreSelfReference">Тип приложения, для работы с которым предназначен сервис.</typeparam>
    public abstract class ServiceBase<TAppCoreSelfReference, TMessageType> : CoreComponentBase<TAppCoreSelfReference>, IMonitoredService<TAppCoreSelfReference>, IMessagingServiceBackgroundOperations<TAppCoreSelfReference>, IUnitOfWorkAccessor<UnitOfWork<DB.MessageQueue, DB.MessageQueueHistory>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessageType : MessageBase, new()
    {
        /// <summary>
        /// Создает новый экземпляр сервиса.
        /// </summary>
        /// <param name="serviceName">Текстовое название сервиса.</param>
        /// <param name="serviceID">Уникальный идентификатор сервиса.</param>
        /// <param name="idMessageType">Идентификатор типа сообщения. Если не задан, то используется автоматически присваиваемый идентификатор. Предпочтительно не задавать значение.</param>
        protected ServiceBase(string serviceName, Guid serviceID, int? idMessageType = null)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));
            if (serviceID == null) throw new ArgumentNullException(nameof(serviceID));

            ServiceID = serviceID;
            ServiceName = serviceName;

            if (!idMessageType.HasValue) IdMessageType = Items.ItemTypeFactory.GetItemType(typeof(TMessageType)).IdItemType;
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            this.RegisterServiceState(ServiceStatus.RunningIdeal, "Сервис запущен.");
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            this.RegisterServiceState(ServiceStatus.Shutdown, "Сервис остановлен.");
        }
        #endregion

        #region Сообщения
        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> в очередь на отправку.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения. Текст ошибки </returns>
        [ApiReversible]
        protected bool RegisterMessage(TMessageType message)
        {
            try
            {
                // todo setError(null);

                using (var db = this.CreateUnitOfWork())
                {
                    var mess = new DB.MessageQueue()
                    {
                        IdMessageType = IdMessageType,
                        StateType = MessageStateType.NotProcessed,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message),
                    };

                    db.Repo1.Add(mess);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception)
            {
                // todo setError("Ошибка во время регистрации сообщения.", ex);
                return false;
            }
        }

        private List<IntermediateStateMessage<TMessageType>> GetUnsentMessages(UnitOfWork<DB.MessageQueue, DB.MessageQueueHistory> db)
        {
            var messages = db.Repo1.
                Where(x => x.IdMessageType == IdMessageType && (x.StateType == MessageStateType.NotProcessed || x.StateType == MessageStateType.RepeatWithControllerType)).
                ToList();

            var messagesUnserialized = messages.Select(x =>
            {
                try
                {
                    var str = x.MessageInfo;
                    return new IntermediateStateMessage<TMessageType>(Newtonsoft.Json.JsonConvert.DeserializeObject<TMessageType>(str), x);
                }
                catch (Exception ex)
                {
                    return new IntermediateStateMessage<TMessageType>(null, x) { StateType = MessageStateType.Error, State = ex.Message, DateChange = DateTime.Now };
                }
            }).ToList();

            return messagesUnserialized;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает список активных коннекторов, работающих с типом сообщений сервиса.
        /// </summary>
        /// <seealso cref="Configuration.CoreConfiguration{TAppCoreSelfReference}.ConnectorsSettings"/>
        protected List<Connectors.IConnectorBase<TAppCoreSelfReference, TMessageType>> GetConnectors()
        {
            return AppCore.Get<MessagingManager<TAppCoreSelfReference>>().GetConnectorsByMessageType<TMessageType>().ToList();
        }

        /// <summary>
        /// Возвращает количество неотправленных сообщений, с которыми работает сервис.
        /// </summary>
        /// <returns></returns>
        [ApiReversible]
        public virtual int GetOutcomingQueueLength()
        {
            using (var db = new UnitOfWork<DB.MessageQueue>())
            {
                return db.Repo1.Where(x => x.IdMessageType == IdMessageType && (x.StateType == MessageStateType.NotProcessed || x.StateType == MessageStateType.RepeatWithControllerType)).Count();
            }
        }
        #endregion

        #region Фоновые операции
        void IMessagingServiceBackgroundOperations<TAppCoreSelfReference>.ExecuteIncoming()
        {
        }

        void IMessagingServiceBackgroundOperations<TAppCoreSelfReference>.ExecuteOutcoming()
        {
            int messagesAll = 0;
            int messagesSent = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    var messages = GetUnsentMessages(db);
                    if (messages == null) return;

                    messagesAll = messages.Count;

                    OnBeforeExecuteOutcoming(messagesAll);

                    var processedMessages = new List<IntermediateStateMessage<TMessageType>>();

                    var time = new MeasureTime();
                    foreach (var intermediateMessage in messages)
                    {
                        if (intermediateMessage.StateType == MessageStateType.Error)
                        {
                            processedMessages.Add(intermediateMessage);
                            continue;
                        }

                        var connectors = GetConnectors().Select(x => new { Connector = x, IdTypeConnector = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType }).OrderBy(x => x.Connector.OrderInPool).ToList();
                        if (intermediateMessage.IdTypeConnector.HasValue)
                            connectors = connectors.Where(x => x.IdTypeConnector.HasValue && x.IdTypeConnector == intermediateMessage.IdTypeConnector).ToList();

                        foreach (var connectorInfo in connectors)
                        {
                            try
                            {
                                var connector = connectorInfo.Connector;
                                var connectorMessage = new ConnectorMessage<TMessageType>(intermediateMessage);
                                connector.Send(connectorMessage, this);
                                if (connectorMessage.HandledState != ConnectorMessageStateType.NotHandled)
                                {
                                    intermediateMessage.DateChange = DateTime.Now;
                                    switch (connectorMessage.HandledState)
                                    {
                                        case ConnectorMessageStateType.Error:
                                            intermediateMessage.StateType = MessageStateType.Error;
                                            intermediateMessage.State = connectorMessage.State;
                                            intermediateMessage.IdTypeConnector = null;
                                            break;

                                        case ConnectorMessageStateType.RepeatWithControllerType:
                                            intermediateMessage.StateType = MessageStateType.RepeatWithControllerType;
                                            intermediateMessage.State = connectorMessage.State;
                                            intermediateMessage.IdTypeConnector = connectorInfo.IdTypeConnector;
                                            break;

                                        case ConnectorMessageStateType.Sent:
                                            intermediateMessage.StateType = MessageStateType.Sent;
                                            intermediateMessage.State = null;
                                            intermediateMessage.IdTypeConnector = null;
                                            break;
                                    }
                                    processedMessages.Add(intermediateMessage);
                                    break;
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }

                        if (time.Calculate(false).TotalSeconds >= 3)
                        {
                            db.SaveChanges();
                            processedMessages.Clear();
                            time.Start();
                        }
                    }

                    if (processedMessages.Count > 0)
                    {
                        db.SaveChanges();
                    }

                    db.SaveChanges();
                    scope.Commit();
                }

                if (messagesAll > 0)
                {
                    this.RegisterServiceState(messagesErrors == 0 ? ServiceStatus.RunningIdeal : ServiceStatus.RunningWithErrors, $"Сообщений в очереди - {messagesAll}. Отправлено - {messagesSent}. Ошибки отправки - {messagesErrors}.");
                }

                var service = AppCore.Get<Monitor<TAppCoreSelfReference>>().GetService(ServiceID);
                if (service != null && (DateTime.Now - service.LastDateEvent).TotalHours >= 1)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Писем нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Messaging.ServiceBase.ExecuteOutcoming: {0}", ex.GetMessageExtended());
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений в очереди - {messagesAll}. Отправлено - {messagesSent}. Ошибки отправки - {messagesErrors}.", ex);
            }
        }

        /// <summary>
        /// Вызывается перед началом отправки сообщений.
        /// </summary>
        protected virtual void OnBeforeExecuteOutcoming(int messagesCount)
        {

        }

        private void TryToSendThrougnConnectors(IntermediateStateMessage<TMessageType> messageBody)
        {
        }

        #endregion

        #region Свойства
        public virtual int IdMessageType { get; set; }

        public Guid ServiceID
        {
            get;
            private set;
        }

        public string ServiceName
        {
            get;
            private set;
        }

        #region IMonitoredService
        /// <summary>
        /// См. <see cref="IMonitoredService.ServiceStatus"/>.
        /// </summary>
        public virtual ServiceStatus ServiceStatus
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService.ServiceStatusDetailed"/>.
        /// </summary>
        public virtual string ServiceStatusDetailed
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService.IsSupportsCurrentStatusInfo"/>.
        /// </summary>
        public virtual bool IsSupportsCurrentStatusInfo
        {
            get;
            protected set;
        }
        #endregion

        #region IMessagingServiceBackgroundOperations
        /// <summary>
        /// См. <see cref="IMessagingService.IsSupportsIncoming"/>.
        /// </summary>
        public virtual bool IsSupportsIncoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMessagingService.IsSupportsIncoming"/>.
        /// </summary>
        public virtual bool IsSupportsOutcoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMessagingService.IsSupportsIncoming"/>.
        /// </summary>
        public virtual int OutcomingQueueLength
        {
            get;
            protected set;
        }
        #endregion
        #endregion
    }
}
