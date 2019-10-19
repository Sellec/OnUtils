using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Transactions;

namespace OnUtils.Data
{
    /// <summary>
    /// Базовый класс контейнера UnitOfWork.
    /// Типизированные варианты принимают в качестве параметров-типов типы объектов, с которыми необходимо работать в 
    /// конкретном контейнере.
    /// Свойства типа RepoXX ссылаются на репозитории объектов, указанных в качестве параметров-типов (например, <see cref="UnitOfWork{TEntity1}.Repo1"/>).
    /// Кроме этого, по-прежнему можно обратиться к репозиторию через метод <see cref="UnitOfWorkBase.Get{TEntity}"/>, 
    /// с условием, что данный тип объектов должен быть зарегистрирован в контейнере.
    /// 
    /// Кроме того, если имеются свойства с типом <see cref="IRepository{TEntity}"/>, то в конструкторе им будет автоматически назначен внутренний репозиторий соответствующего типа. 
    /// Фактически, будет создана обертка над вызовом <see cref="Get{TEntity}"/>.
    /// Поддерживаются get-set свойства, get свойства (без set, автоматическое назначение внутреннего field).
    /// </summary>
    public abstract partial class UnitOfWorkBase : IDisposable
    {
        private IDataContext _context = null;
        private ConcurrentDictionary<Type, object> _repositoryCache = new ConcurrentDictionary<Type, object>();

        /// <summary>
        /// Конструктор принимает список типов объектов для регистрации репозиториев.
        /// При попытке работать с незарегистрированным типом объектов (через <see cref="Get{TEntity}"/> или свойства-ссылки RepoXX) 
        /// будет сгенерировано исключение.
        /// </summary>
        /// <param name="entityTypes">Список типов объектов</param>
        /// <exception cref="InvalidOperationException">Возникает, если не установлен провайдер данных (см. <see cref="DataAccessManager.SetDefaultService(IDataService)"/>).</exception>
        protected UnitOfWorkBase(params Type[] entityTypes)
        {
            DataAccessManager.CheckDefaultService();

            var entityTypes2 = GetPropertiesTypes();
            entityTypes2.AddRange(entityTypes);
            if (entityTypes2.Any(x => x.IsNested)) throw new InvalidOperationException("Нельзя использовать вложенные типы в качестве типов объектов.");
            _context = DataAccessManager.GetDefaultService().CreateDataContext(modelAccessor => OnModelCreating(modelAccessor), entityTypes2.ToArray());
            GenerateProperties();
        }

        /// <summary>
        /// Вызывается во время создания модели данных и предоставляет доступ к процессу построения модели через методы расширения для <paramref name="modelAccessor"/>.
        /// </summary>
        protected virtual void OnModelCreating(UnitOfWork.IModelAccessor modelAccessor)
        {

        }

        #region Получение данных из репозиториев
        /// <summary>
        /// Возвращает интерфейс для работы с репозиторием указанного типа объектов <typeparamref name="TEntity"/>.
        /// Репозиторий регистрируется для указанного типа объектов <typeparamref name="TEntity"/> внутри контейнера и сохраняется до уничтожения контейнера.
        /// Тип объектов <typeparamref name="TEntity"/> должен быть типом, зарегистрированным для контекста доступа к данным (можно проверить в <see cref="IDataContext.RegisteredTypes"/>).
        /// </summary>
        /// <returns></returns>
        public IRepository<TEntity> Get<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_context.RegisteredTypes.Contains(type)) throw new TypeAccessException($"Тип {type.Name} не зарегистрирован в контейнере.");

            var repo = _repositoryCache.GetOrAdd(
                type,
                (t) => DataAccessManager.GetDefaultService().CreateRepository<TEntity>(_context)
            );

