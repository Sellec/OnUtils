using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Reflection;

using System.Threading;
using System.Threading.Tasks;

using System.Transactions;

namespace OnUtils.Data
{
    /// <summary>
    /// Описывает контекст доступа к данным.
    /// </summary>
    public interface IDataContext : IDBOAccess, IDisposable
    {
        #region Работа с объектами
        /// <summary>
        /// Возвращает результат выполнения запроса внутри контекста. В качестве запроса может выступать строка, объект и т.п. - зависит от возможностей конкретной реализации контекста.
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="query">Запрос, который необходимо выполнить.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам запроса.
        /// Это может быть анонимный тип, например, для запроса с условием "DateChange=@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        /// <param name="cacheInLocal">Указывает, следует ли кешировать объекты, созданные в результате выполнения запроса, во внутренних репозиториях соответствующих типов.</param>
        /// <param name="entityExample"></param>
        IEnumerable<TEntity> ExecuteQuery<TEntity>(object query, object parameters = null, bool cacheInLocal = false, TEntity entityExample = default(TEntity)) where TEntity : class;

        ///// <summary>
        ///// Возвращает результат выполнения запроса внутри контекста. В качестве запроса может выступать строка, объект и т.п. - зависит от возможностей конкретной реализации контекста.
        ///// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        ///// </summary>
        ///// <param name="query">Запрос, который необходимо выполнить.</param>
        ///// <param name="parameters">
        ///// Коллекция, содержащая параметры со значениями, соответствующие параметрам запроса.
        ///// Например, для запроса с условием "DateChange=@Date" это должна быть коллекция вида new Dictionary{string, object}(){ { "Date", DateTime.Now } }.
        ///// </param>
        ///// <param name="cacheInLocal">Указывает, следует ли кешировать объекты, созданные в результате выполнения запроса, во внутренних репозиториях соответствующих типов.</param>
        ///// <param name="entityExample"></param>
        //IEnumerable<TEntity> ExecuteQuery<TEntity>(object query, IDictionary<string, object> parameters = null, bool cacheInLocal = false, TEntity entityExample = default(TEntity)) where TEntity : class;

        /// <summary>
        /// Возвращает результат выполнения запроса внутри контекста. В качестве запроса может выступать строка, объект и т.п. - зависит от возможностей конкретной реализации контекста.
        /// Возвращает количество строк, затронутых запросом..
        /// </summary>
        /// <param name="query">Запрос, который необходимо выполнить.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам запроса.
        /// Это может быть анонимный тип, например, для запроса с условием "DateChange=@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        int ExecuteQuery(object query, object parameters = null);

        ///// <summary>
        ///// Конструкция UPSERT (INSERT OR UPDATE).
        ///// Создает конструкцию UPSERT (MERGE для MS SQL, INSERT ON DUPLICATE KEY UPDATE для MySQL) на основе свойств объектов в <paramref name="objectsIntoQuery"/> и списка полей <paramref name="updateFields"/>.
        ///// </summary>
        ///// <param name="tableName">Название обновляемой таблицы.</param>
        ///// <param name="objectsIntoQuery">Список объектов <paramref name="objectsIntoQuery"/> для добавления или обновления.</param>
        ///// <param name="updateFields">Список полей, значения которых следует обновить, если объект с ключом уже существует в базе данных.</param>
        ///// <returns>Количество строк, затронутых операций.</returns>
        //int InsertOrDuplicateUpdate(string tableName, IEnumerable<object> objectsIntoQuery, params string[] updateFields);

        ///// <summary>
        ///// Конструкция UPSERT (INSERT OR UPDATE).
        ///// Создает конструкцию UPSERT (MERGE для MS SQL, INSERT ON DUPLICATE KEY UPDATE для MySQL) на основе свойств объектов в <paramref name="objectsIntoQuery"/> и списка полей <paramref name="updateFields"/>.
        ///// </summary>
        ///// <param name="tableName">Название обновляемой таблицы.</param>
        ///// <param name="objectsIntoQuery">Список объектов <paramref name="objectsIntoQuery"/> для добавления или обновления.</param>
        ///// <param name="lastIdentity">Идентификатор последнего вставленного столбца в данном запросе.</param>
        ///// <param name="updateFields">Список полей, значения которых следует обновить, если объект с ключом уже существует в базе данных.</param>
        ///// <returns>Количество строк, затронутых операций.</returns>
        //int InsertOrDuplicateUpdate(string tableName, IEnumerable<object> objectsIntoQuery, out object lastIdentity, params string[] updateFields);

        ///// <summary>
        ///// См. <see cref="InsertOrDuplicateUpdate(string, IEnumerable{object}, string[])"/>. Отличается тем, что принимает не список объектов, а строку запроса.
        ///// </summary>
        //int InsertOrDuplicateUpdate(string tableName, string insertQuery, params string[] updateFields);

        /// <summary>
        /// Возвращает состояние объекта <paramref name="item"/> относительно текущего контекста данных.
        /// </summary>
        ItemState GetItemState(object item);
        #endregion

        #region Применение изменений
        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// </summary>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        int SaveChanges();

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <typeparamref name="TEntity"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// </summary>
        /// <typeparam name="TEntity">Тип сущностей, для которых следует применить изменения.</typeparam>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        int SaveChanges<TEntity>() where TEntity : class;

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <paramref name="entityType"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// </summary>
        /// <param name="entityType">Тип сущностей, для которых следует применить изменения.</param>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        int SaveChanges(Type entityType);

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого сохраненного объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// </summary>
        /// <param name="cancellationToken">Токен System.Threading.CancellationToken, который нужно отслеживать во время ожидания выполнения задачи.</param>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        #endregion

        #region TransactionScope
        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope()"/>.
        /// </summary>
        ITransactionScope CreateScope();

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption)"/>.
        /// </summary>
        ITransactionScope CreateScope(TransactionScopeOption scopeOption);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(Transaction)"/>.
        /// </summary>
        ITransactionScope CreateScope(Transaction transactionToUse);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption, TimeSpan)"/>.
        /// </summary>
        ITransactionScope CreateScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption, TransactionOptions)"/>.
        /// </summary>
        ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(Transaction, TimeSpan)"/>.
        /// </summary>
        ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(TransactionScopeOption, TransactionOptions, EnterpriseServicesInteropOption)"/>.
        /// </summary>
        ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption);

        /// <summary>
        /// См. <see cref="TransactionScope.TransactionScope(Transaction, TimeSpan, EnterpriseServicesInteropOption)"/>.
        /// </summary>
        ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption);
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает и задает режим чтения/записи данных. Если true, то изменение данных в контексте возможны. Если false, то методы SaveChanges/AddEntity будут возвращать исключение <see cref="InvalidOperationException"/>.
        /// </summary>
        bool IsReadonly { get; set; }

        /// <summary>
        /// Возвращает список типов объектов, зарегистрированных для использования в контексте данных.
        /// </summary>
        Type[] RegisteredTypes { get; }

        /// <summary>
        /// Возвращает или задает таймаут выполнения запроса в миллисекундах.
        /// </summary>
        int QueryTimeout { get; set; }
        #endregion
    }
}
