using OnUtils.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using OnUtils.Architecture.ObjectPool;

namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Data;
    using Items;
    using Components;
    using ServiceMonitor;
    using Messages;

    /// <summary>
    /// Предпочтительная базовая реализация сервиса обработки сообщений для приложения.
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
        private readonly string TasksOutcomingSend;
        private readonly string TasksIncomingReceive;
        private readonly string TasksIncomingHandle;

        private ConcurrentDictionary<string, int> _executingFlags = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// Создает новый экземпляр сервиса.
        /// </summary>
        /// <param name="serviceName">Текстовое название сервиса.</param>
        /// <param name="serviceID">Уникальный идентификатор сервиса.</param>
        protected MessageServiceBase(string serviceName, Guid serviceID)
        {
            var type = GetType();
            TasksOutcomingSend = type.FullName + "_" + nameof(TasksOutcomingSend);
            TasksIncomingReceive = type.FullName + "_" + nameof(TasksIncomingReceive);
            TasksIncomingHandle = type.FullName + "_" + nameof(TasksIncomingHandle);

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
            TasksManager.SetTask(TasksOutcomingSend + "_minutely1", Cron.MinuteInterval(1), () => MessagingManager<TAppCoreSelfReference>.CallServiceOutcoming(type));
            TasksManager.SetTask(TasksIncomingReceive + "_minutely1", Cron.MinuteInterval(1), () => MessagingManager<TAppCoreSelfReference>.CallServiceIncomingReceive(type));
            TasksManager.SetTask(TasksIncomingHandle + "_minutely1", Cron.MinuteInterval(1), () => MessagingManager<TAppCoreSelfReference>.CallServiceIncomingHandle(type));

            _executingFlags.AddOrUpdate(nameof(RegisterOutcomingMessage), 1, (k, o) => Math.Min(int.MaxValue, o + 1));
            TasksManager.SetTask(TasksOutcomingSend + "_immediately", DateTime.Now.AddSeconds(5), () => MessagingManager<TAppCoreSelfReference>.CallServiceOutcoming(type));

            _executingFlags.AddOrUpdate(nameof(RegisterIncomingMessage), 1, (k, o) => Math.Min(int.MaxValue, o + 1));
            TasksManager.SetTask(TasksIncomingHandle + "_immediately", DateTime.Now.AddSeconds(5), () => MessagingManager<TAppCoreSelfReference>.CallServiceIncomingHandle(type));
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            this.RegisterServiceState(ServiceStatus.Shutdown, "Сервис остановлен.");

            var type = GetType();

            TasksManager.RemoveTask(TasksOutcomingSend + "_minutely1");
            TasksManager.RemoveTask(TasksOutcomingSend + "_immediately");

            TasksManager.RemoveTask(TasksIncomingReceive + "_minutely1");

            TasksManager.RemoveTask(TasksIncomingHandle + "_minutely1");
            TasksManager.RemoveTask(TasksIncomingHandle + "_immediately");
        }
        #endregion

        #region Сообщения
        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> в очередь на отправку.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения.</returns>
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
                        StateType = DB.MessageStateType.NotProcessed,
                        Direction = false,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message),
                    };

                    db.MessageQueue.Add(mess);
                    db.SaveChanges();
                    if (_executingFlags.AddOrUpdate(nameof(RegisterOutcomingMessage), 1, (k, o) => Math.Min(int.MaxValue, o + 1)) == 1)
                    {
                        var type = GetType();
                        TasksManager.SetTask(TasksOutcomingSend + "_immediately", DateTime.Now.AddSeconds(5), () => MessagingManager<TAppCoreSelfReference>.CallServiceOutcoming(type));
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка регистрации исходящего сообщения", null, ex);
                return false;
            }
        }

        /// <summary>
        /// Регистрирует сообщение <paramref name="message"/> как входящее. Оно поступит в обработку в компоненты <see cref="IncomingMessageHandler{TAppCoreSelfReference, TMessage}"/>.
        /// </summary>
        /// <returns>Возвращает true в случае успеха и false в случае ошибки во время регистрации сообщения.</returns>
        [ApiReversible]
        protected bool RegisterIncomingMessage(TMessageType message)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                {
                    var mess = new DB.MessageQueue()
                    {
                        IdMessageType = IdMessageType,
                        StateType = DB.MessageStateType.NotProcessed,
                        Direction = true,
                        DateCreate = DateTime.Now,
                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message),
                    };

                    db.MessageQueue.Add(mess);
                    db.SaveChanges();
                    if (_executingFlags.AddOrUpdate(nameof(RegisterIncomingMessage), 1, (k, o) => Math.Min(int.MaxValue, o + 1)) == 1)
                    {
                        var type = GetType();
                        TasksManager.SetTask(TasksIncomingHandle + "_immediately", DateTime.Now.AddSeconds(5), () => MessagingManager<TAppCoreSelfReference>.CallServiceIncomingHandle(type));
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                this.RegisterEvent(Journaling.EventType.Error, "Ошибка регистрации входящего сообщения", null, ex);
                return false;
            }
        }

        private List<IntermediateStateMessage<TMessageType>> GetMessages(DB.DataContext db, bool direction)
        {
            var messages = db.MessageQueue.
                Where(x => x.Direction == direction && x.IdMessageType == IdMessageType && (x.StateType == DB.MessageStateType.NotProcessed || x.StateType == DB.MessageStateType.Repeat)).
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
                    return new IntermediateStateMessage<TMessageType>(null, x) { StateType = DB.MessageStateType.Error, State = ex.Message, DateChange = DateTime.Now };
                }
            }).ToList();

            return messagesUnserialized;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает список активных компонентов, работающих с типом сообщений сервиса.
        /// </summary>
        /// <seealso cref="Configuration.CoreConfiguration{TAppCoreSelfReference}.MessageServicesComponentsSettings"/>
        protected List<MessageServiceComponent<TAppCoreSelfReference, TMessageType>> GetComponents()
        {
            return AppCore.Get<MessagingManager<TAppCoreSelfReference>>().GetComponentsByMessageType<TMessageType>().ToList();
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
                return db.Repo1.Where(x => x.IdMessageType == IdMessageType && (x.StateType == DB.MessageStateType.NotProcessed || x.StateType == DB.MessageStateType.Repeat)).Count();
            }
        }
        #endregion

        #region IInternalForTasks
        void IMessageServiceInternal<TAppCoreSelfReference>.PrepareOutcoming()
        {
            var type = GetType();

            if (_executingFlags.AddOrUpdate(TasksOutcomingSend, 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1) return;
            _executingFlags[nameof(RegisterOutcomingMessage)] = 0;

            int messagesAll = 0;
            int messagesSent = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    var messages = GetMessages(db, false);
                    if (messages == null) return;

                    messagesAll = messages.Count;

                    OnBeforeExecuteOutcoming(messagesAll);

                    var processedMessages = new List<IntermediateStateMessage<TMessageType>>();

                    var time = new MeasureTime();
                    foreach (var intermediateMessage in messages)
                    {
                        if (intermediateMessage.StateType == DB.MessageStateType.Error)
                        {
                            processedMessages.Add(intermediateMessage);
                            continue;
                        }

                        var components = GetComponents().
                            OfType<OutcomingMessageSender<TAppCoreSelfReference, TMessageType>>().
                            Select(x => new {
                                Component = x,
                                IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                            }).
                            OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                            ToList();

                        if (intermediateMessage.IdTypeComponent.HasValue)
                        {
                            components = components.Where(x => x.IdTypeComponent.HasValue && x.IdTypeComponent == intermediateMessage.IdTypeComponent).ToList();
                        }

                        foreach (var componentInfo in components)
                        {
                            try
                            {
                                var component = componentInfo.Component;
                                var messageInfo = new MessageInfo<TMessageType>(intermediateMessage);
                                if (component.OnSend(messageInfo, this))
                                {
                                    if (messageInfo.StateType == MessageStateType.NotHandled) messageInfo.StateType = MessageStateType.Completed;
                                    intermediateMessage.DateChange = DateTime.Now;
                                    switch (messageInfo.StateType)
                                    {
                                        case MessageStateType.Error:
                                            intermediateMessage.StateType = DB.MessageStateType.Error;
                                            intermediateMessage.State = messageInfo.State;
                                            intermediateMessage.IdTypeComponent = null;
                                            break;

                                        case MessageStateType.Repeat:
                                            intermediateMessage.StateType = DB.MessageStateType.Repeat;
                                            intermediateMessage.State = messageInfo.State;
                                            intermediateMessage.IdTypeComponent = componentInfo.IdTypeComponent;
                                            break;

                                        case MessageStateType.Completed:
                                            intermediateMessage.StateType = DB.MessageStateType.Complete;
                                            intermediateMessage.State = null;
                                            intermediateMessage.IdTypeComponent = null;
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
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений в очереди - {messagesAll}. Отправлено - {messagesSent}. Ошибки отправки - {messagesErrors}.", ex);
            }
            finally
            {
                _executingFlags[TasksOutcomingSend] = 0;
            }
        }

        void IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncomingReceive()
        {
            var type = GetType();

            if (_executingFlags.AddOrUpdate(TasksIncomingReceive, 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1) return;

            int messagesReceived = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress)) // Здесь Suppress вместо RequiresNew, т.к. весь процесс отправки занимает много времени и блокировать таблицу нельзя.
                {
                    var components = GetComponents().
                    OfType<IncomingMessageReceiver<TAppCoreSelfReference, TMessageType>>().
                    Select(x => new {
                        Component = x,
                        IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                    }).
                    OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                    ToList();

                    foreach (var componentInfo in components)
                    {
                        try
                        {
                            var messages = componentInfo.Component.OnReceive(this);
                            if (messages != null && messages.Count > 0)
                            {
                                int countAdded = 0;
                                foreach (var message in messages)
                                {
                                    if (message == null) continue;

                                    var stateType = DB.MessageStateType.NotProcessed;
                                    switch (message.StateType)
                                    {
                                        case MessageStateType.Completed:
                                            stateType = DB.MessageStateType.Complete;
                                            break;

                                        case MessageStateType.Error:
                                            stateType = DB.MessageStateType.Error;
                                            break;

                                        case MessageStateType.NotHandled:
                                            stateType = DB.MessageStateType.NotProcessed;
                                            break;

                                        case MessageStateType.Repeat:
                                            stateType = DB.MessageStateType.Repeat;
                                            break;

                                    }

                                    var mess = new DB.MessageQueue()
                                    {
                                        IdMessageType = IdMessageType,
                                        Direction = true,
                                        State = message.State,
                                        StateType = stateType,
                                        DateCreate = DateTime.Now,
                                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message.Message),
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
                        catch (Exception ex)
                        {
                            this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
                            continue;
                        }

                        try
                        {
                            while (true)
                            {
                                var message = componentInfo.Component.OnBeginReceive(this);
                                if (message == null) break;

                                DB.MessageQueue queueMessage = null;

                                var queueState = DB.MessageStateType.NotProcessed;
                                switch (message.StateType)
                                {
                                    case MessageStateType.Completed:
                                        queueState = DB.MessageStateType.Complete;
                                        break;

                                    case MessageStateType.Error:
                                        queueState = DB.MessageStateType.Error;
                                        break;

                                    case MessageStateType.NotHandled:
                                        queueState = DB.MessageStateType.NotProcessed;
                                        break;

                                    case MessageStateType.Repeat:
                                        queueState = DB.MessageStateType.Repeat;
                                        break;

                                }

                                try
                                {
                                    var mess = new DB.MessageQueue()
                                    {
                                        IdMessageType = IdMessageType,
                                        Direction = true,
                                        State = message.State,
                                        StateType = DB.MessageStateType.IntermediateAdded,
                                        DateCreate = DateTime.Now,
                                        MessageInfo = Newtonsoft.Json.JsonConvert.SerializeObject(message.Message),
                                    };

                                    db.MessageQueue.Add(mess);
                                    db.SaveChanges();

                                    queueMessage = mess;
                                }
                                catch (Exception ex)
                                {
                                    this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка регистрации сообщения после '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка регистрации сообщения после вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
                                    try
                                    {
                                        componentInfo.Component.OnEndReceive(false, message, this);
                                    }
                                    catch (Exception ex2)
                                    {
                                        this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}' после ошибки регистрации сообщения.", ex2);
                                    }
                                    continue;
                                }

                                try
                                {
                                    var endReceiveResult = componentInfo.Component.OnEndReceive(true, message, this);
                                    if (endReceiveResult)
                                    {
                                        queueMessage.StateType = queueState;
                                    }
                                    else
                                    {
                                        db.MessageQueue.Delete(queueMessage);
                                    }
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}' после успешной регистрации сообщения.", ex);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.RegisterServiceEvent(Journaling.EventType.Error, $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}'", $"Ошибка вызова '{nameof(componentInfo.Component.OnBeginReceive)}' для компонента '{componentInfo?.Component?.GetType()?.FullName}'.", ex);
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
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений получено - {messagesReceived}.", ex);
            }
            finally
            {
                _executingFlags[TasksIncomingReceive] = 0;
            }
        }

        void IMessageServiceInternal<TAppCoreSelfReference>.PrepareIncomingHandle()
        {
            var type = GetType();

            if (_executingFlags.AddOrUpdate(TasksIncomingHandle, 1, (k, o) => Math.Min(int.MaxValue, o + 1)) > 1) return;
            _executingFlags[nameof(RegisterIncomingMessage)] = 0;

            int messagesAll = 0;
            int messagesSent = 0;
            int messagesErrors = 0;

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var messages = GetMessages(db, true);
                    if (messages.IsNullOrEmpty()) return;

                    messagesAll = messages.Count;

                    var components = GetComponents().
                        OfType<IncomingMessageHandler<TAppCoreSelfReference, TMessageType>>().
                        Select(x => new {
                            Component = x,
                            IdTypeComponent = ItemTypeFactory.GetItemType(x.GetType())?.IdItemType
                        }).
                        OrderBy(x => ((IPoolObjectOrdered)x.Component).OrderInPool).
                        ToList();

                    foreach (var intermediateMessage in messages)
                    {
                        var componentsForMessage = components;
                        if (intermediateMessage.IdTypeComponent.HasValue)
                        {
                            components = components.Where(x => x.IdTypeComponent.HasValue && x.IdTypeComponent == intermediateMessage.IdTypeComponent).ToList();
                        }

                        foreach (var componentInfo in components)
                        {
                            try
                            {
                                var component = componentInfo.Component;
                                var messageInfo = new MessageInfo<TMessageType>(intermediateMessage);
                                if (component.OnPrepare(messageInfo, this))
                                {
                                    if (messageInfo.StateType == MessageStateType.NotHandled) messageInfo.StateType = MessageStateType.Completed;
                                    intermediateMessage.DateChange = DateTime.Now;
                                    switch (messageInfo.StateType)
                                    {
                                        case MessageStateType.Error:
                                            intermediateMessage.StateType = DB.MessageStateType.Error;
                                            intermediateMessage.State = messageInfo.State;
                                            intermediateMessage.IdTypeComponent = null;
                                            break;

                                        case MessageStateType.Repeat:
                                            intermediateMessage.StateType = DB.MessageStateType.Repeat;
                                            intermediateMessage.State = messageInfo.State;
                                            intermediateMessage.IdTypeComponent = componentInfo.IdTypeComponent;
                                            break;

                                        case MessageStateType.Completed:
                                            intermediateMessage.StateType = DB.MessageStateType.Complete;
                                            intermediateMessage.State = null;
                                            intermediateMessage.IdTypeComponent = null;
                                            break;
                                    }
                                    db.SaveChanges();
                                    break;
                                }
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }
                }

                if (messagesAll > 0)
                {
                    this.RegisterServiceState(messagesErrors == 0 ? ServiceStatus.RunningIdeal : ServiceStatus.RunningWithErrors, $"Сообщений в очереди - {messagesAll}. Обработано - {messagesSent}. Ошибки обработки - {messagesErrors}.");
                }

                var service = AppCore.Get<Monitor<TAppCoreSelfReference>>().GetService(ServiceID);
                if (service != null && (DateTime.Now - service.LastDateEvent).TotalHours >= 1)
                {
                    this.RegisterServiceState(ServiceStatus.RunningIdeal, $"Сообщений нет, сервис работает без ошибок.");
                }
            }
            catch (Exception ex)
            {
                this.RegisterServiceState(ServiceStatus.RunningWithErrors, $"Сообщений в очереди - {messagesAll}. Обработано - {messagesSent}. Ошибки обработки - {messagesErrors}.", ex);
            }
            finally
            {
                _executingFlags[TasksIncomingHandle] = 0;
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
