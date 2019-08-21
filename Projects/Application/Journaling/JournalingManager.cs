using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace OnUtils.Application.Journaling
{
    using Application.DB;
    using Architecture.AppCore;
    using Data;
    using Items;
    using ExecutionRegisterResult = ExecutionResult<int?>;
    using ExecutionResultJournalData = ExecutionResult<Model.JournalData>;
    using ExecutionResultJournalDataList = ExecutionResult<List<Model.JournalData>>;
    using ExecutionResultJournalName = ExecutionResult<Model.JournalInfo>;

    /// <summary>
    /// Представляет менеджер системных журналов. Позволяет создавать журналы, как привязанные к определенным типам, так и вручную, и регистрировать в них события.
    /// </summary>
    public sealed class JournalingManager<TAppCoreSelfReference> :
        CoreComponentBase<TAppCoreSelfReference>,
        IComponentSingleton<TAppCoreSelfReference>,
        IUnitOfWorkAccessor<DB.DataContext>,
        ITypedJournalComponent<JournalingManager<TAppCoreSelfReference>>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        //Список журналов, основанных на определенном типе объектов.
        private ConcurrentDictionary<Type, ExecutionResultJournalName> _typedJournalsList = new ConcurrentDictionary<Type, ExecutionResultJournalName>();

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            DatabaseAccessor = AppCore.Get<DB.JournalingManagerDatabaseAccessor<TAppCoreSelfReference>>();
            RegisterJournalTyped<JournalingManager<TAppCoreSelfReference>>("Менеджер журналов");
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region Регистрация журналов
        /// <summary>
        /// Регистрирует новый журнал или обновляет старый по ключу <paramref name="uniqueKey"/> (если передан).
        /// </summary>
        /// <param name="idType">См. <see cref="DB.JournalNameDAO.IdJournalType"/>.</param>
        /// <param name="name">См. <see cref="DB.JournalNameDAO.Name"/>.</param>
        /// <param name="uniqueKey">См. <see cref="DB.JournalNameDAO.UniqueKey"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="name"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName RegisterJournal(int idType, string name, string uniqueKey = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

                var data = new DB.JournalNameDAO()
                {
                    IdJournalType = idType,
                    Name = name,
                    UniqueKey = uniqueKey
                };

                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
                {
                    db.JournalName.AddOrUpdate(x => x.UniqueKey, data);
                    db.SaveChanges();
                    scope.Commit();
                }

                var info = new Model.JournalInfo();
                Model.JournalInfo.Fill(info, data);
                return new ExecutionResultJournalName(true, null, info);
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.RegisterJournal)}: {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время регистрации журнала с именем '{name}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="name">См. <see cref="DB.JournalNameDAO.Name"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="name"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName RegisterJournalTyped<TJournalTyped>(string name)
        {
            return RegisterJournalTyped(typeof(TJournalTyped), name);
        }

        internal ExecutionResultJournalName RegisterJournalTyped(Type typedType, string name)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);
            var fullName = Utils.TypeNameHelper.GetFullNameCleared(typedType);
            return RegisterJournal(JournalingConstants.IdSystemJournalType, name, JournalingConstants.TypedJournalsPrefix + fullName);
        }
        #endregion

        #region Получить журналы
        /// <summary>
        /// Возвращает журнал по уникальному ключу <paramref name="uniqueKey"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="uniqueKey"/> представляет пустую строку или null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournal(string uniqueKey)
        {
            try
            {
                if (string.IsNullOrEmpty(uniqueKey)) throw new ArgumentNullException(nameof(uniqueKey));

                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var data = db.JournalName.Where(x => x.UniqueKey == uniqueKey).FirstOrDefault();
                    var info = new Model.JournalInfo();
                    Model.JournalInfo.Fill(info, data);
                    return new ExecutionResultJournalName(data != null, data != null ? null : "Журнал с указанным уникальным ключом не найден.", info);
                }
            }
            catch (ArgumentNullException) { throw; }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.GetJournal)}(string): {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время получения журнала с уникальным именем '{uniqueKey}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает журнал по идентификатору <paramref name="IdJournal"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournal(int IdJournal)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var data = db.JournalName.Where(x => x.IdJournal == IdJournal).FirstOrDefault();
                    var info = new Model.JournalInfo();
                    Model.JournalInfo.Fill(info, data);
                    return new ExecutionResultJournalName(data != null, data != null ? null : "Журнал с указанным уникальным идентификатором не найден.", info);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.GetJournal)}(int): {ex.ToString()}");
                return new ExecutionResultJournalName(false, $"Возникла ошибка во время получения журнала с идентификатором '{IdJournal}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает журнал на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        [ApiIrreversible]
        public ExecutionResultJournalName GetJournalTyped<TJournalTyped>()
        {
            return GetJournalTyped(typeof(TJournalTyped));
        }

        internal ExecutionResultJournalName GetJournalTyped(Type typeTyped)
        {
            return _typedJournalsList.GetOrAddWithExpiration(
                typeTyped,
                (t) =>
                {
                    var fullName = Utils.TypeNameHelper.GetFullNameCleared(typeTyped);
                    return GetJournal(JournalingConstants.TypedJournalsPrefix + fullName);
                },
                TimeSpan.FromMinutes(5));
        }

        /// <summary>
        /// Возвращает события, связанные с объектом <paramref name="relatedItem"/> во всех журналах.
        /// </summary>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalDataList"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionResultJournalDataList GetJournalForItem(ItemBase<TAppCoreSelfReference> relatedItem)
        {
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionResultJournalDataList(false, "Ошибка получения данных о типе объекта.");

            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var query = DatabaseAccessor.CreateQueryJournalData(db).Where(x => x.JournalData.IdRelatedItem == relatedItem.ID && x.JournalData.IdRelatedItemType == itemType.IdItemType);
                    var data = DatabaseAccessor.FetchQueryJournalData(query);

                    return new ExecutionResultJournalDataList(true, null, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.GetJournalForItem)}: {ex.ToString()}");
                return new ExecutionResultJournalDataList(false, $"Возникла ошибка во время получения событий. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Возвращает событие с идентификатором <paramref name="idJournalData"/>. Все методы регистрации событий в результате содержат идентификатор созданной записи.
        /// </summary>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionResultJournalData"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionResultJournalData.Result"/> содержит информацию о событии.
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionResultJournalData GetJournalData(int idJournalData)
        {
            try
            {
                using (var db = this.CreateUnitOfWork())
                using (var scope = db.CreateScope(TransactionScopeOption.Suppress))
                {
                    var query = DatabaseAccessor.CreateQueryJournalData(db).Where(x => x.JournalData.IdJournalData == idJournalData);
                    var data = DatabaseAccessor.FetchQueryJournalData(query).FirstOrDefault();

                    return new ExecutionResultJournalData(true, null, data);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.GetJournalForItem)}: {ex.ToString()}");
                return new ExecutionResultJournalData(false, $"Возникла ошибка во время получения события. Смотрите информацию в системном текстовом журнале.");
            }
        }
        #endregion

        #region Записать в журнал
        /// <summary>
        /// Регистрирует новое событие в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent(int IdJournal, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventInternal(IdJournal, eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEvent<TJournalTyped>(EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEvent(typeof(TJournalTyped), eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        internal ExecutionRegisterResult RegisterEvent(Type typedType, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);

            try
            {
                var journalResult = GetJournalTyped(typedType);
                return !journalResult.IsSuccess ?
                    new ExecutionRegisterResult(false, journalResult.Message) :
                    RegisterEventInternal(journalResult.Result.IdJournal, eventType, eventInfo, eventInfoDetailed, eventTime, exception);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.RegisterEvent)}: {ex.ToString()}");
                var fullName = Utils.TypeNameHelper.GetFullNameCleared(typedType);
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в типизированный журнал '{fullName}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом <paramref name="relatedItem"/>, в журнале <paramref name="IdJournal"/>.
        /// </summary>
        /// <param name="IdJournal">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="relatedItem">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem(int IdJournal, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionRegisterResult(false, "Ошибка получения данных о типе объекта.");

            return RegisterEventInternal(IdJournal, eventType, eventInfo, eventInfoDetailed, eventTime, exception, relatedItem.ID, itemType.IdItemType);
        }

        /// <summary>
        /// Регистрирует новое событие, связанное с объектом <paramref name="relatedItem"/>, в журнале на основе типа <typeparamref name="TJournalTyped"/>.
        /// </summary>
        /// <param name="relatedItem">См. <see cref="DB.JournalDAO.IdJournal"/>.</param>
        /// <param name="eventType">См. <see cref="DB.JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="DB.JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="DB.JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="DB.JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="DB.JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>
        /// Возвращает объект <see cref="ExecutionRegisterResult"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. 
        /// В случае успеха свойство <see cref="ExecutionRegisterResult.Result"/> содержит идентификатор записи журнала (см. также <see cref="GetJournalData(int)"/>).
        /// В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.
        /// </returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="relatedItem"/> равен null.</exception>
        [ApiIrreversible]
        public ExecutionRegisterResult RegisterEventForItem<TJournalTyped>(ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            return RegisterEventForItem(typeof(TJournalTyped), relatedItem, eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }

        internal ExecutionRegisterResult RegisterEventForItem(Type typedType, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
        {
            typedType = ManagerExtensions.GetJournalType(typedType);
            if (relatedItem == null) throw new ArgumentNullException(nameof(relatedItem));
            var itemType = ItemTypeFactory.GetItemType(relatedItem.GetType());
            if (itemType == null) return new ExecutionRegisterResult(false, "Ошибка получения данных о типе объекта.");

            try
            {
                var journalResult = GetJournalTyped(typedType);
                return !journalResult.IsSuccess ?
                    new ExecutionRegisterResult(false, journalResult.Message) :
                    RegisterEventInternal(journalResult.Result.IdJournal, eventType, eventInfo, eventInfoDetailed, eventTime, exception, relatedItem.ID, itemType.IdItemType);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.RegisterEventForItem)}: {ex.ToString()}");
                var fullName = Utils.TypeNameHelper.GetFullNameCleared(typedType);
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в типизированный журнал '{fullName}'. Смотрите информацию в системном текстовом журнале.");
            }
        }

        private ExecutionRegisterResult RegisterEventInternal(int IdJournal, EventType eventType, string eventInfo, string eventInfoDetailed, DateTime? eventTime, Exception exception, int? idRelatedItem = null, int? idRelatedItemType = null)
        {
            try
            {
                if (IdJournal <= 0) throw new ArgumentOutOfRangeException(nameof(IdJournal));
                if (string.IsNullOrEmpty(eventInfo)) throw new ArgumentNullException(nameof(eventInfo));

                var exceptionDetailed = exception != null ? (exception.GetMessageExtended() + "\r\n" + exception.ToString()) : null;
                if (!string.IsNullOrEmpty(exceptionDetailed))
                {
                    var pos = exceptionDetailed.IndexOf("System.Web.Mvc.ActionMethodDispatcher.Execute", StringComparison.InvariantCultureIgnoreCase);
                    if (pos >= 0) exceptionDetailed = exceptionDetailed.Substring(0, pos);
                }

                var idUser = AppCore.GetUserContextManager().GetCurrentUserContext()?.IdUser;
                if (idUser == 0) idUser = null;

                var data = new DB.JournalDAO()
                {
                    IdJournal = IdJournal,
                    EventType = eventType,
                    EventInfo = eventInfo?.Truncate(0, 300),
                    EventInfoDetailed = eventInfoDetailed,
                    ExceptionDetailed = exceptionDetailed,
                    DateEvent = eventTime ?? DateTime.Now,
                    IdUser = idUser
                };

                using (var db = this.CreateUnitOfWork())
                {
                    DB.JournalNameDAO journalForCritical = null;

                    using (var scope = db.CreateScope(TransactionScopeOption.RequiresNew))
                    {
                        db.Journal.Add(data);
                        db.SaveChanges();

                        if (eventType == EventType.CriticalError)
                        {
                            journalForCritical = db.JournalName.Where(x => x.IdJournal == IdJournal).FirstOrDefault();
                        }
                        scope.Commit();
                    }

                    if (eventType == EventType.CriticalError)
                    {
                        var body = $"Дата события: {data.DateEvent.ToString("dd.MM.yyyy HH:mm:ss")}\r\n";
                        body += $"Сообщение: {data.EventInfo}\r\n";
                        if (!string.IsNullOrEmpty(data.EventInfoDetailed)) body += $"Подробная информация: {data.EventInfoDetailed}\r\n";
                        if (!string.IsNullOrEmpty(data.ExceptionDetailed)) body += $"Исключение: {data.ExceptionDetailed}\r\n";

                        AppCore.Get<Messaging.MessagingManager<TAppCoreSelfReference>>().GetCriticalMessagesReceivers().ForEach(x => x.SendToAdmin(journalForCritical != null ? $"Критическая ошибка в журнале '{journalForCritical.Name}'" : "Критическая ошибка", body));
                    }

                }

                return new ExecutionRegisterResult(true, null, data.IdJournalData);
            }
            catch (HandledException ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.RegisterEvent)}: {ex.InnerException?.ToString()}");
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в журнал №{IdJournal}. {ex.Message} Смотрите информацию в системном текстовом журнале.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{typeof(JournalingManager<TAppCoreSelfReference>).FullName}.{nameof(JournalingManager<TAppCoreSelfReference>.RegisterEvent)}: {ex.ToString()}");
                return new ExecutionRegisterResult(false, $"Возникла ошибка во время регистрации события в журнал №{IdJournal}. Смотрите информацию в системном текстовом журнале.");
            }
        }

        #endregion

        #region Свойства
        /// <summary>
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public DB.JournalingManagerDatabaseAccessor<TAppCoreSelfReference> DatabaseAccessor { get; private set; }
        #endregion
    }
}
