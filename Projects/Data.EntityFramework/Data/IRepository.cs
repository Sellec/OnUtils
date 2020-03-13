using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace OnUtils.Data
{
    /// <summary>
    /// Указывает, как следует обновлять значение столбца.
    /// </summary>
    public class UpsertField
    {
        /// <summary>
        /// Указывает, что столбец должен быть обновлен напрямую.
        /// </summary>
        public UpsertField(string columnName)
        {
            ColumnName = columnName;
            IsDirect = true;
        }

        /// <summary>
        /// Указывает, что столбец должен быть обновлен напрямую.
        /// </summary>
        public UpsertField(string columnName, string updateRightPart)
        {
            ColumnName = columnName;
            IsDirect = false;
            UpdateRightPart = updateRightPart;
        }

        /// <summary>
        /// Название столбца.
        /// </summary>
        public string ColumnName
        {
            get;
            set;
        }

        /// <summary>
        /// Указывает, что значение поля должно быть обновлено путем простого приравнивания (Column = NewValue).
        /// </summary>
        public bool IsDirect
        {
            get;
            set;
        }

        /// <summary>
        /// Преобразование для столбца.
        /// </summary>
        public string UpdateRightPart
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Представляет интерфейс для доступа к объектам в репозитории, для выполнения запросов и операций SCRUD.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Возвращает контекст доступа к данным для данного репозитория.
        /// Если репозиторий относится к контейнеру <see cref="UnitOfWorkBase"/>, то вернет контекст доступа к данным всего контейнера.
        /// </summary>
        IDataContext DataContext { get; }

        /// <summary>
        /// Определяет, имеются ли в объектах репозитория несохраненные изменения.
        /// </summary>
        /// <returns>Возвращает true, если свойства каких-либо объектов в репозитории были изменены и отличаются от оригинальных значений (полученных при первом добавлении объекта в репозиторий).</returns>
        bool HasChanges();

        /// <summary>
        /// Очищает локальный кеш репозитория.
        /// </summary>
        void ClearCache();

    }

    /// <summary>
    /// Представляет интерфейс для доступа к объектам в репозитории, для выполнения запросов и операций SCRUD.
    /// </summary>
    public interface IRepository<TEntity> : IRepository, IQuery<TEntity> where TEntity : class
    {
        /// <summary>
        /// Добавляет объекты <paramref name="items"/> в репозиторий.
        /// </summary>
        void Add(params TEntity[] items);

        /// <summary>
        /// Добавляет объекты <paramref name="items"/> в репозиторий.
        /// Для объектов, уже находящихся в репозитории, будут выполнены операции UPDATE, для остальных - CREATE (<see cref="Add(TEntity[])"/>).
        /// </summary>
        void AddOrUpdate(params TEntity[] items);

        /// <summary>
        /// Добавляет объекты <paramref name="items"/> в репозиторий.
        /// Для объектов, уже находящихся в репозитории, будут выполнены операции UPDATE, для остальных - CREATE (<see cref="Add(TEntity[])"/>).
        /// Ключом для определения существующих записей является значение, возвращаемое <paramref name="identifierExpression"/>.
        /// </summary>
        void AddOrUpdate(Expression<Func<TEntity, object>> identifierExpression, params TEntity[] items);

        /// <summary>
        /// Конструкция UPSERT (INSERT OR UPDATE).
        /// Создает конструкцию UPSERT (MERGE для MS SQL, INSERT ON DUPLICATE KEY UPDATE для MySQL) на основе свойств объектов в <paramref name="objectsIntoQuery"/> и списка полей <paramref name="updateFields"/>, а также метаданных типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="objectsIntoQuery">Список объектов <paramref name="objectsIntoQuery"/> для добавления или обновления.</param>
        /// <param name="updateFields">Список полей, значения которых следует обновить, если объект с ключом уже существует в базе данных.</param>
        /// <returns>Количество строк, затронутых операций.</returns>
        int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, params UpsertField[] updateFields);

        /// <summary>
        /// Конструкция UPSERT (INSERT OR UPDATE).
        /// Создает конструкцию UPSERT (MERGE для MS SQL, INSERT ON DUPLICATE KEY UPDATE для MySQL) на основе свойств объектов в <paramref name="objectsIntoQuery"/> и списка полей <paramref name="updateFields"/>, а также метаданных типа <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="objectsIntoQuery">Список объектов <paramref name="objectsIntoQuery"/> для добавления или обновления.</param>
        /// <param name="lastIdentity">Идентификатор последней вставленной строки в данном запросе.</param>
        /// <param name="updateFields">Список полей, значения которых следует обновить, если объект с ключом уже существует в базе данных.</param>
        /// <returns>Количество строк, затронутых операций.</returns>
        int InsertOrDuplicateUpdate(IEnumerable<TEntity> objectsIntoQuery, out object lastIdentity, params UpsertField[] updateFields);

        /// <summary>
        /// См. <see cref="InsertOrDuplicateUpdate(IEnumerable{TEntity}, UpsertField[])"/>. Отличается тем, что принимает не список объектов, а строку запроса.
        /// </summary>
        int InsertOrDuplicateUpdate(string insertQuery, params UpsertField[] updateFields);

        /// <summary>
        /// Помечает объект <paramref name="item"/> в репозитории на удаление. Если объект не прикреплен к репозиторию, возвращает ошибку.
        /// </summary>
        void Delete(TEntity item);

        /// <summary>
        /// Определяет, имеются ли для объекта <paramref name="item"/> в репозитории несохраненные изменения.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Возвращает true, если свойства объекта были изменены и отличаются от оригинальных значений (полученных при первом добавлении объекта в репозиторий).</returns>
        bool HasChanges(TEntity item);

        /// <summary>
        /// Возвращает объекты из локального кеша репозитория.
        /// </summary>
        /// <param name="condition">Условие для поиска объектов в кеше репозитория.</param>
        IEnumerable<TEntity> FromCache(Expression<Func<TEntity, bool>> condition);

        /// <summary>
        /// Возвращает коллекцию <see cref="System.ComponentModel.BindingList{TEntity}"/> с привязкой к кешированному содержимому репозитория.
        /// </summary>
        System.ComponentModel.BindingList<TEntity> AsBindingList();

        /// <summary>
        /// Возвращает название таблицы.
        /// </summary>
        /// <returns></returns>
        string GetTableName();

        /// <summary>
        /// Отражает состояние чтения/записи контекста данных, с которым связан репозиторий.
        /// В режиме "только чтение":
        ///     1. Возможно только выполнение запросов;
        ///     2. Метод <see cref="Add(TEntity[])"/> возвращает ошибку при попытке добавить объект.
        ///     3. Метод <see cref="AddOrUpdate(TEntity[])"/> возвращает ошибку при попытке добавить объект.
        ///     4. Метод <see cref="Delete(TEntity)"/> возвращает ошибку при попытке удалить объект.
        ///     5. Метод <see cref="HasChanges(TEntity)"/> всегда возвращает false.
        ///     6. Метод <see cref="HasChanges"/> всегда возвращает false.
        /// </summary>
        bool IsReadonly { get; }

    }

}
