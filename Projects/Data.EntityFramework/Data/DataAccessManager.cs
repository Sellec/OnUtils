using System;
using System.Collections.Generic;

namespace OnUtils.Data
{
    /// <summary>
    /// Предоставляет доступ к общим функциям, связанным со слоем данных.
    /// </summary>
    public static class DataAccessManager
    {
        #region IDataService
        private static bool _testKnownService = true;
        private static IDataService _defaultService = null;
        private static object _defaultServiceSyncRoot = new object();

        /// <summary>
        /// Устанавливает сервис <see cref="IDataService"/> в качестве основного для построения контекстов данных и репозиториев.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="service"/> равен null.</exception>
        public static void SetDefaultService(IDataService service)
        {
            if (service == null) throw new ArgumentNullException(nameof(service));
            _testKnownService = false;
            _defaultService = service;
            _defaultService.Initialize();
        }

        /// <summary>
        /// Возвращает текущий сервис <see cref="IDataService"/> для построения контекстов данных и репозиториев. Если возвращает null, значит, сервис не задан и при попытке создать новые контексты или репозитории будет генерироваться исключение <see cref="InvalidOperationException"/>.
        /// </summary>
        public static IDataService GetDefaultService()
        {
            lock (_defaultServiceSyncRoot)
            {
                if (_defaultService == null && _testKnownService)
                {
                    try
                    {
                        _testKnownService = false;
                        var serviceType = Type.GetType("OnUtils.Data.EntityFramework.DataService, OnUtils.Data.EntityFramework6, Culture=neutral, PublicKeyToken=8e22adab863b765a", false);
                        if (serviceType == null) serviceType = Type.GetType("OnUtils.Data.EntityFramework.DataService, OnUtils.Data.EntityFrameworkCore, Culture=neutral, PublicKeyToken=8e22adab863b765a", false);
                        if (serviceType != null) SetDefaultService((IDataService)Activator.CreateInstance(serviceType));
                    }
                    catch { }
                }
            }
            return _defaultService;
        }
        #endregion

        #region IConnectionStringResolver
        private static IConnectionStringResolver _connectionStringResolver = null;

        /// <summary>
        /// Позволяет задать объект <paramref name="resolver"/>, возвращающий строки подключения для контекстов данных.
        /// </summary>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="resolver"/> равен null.</exception>
        public static void SetConnectionStringResolver(IConnectionStringResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
            _connectionStringResolver = resolver;
        }

        /// <summary>
        /// Возвращает текущий объект <see cref="IConnectionStringResolver"/>, возвращающий строки подключения для контекстов данных
        /// </summary>
        public static IConnectionStringResolver GetConnectionStringResolver()
        {
            return _connectionStringResolver;
        }
        #endregion

        #region Методы
        /// <summary>
        /// Возвращает репозиторий для объектов типа <typeparamref name="TEntity"/>. 
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="IDataService"/> (см. <see cref="SetDefaultService(IDataService)"/>).</exception>
        public static IRepository<TEntity> Get<TEntity>() where TEntity : class
        {
            CheckDefaultService();
            var context = GetDefaultService().CreateDataContext(null, new Type[] { typeof(TEntity) });
            context.IsReadonly = true;
            return GetDefaultService().CreateRepository<TEntity>(context);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры. 
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="IDataService"/> (см. <see cref="SetDefaultService(IDataService)"/>).</exception>
        public static IEnumerable<TEntity> StoredProcedure<TEntity>(string procedure_name, object parameters = null) where TEntity : class
        {
            CheckDefaultService();
            var context = GetDefaultService().CreateDataContext(null, new Type[] { typeof(TEntity) });
            return context.StoredProcedure<TEntity>(procedure_name, parameters);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="IDataService"/> (см. <see cref="SetDefaultService(IDataService)"/>).</exception>
        public static Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>> StoredProcedure<TEntity1, TEntity2>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            CheckDefaultService();
            var context = GetDefaultService().CreateDataContext(null, new Type[] { typeof(TEntity1), typeof(TEntity2) });
            return context.StoredProcedure<TEntity1, TEntity2>(procedure_name, parameters);
        }

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен сервис <see cref="IDataService"/> (см. <see cref="SetDefaultService(IDataService)"/>).</exception>
        public static Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>> StoredProcedure<TEntity1, TEntity2, TEntity3>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class
        {
            CheckDefaultService();
            var context = GetDefaultService().CreateDataContext(null, new Type[] { typeof(TEntity1), typeof(TEntity2), typeof(TEntity3) });
            return context.StoredProcedure<TEntity1, TEntity2, TEntity3>(procedure_name, parameters);
        }

        internal static void CheckDefaultService()
        {
            if (GetDefaultService() == null) throw new InvalidOperationException($"Не установлен сервис для построения контекстов данных и репозиториев '{nameof(IDataService)}'.");
        }
        #endregion

    }

}