            return (IRepository<TEntity>)repo;
        }

        /// <summary>
        /// Возвращает состояние объекта <paramref name="item"/> относительно текущего контейнера.
        /// </summary>
        public ItemState GetState(object item)
        {
            return _context.GetItemState(item);
        }

        #endregion

        /// <summary>
        /// Выполняет операции для очистки данных и уничтожения объекта. 
        /// Все незавершенные транзакции для объектов внутри контейнера откатываются.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (_context != null) _context.Dispose();
            }
            finally
            {

            }
        }

        #region Внесение и применение изменений
        /// <summary>
        /// Добавляет объект <paramref name="item"/> в репозиторий соответствующего типа <typeparamref name="TEntity"/>.
        /// Тип <typeparamref name="TEntity"/> должен быть зарегистрирован в контейнере.
        /// </summary>
        public void AddEntity<TEntity>(TEntity item) where TEntity : class
        {
            Get<TEntity>().Add(item);
        }

        /// <summary>
        /// Удаляет объект <paramref name="item"/> из репозитория соответствующего типа <typeparamref name="TEntity"/>.
        /// Тип <typeparamref name="TEntity"/> должен быть зарегистрирован в контейнере.
        /// </summary>
        public void DeleteEntity<TEntity>(TEntity item) where TEntity : class
        {
            Get<TEntity>().Delete(item);
        }

        /// <summary>
        /// См. <see cref="IDataContext.SaveChanges()"/>.  
        /// </summary>
        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        /// <summary>
        /// См. <see cref="IDataContext.SaveChanges{TEntity}"/>.  
        /// </summary>
        public int SaveChanges<TEntity>() where TEntity : class
        {
            return _context.SaveChanges<TEntity>();
        }

        /// <summary>
        /// См. <see cref="IDataContext.SaveChanges(Type)"/>.  
        /// </summary>
        public int SaveChanges(Type entityType)
        {
            return _context.SaveChanges(entityType);
        }
        #endregion

        #region Транзакции
        /// <summary>
        /// См. <see cref="IDataContext.CreateScope()"/>.
        /// </summary>
        public ITransactionScope CreateScope()
        {
            return _context.CreateScope();
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(TransactionScopeOption)"/>.
        /// </summary>
        public ITransactionScope CreateScope(TransactionScopeOption scopeOption)
        {
            return _context.CreateScope(scopeOption);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(Transaction)"/>.
        /// </summary>
        public ITransactionScope CreateScope(Transaction transactionToUse)
        {
            return _context.CreateScope(transactionToUse);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(TransactionScopeOption, TimeSpan)"/>.
        /// </summary>
        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            return _context.CreateScope(scopeOption, scopeTimeout);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(TransactionScopeOption, TransactionOptions)"/>.
        /// </summary>
        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            return _context.CreateScope(scopeOption, transactionOptions);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(Transaction, TimeSpan)"/>.
        /// </summary>
        public ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            return _context.CreateScope(transactionToUse, scopeTimeout);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(TransactionScopeOption, TransactionOptions, EnterpriseServicesInteropOption)"/>.
        /// </summary>
        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption)
        {
            return _context.CreateScope(scopeOption, transactionOptions, interopOption);
        }

        /// <summary>
        /// См. <see cref="IDataContext.CreateScope(Transaction, TimeSpan, EnterpriseServicesInteropOption)"/>.
        /// </summary>
        public ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption)
        {
            return _context.CreateScope(transactionToUse, scopeTimeout, interopOption);
        }
        #endregion

        #region Свойства
        /// <summary>
        /// Возвращает контекст доступа к данным.
        /// </summary>
        public IDataContext DataContext
        {
            get { return _context; }
        }
        #endregion

        private List<Type> GenerateProperties()
        {
            var repositoryTypes = new List<Type>();

            var typesList = new List<Type>() { GetType() };
            typesList.AddRange(GetType().GetNestedTypes());

            var properties = typesList.
                SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty).
                Select(y => new { Property = y, PropertyInterface = Types.TypeHelpers.ExtractGenericInterface(y.PropertyType, typeof(IRepository<>)) }).
                Where(y => y.PropertyInterface != null).
                Select(y => y));

            var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var property in properties)
            {
                var setMethod = property.Property.GetSetMethod(true); //property.Property.SetMethod 4.5.2
                var setBackingField = setMethod == null ? property.Property.DeclaringType.GetField($"<{property.Property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                if (setMethod == null && setBackingField == null) throw new Exception("Невозможно установить значение свойства.");

                //var setBackingField = property.Property.SetMethod == null ? property.Property.DeclaringType.GetField($"<{property.Property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                //if (property.Property.SetMethod == null && setBackingField == null) throw new Exception("Невозможно установить значение свойства.");

                var entityType = property.PropertyInterface.GetGenericArguments().First(); //.GenericTypeArguments[0]; //4.5.2
                if (!repositoryTypes.Contains(entityType)) repositoryTypes.Add(entityType);

                //var ddd = this.GetType().GetMethod(nameof(Get)).MakeGenericMethod(entityType);
                //var propValue = ddd.Invoke(this, new object[] { });

                var wrapperType = typeof(UnitOfWork.RepositoryPropertyWrapper<>).MakeGenericType(entityType);
                var wrapperConstructor = wrapperType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic,
                                                         null,
                                                         new Type[] { typeof(UnitOfWorkBase) },
                                                         null);
                var wrapper = wrapperConstructor.Invoke(new object[] { this });

#if NET40
                if (setMethod != null) property.Property.SetValue(this, wrapper, null);
#else
                if (setMethod != null) property.Property.SetValue(this, wrapper);
#endif
                else if (setBackingField != null) setBackingField.SetValue(this, wrapper);
            }

            return repositoryTypes;
        }

        private List<Type> GetPropertiesTypes()
        {
            var repositoryTypes = new List<Type>();

            var types = this.GetType().GetNestedTypes().ToList();
            types.Add(this.GetType());
            var properties = types.SelectMany(x => x.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty)
                                                    .Select(y => new { Property = y, PropertyInterface = Types.TypeHelpers.ExtractGenericInterface(y.PropertyType, typeof(IRepository<>)) })
                                                    .Where(y => y.PropertyInterface != null)
                                                    .Select(y => y));

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (var property in properties)
            {
                var setMethod = property.Property.GetSetMethod(true); //property.Property.SetMethod 4.5.2
                var setBackingField = setMethod == null ? property.Property.DeclaringType.GetField($"<{property.Property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                if (setMethod == null && setBackingField == null) throw new Exception("Невозможно установить значение свойства.");

                //var setBackingField = property.Property.SetMethod == null ? property.Property.DeclaringType.GetField($"<{property.Property.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic) : null;
                //if (property.Property.SetMethod == null && setBackingField == null) throw new Exception("Невозможно установить значение свойства.");

                var entityType = property.PropertyInterface.GetGenericArguments().First(); //.GenericTypeArguments[0]; //4.5.2
                if (!repositoryTypes.Contains(entityType)) repositoryTypes.Add(entityType);
            }

            return repositoryTypes;
        }

    }
}
