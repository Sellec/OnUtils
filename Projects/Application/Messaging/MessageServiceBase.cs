using OnUtils.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Data;
    using Items;
    using MessageHandlers;
    using ServiceMonitor;

    /// <summary>
    /// Предпочтительная базовая реализация сервиса отправки-приема сообщений для приложения.
    /// </summary>
    /// <typeparam name="TMessageType">Тип сообщения, с которым работает сервис.</typeparam>
    /// <typeparam name="TAppCoreSelfReference">Тип приложения, для работы с которым предназначен сервис.</typeparam>
    public abstract class MessageServiceBase<TAppCoreSelfReference, TMessageType> : 
        CoreComponentBase<TAppCoreSelfReference>,
        IMessageService<TAppCoreSelfReference>,
        IMessageServiceInternal<TAppCoreSelfReference>,
        IUnitOfWorkAccessor<DB.DataContext>,
        IAutoStart
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TMessageType : MessageBase, new()
    {
        private ConcurrentDictionary<string, int> _executingFlags = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Создает новый экземпляр сервиса.
        /// </summary>
        /// <param name="serviceName">Текстовое название сервиса.</param>
        /// <param name="serviceID">Уникальный идентификатор сервиса.</param>
        protected MessageServiceBase(string serviceName, Guid serviceID)
        {
            if (string.IsNullOrEmpty(serviceName)) throw new ArgumentNullException(nameof(serviceName));
            if (serviceID == null) throw new ArgumentNullException(nameof(serviceID));

            ServiceID = serviceID;
            ServiceName = serviceName;

            IdMessageType = Items.ItemTypeFactory.GetItemType(typeof(TMessageType)).IdItemType;
        }

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            this.RegisterServiceState(ServiceStatus.RunningIdeal, "Сервис запущен.");

            var type = GetType();
            TasksManager.SetTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncoming) + "_minutely1", Cron.MinuteInterval(1), () => MessagingManager<TAppCoreSelfReference>.CallServiceIncoming(type));
            TasksManager.SetTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming) + "_minutely1", Cron.MinuteInterval(1), () => MessagingManager<TAppCoreSelfReference>.CallServiceOutcoming(type));
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            this.RegisterServiceState(ServiceStatus.Shutdown, "Сервис остановлен.");

            var type = GetType();
            TasksManager.RemoveTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncoming) + "_minutely1");
            TasksManager.RemoveTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming) + "_minutely1");
            TasksManager.RemoveTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming) + "_immediately");
        }
        #endregion

        #region Сообщения
        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> в очередь на отправку.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения. Текст ошибки </returns>
        [ApiReversible]
        protected bool RegisterOutcomingMessage(TMessageType message)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                {
                    var mess = new DB.MessageQueue()
                    {
                        IdMessageType = IdMessageType,
                        StateType = MessageStateType.NotProcessed,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message),
                    };

                    db.MessageQueue.Add(mess);
                    db.SaveChanges();
                    if (_executingFlags.AddOrUpdate(nameof(RegisterOutcomingMessage), 1, (k, o) => Math.Min(int.MaxValue, o + 1)) == 1)
                    {
                        var type = GetType();
                        TasksManager.SetTask(type.FullName + "_" + nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming) + "_immediately", DateTime.Now.AddSeconds(5), () => MessagingManager<TAppCoreSelfReference>.CallServiceOutcoming(type));
                        Debug.WriteLineNoLog($"Messaging: {type.FullName} planning immediately - success");
                    }
                    else
                    {
                        var type = GetType();
                        Debug.WriteLineNoLog($"Messaging: {type.FullName} planning immediately - restrict");
                    }

                    return true;
                }
            }
            catch (Exception)
            {
                // todo setError("Ошибка во время регистрации сообщения.", ex);
                return false;
            }
        }

        private List<IntermediateStateMessage<TMessageType>> GetUnsentMessages(DB.DataContext db)
        {
            var messages = db.MessageQueue.
                Where(x => !x.Direction && x.IdMessageType == IdMessageType && (x.StateType == MessageStateType.NotProcessed || x.StateType == MessageStateType.RepeatWithControllerType)).
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
        /// Возвращает список активных обработчиков, работающих с типом сообщений сервиса.
        /// </summary>
        /// <seealso cref="Configuration.CoreConfiguration{TAppCoreSelfReference}.MessageHandlersSettings"/>
        protected List<IMessageHandler<TAppCoreSelfReference, TMessageType>> GetHandlers()
        {
            return AppCore.Get<MessagingManager<TAppCoreSelfReference>>().GetHandlersByMessageType<TMessageType>().ToList();
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

        #region IInternalForTasks
        void IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncoming()
        {
            var type = GetType();

            if (_executingFlags.AddOrUpdate(nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncoming), 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1)
            {
                Debug.WriteLineNoLog($"Messaging: {type.FullName} task2 - restrict");
                return;
            }
            Debug.WriteLineNoLog($"Messaging: {type.FullName} task2 - working start");

            int messagesReceived = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    var handlers = GetHandlers().
                    OfType<IMessageReceiver<TAppCoreSelfReference, TMessageType>>().
                    Select(x => new {
                        Handler = x,
                        IdTypeHandler = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                    }).
                    OrderBy(x => x.Handler.OrderInPool).
                    ToList();

                    foreach (var handlerInfo in handlers)
                    {
                        try
                        {
                            var messages = handlerInfo.Handler.Receive(this);
                            if (messages != null && messages.Count > 0)
                            {
                                int countAdded = 0;
                                foreach (var message in messages)
                                {
                                    var mess = new DB.MessageQueue()
                                    {
                                        IdMessageType = IdMessageType,
                                        Direction = true,
                                        State = message.State,
                                        StateType = message.StateType,
                                        DateCreate = DateTime.Now,
                                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message.MessageBody),
                                    };

                                    db.MessageQueue.Add(mess);
                                    countAdded++;
                                    messagesReceived++;

                                    if (countAdded >= 50)
                                    {
                                        db.SaveChanges();
                                        countAdded = 0;
                                    }
                                }

                                db.SaveChanges();
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }

                    db.SaveChanges();
                    scope.Commit();
                }

                if (messagesReceived > 0)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений получено - {messagesReceived}.");
                }

                var service = AppCore.Get<Monitor<TAppCoreSelfReference>>().GetService(ServiceID);
                if (service != null && (DateTime.Now - service.LastDateEvent).TotalHours >= 1)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Писем нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Messaging.ServiceBase.PrepareIncoming: {0}", ex.GetMessageExtended());
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений получено - {messagesReceived}.", ex);
            }
            finally
            {
                Debug.WriteLineNoLog($"Messaging: {type.FullName} task2 - working end");
                _executingFlags[nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncoming)] = 0;
            }
        }

        void IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming()
        {
            var type = GetType();

            if (_executingFlags.AddOrUpdate(nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming), 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1)
            {
                Debug.WriteLineNoLog($"Messaging: {type.FullName} task - restrict");
                return;
            }
            Debug.WriteLineNoLog($"Messaging: {type.FullName} task - working start");

            int messagesAll = 0;
            int messagesSent = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    _executingFlags[nameof(RegisterOutcomingMessage)] = 0;
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

                        var handlers = GetHandlers().
                            OfType<IMessageSender<TAppCoreSelfReference, TMessageType>>().
                            Select(x => new {
                                Handler = x,
                                IdTypeHandler = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                            }).
                            OrderBy(x => x.Handler.OrderInPool).
                            ToList();

                        if (intermediateMessage.IdTypeHandler.HasValue)
                        {
                            handlers = handlers.Where(x => x.IdTypeHandler.HasValue && x.IdTypeHandler == intermediateMessage.IdTypeHandler).ToList();
                        }

                        foreach (var handlerInfo in handlers)
                        {
                            try
                            {
                                var handler = handlerInfo.Handler;
                                var handlerMessage = new HandlerMessage<TMessageType>(intermediateMessage);
                                handler.Send(handlerMessage, this);
                                if (handlerMessage.HandledState != HandlerMessageStateType.NotHandled)
                                {
                                    intermediateMessage.DateChange = DateTime.Now;
                                    switch (handlerMessage.HandledState)
                                    {
                                        case HandlerMessageStateType.Error:
                                            intermediateMessage.StateType = MessageStateType.Error;
                                            intermediateMessage.State = handlerMessage.State;
                                            intermediateMessage.IdTypeHandler = null;
                                            break;

                                        case HandlerMessageStateType.RepeatWithControllerType:
                                            intermediateMessage.StateType = MessageStateType.RepeatWithControllerType;
                                            intermediateMessage.State = handlerMessage.State;
                                            intermediateMessage.IdTypeHandler = handlerInfo.IdTypeHandler;
                                            break;

                                        case HandlerMessageStateType.Completed:
                                            intermediateMessage.StateType = MessageStateType.Complete;
                                            intermediateMessage.State = null;
                                            intermediateMessage.IdTypeHandler = null;
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
            finally
            {
                Debug.WriteLineNoLog($"Messaging: {type.FullName} task - working end");
                _executingFlags[nameof(IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming)] = 0;
            }
        }
        #endregion

        #region Фоновые операции
        /// <summary>
        /// Вызывается перед началом отправки сообщений.
        /// </summary>
        protected virtual void OnBeforeExecuteOutcoming(int messagesCount)
        {

        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор типа сообщения.
        /// </summary>
        public int IdMessageType { get; }

        /// <summary>
        /// Возвращает идентификатор сервиса.
        /// </summary>
        public Guid ServiceID
        {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает название сервиса.
        /// </summary>
        public string ServiceName
        {
            get;
            private set;
        }

        #region IMonitoredService
        /// <summary>
        /// См. <see cref="IMonitoredService{TAppCoreSelfReference}.ServiceStatus"/>.
        /// </summary>
        public virtual ServiceStatus ServiceStatus
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService{TAppCoreSelfReference}.ServiceStatusDetailed"/>.
        /// </summary>
        public virtual string ServiceStatusDetailed
        {
            get;
            protected set;
        }

        /// <summary>
        /// См. <see cref="IMonitoredService{TAppCoreSelfReference}.IsSupportsCurrentStatusInfo"/>.
        /// </summary>
        public virtual bool IsSupportsCurrentStatusInfo
        {
            get;
            protected set;
        }
        #endregion

        #region IMessagingServiceBackgroundOperations
        /// <summary>
        /// Указывает, что сервис поддерживает прием сообщений.
        /// </summary>
        public virtual bool IsSupportsIncoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// Указывает, что сервис поддерживает отправку сообщений.
        /// </summary>
        public virtual bool IsSupportsOutcoming
        {
            get;
            protected set;
        }

        /// <summary>
        /// Возвращает длину очереди на отправку сообщений.
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
