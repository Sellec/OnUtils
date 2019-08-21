using System;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Application.Journaling.DB
{
    using Application.DB;
    using Architecture.AppCore;
    using Data;

    /// <summary>
    /// Предоставляет методы доступа к данным для менеджера журналов.
    /// </summary>
    public class JournalingManagerDatabaseAccessor<TAppCoreSelfReference> :
        CoreComponentBase<TAppCoreSelfReference>,
        IComponentSingleton<TAppCoreSelfReference>,
        IUnitOfWorkAccessor<DataContext>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        #region JournalInfo
        /// <summary>
        /// Формирует запрос в общем виде для получения данных о журналах. Запрос может быть дополнен условиями, сортировкой, соединениями с таблицами и т.д. и должен быть передан в <see cref="FetchQueryJournalInfo(IQueryable{QueryJournalInfo})"/>.
        /// </summary>
        /// <seealso cref="FetchQueryJournalInfo(IQueryable{QueryJournalInfo})"/>
        /// <seealso cref="FetchQueryJournalInfo{TQuery, TInfo}(IQueryable{TQuery}, Action{TQuery, TInfo})"/>
        public IQueryable<QueryJournalInfo> CreateQueryJournalInfo(DataContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var query = from JournalName in context.JournalName
                        select new QueryJournalInfo() { JournalName = JournalName };

            return query;
        }

        /// <summary>
        /// Возвращает данные о журналах, преобразованные к общему виду на основе переданного запроса.
        /// </summary>
        /// <seealso cref="CreateQueryJournalInfo(DataContext)"/>
        public List<TInfo> FetchQueryJournalInfo<TQuery, TInfo>(IQueryable<TQuery> query, Action<TQuery, TInfo> fillCallback)
            where TQuery : QueryJournalInfo, new()
            where TInfo : Model.JournalInfo, new()
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var list = new List<TInfo>();
            foreach (var row in query)
            {
                var info = new TInfo();
                Model.JournalInfo.Fill(info, row.JournalName);
                fillCallback?.Invoke(row, info);
                list.Add(info);
            }

            return list;
        }

        /// <summary>
        /// Возвращает данные о журналах, преобразованные к общему виду на основе переданного запроса.
        /// </summary>
        /// <seealso cref="CreateQueryJournalInfo(DataContext)"/>
        public List<Model.JournalInfo> FetchQueryJournalInfo(IQueryable<QueryJournalInfo> query)
        {
            return FetchQueryJournalInfo<QueryJournalInfo, Model.JournalInfo>(query, null);
        }
        #endregion

        #region JournalData
        /// <summary>
        /// Формирует запрос в общем виде для получения данных журналов. Запрос может быть дополнен условиями, сортировкой, соединениями с таблицами и т.д. и должен быть передан в <see cref="FetchQueryJournalData(IQueryable{QueryJournalData})"/>.
        /// </summary>
        /// <seealso cref="FetchQueryJournalData(IQueryable{QueryJournalData})"/>
        public IQueryable<QueryJournalData> CreateQueryJournalData(DataContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var query = from Journal in context.Journal
                        join JournalName in context.JournalName on Journal.IdJournal equals JournalName.IdJournal
                        join UserBase in context.Users on Journal.IdUser equals UserBase.IdUser into UserBase_j
                        from UserBase in UserBase_j.DefaultIfEmpty()
                        select new QueryJournalData() { JournalData = Journal, JournalName = JournalName, User = UserBase };

            return query;
        }

        /// <summary>
        /// Возвращает данные журналов, преобразованные к общему виду на основе переданного запроса.
        /// </summary>
        /// <seealso cref="CreateQueryJournalData(DataContext)"/>
        public List<TData> FetchQueryJournalData<TQuery, TData>(IQueryable<TQuery> query, Action<TQuery, TData> fillCallback)
            where TQuery : QueryJournalData, new()
            where TData : Model.JournalData, new()
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var journals = new Dictionary<int, Model.JournalInfo>();
            var users = new Dictionary<int, Users.UserInfo>();
            var list = new List<TData>();
            foreach (var row in query)
            {
                if (!journals.TryGetValue(row.JournalName.IdJournal, out var journal))
                {
                    journal = new Model.JournalInfo();
                    Model.JournalInfo.Fill(journal, row.JournalName);
                    journals[row.JournalName.IdJournal] = journal;
                }

                Users.UserInfo user = null;
                if (row.User != null && !users.TryGetValue(row.User.IdUser, out user))
                {
                    user = new Users.UserInfo(row.User);
                    users[row.User.IdUser] = user;
                }

                var data = new TData();
                Model.JournalData.Fill(data, row.JournalData, journal, user);
                fillCallback?.Invoke(row, data);
                list.Add(data);
            }

            return list;
        }

        /// <summary>
        /// Возвращает данные журналов, преобразованные к общему виду на основе переданного запроса.
        /// </summary>
        /// <seealso cref="CreateQueryJournalData(DataContext)"/>
        public List<Model.JournalData> FetchQueryJournalData(IQueryable<QueryJournalData> query)
        {
            return FetchQueryJournalData<QueryJournalData, Model.JournalData>(query, null);
        }
        #endregion

    }
}
