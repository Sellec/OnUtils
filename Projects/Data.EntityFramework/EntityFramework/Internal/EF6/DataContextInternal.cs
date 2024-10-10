using Dapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace OnUtils.Data.EntityFramework.Internal
{
    using Data;
    using Data.Errors;
    using Data.Validation;
    using UnitOfWork;

    [DbConfigurationType(typeof(DbConfigInternal))]
    class DataContextInternal : DbContext, IDataContext
    {
        private static ConcurrentDictionary<string, PropertyInfo> edmCSpaceTypeNameList = new ConcurrentDictionary<string, PropertyInfo>();
        private List<Type> _entityTypes = new List<Type>();
        private DbCompiledModel _model = null;
        private bool _isReadonly = false;

        public DataContextInternal(Action<IModelAccessor> modelAccessorDelegate, Type[] entityTypes) :
            this(entityTypes, DataContextInternalModelFactory.GetCompiledModel(entityTypes, (modelBuilder, connectionString) => OnModelCreating(modelAccessorDelegate, modelBuilder, connectionString)))
        {
        }

        private DataContextInternal(Type[] entityTypes, Tuple<DbCompiledModel, string> model)
            : base(model.Item2, model.Item1)
        {
            _model = model.Item1;
            entityTypes.ForEach(x => RecursivelyAddEntityTypes(x));
            SetInitializer(this);
            Database.Log = DataAccessManager.SQLDebug;
            IsReadonly = false;
        }

        private static string OnModelCreating(Action<IModelAccessor> modelAccessorDelegate, DbModelBuilder modelBuilder, string connectionString)
        {
            if (modelAccessorDelegate != null)
            {
                var modelAccessor = new ModelAccessorInternal() { ConnectionString = connectionString };
                modelAccessorDelegate(modelAccessor);
                modelAccessor?.ModelBuilderDelegate?.Invoke(modelBuilder);
                connectionString = modelAccessor.ConnectionString;
            }

            return connectionString;
        }

        private void RecursivelyAddEntityTypes(Type entityType)
        {
            if (_entityTypes.Contains(entityType)) return;
            _entityTypes.Add(entityType);
        }

        private void SetInitializer<TContext>(TContext context) where TContext : DbContext
        {
            var method = typeof(Database).GetMethod(nameof(Database.SetInitializer), BindingFlags.Static | BindingFlags.Public);
            var method2 = method.MakeGenericMethod(context.GetType());
            method2.Invoke(null, new object[] { null });
        }

        private void UpdateReadonlyBehaviour()
        {
            var method = typeof(ObjectContext).GetMethod(nameof(ObjectContext.CreateObjectSet), new Type[0]);
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;

            var entityContainers = objectContext.MetadataWorkspace.GetItems<EntityContainer>(DataSpace.CSpace);
            var objectItemCollection = (ObjectItemCollection)objectContext.MetadataWorkspace.GetItemCollection(DataSpace.OSpace);

            foreach (var entityType in _entityTypes)
            {
                foreach (EntityContainer entityContainer in entityContainers)
                {
                    EdmType objectEdmType;
                    if (objectItemCollection.TryGetItem(FullNameWithNesting(entityType), out objectEdmType))
                    {
                        var propertyCSpaceTypeName = edmCSpaceTypeNameList.GetOrAdd(objectEdmType.GetType().FullName, k => objectEdmType.GetType().GetProperty("CSpaceTypeName", BindingFlags.Instance | BindingFlags.NonPublic));
                        var cSpaceTypeName = propertyCSpaceTypeName?.GetValue(objectEdmType, null)?.ToString();

                        if (entityContainer.EntitySets.Any(x => x.ElementType.FullName == cSpaceTypeName))
                        {
                            var set = method.MakeGenericMethod(entityType).Invoke(objectContext, new object[0]) as ObjectQuery;
                            set.MergeOption = IsReadonly ? MergeOption.NoTracking : MergeOption.AppendOnly;
                        }
                    }
                }
            }
        }

        public static string FullNameWithNesting(Type type)
        {
            if (!type.IsNested)
            {
                return type.FullName;
            }

            return type.FullName.Replace('+', '.');
        }

        #region Работа с объектами
        IEnumerable<TEntity> IDataContext.ExecuteQuery<TEntity>(object query, object parameters, bool cacheInLocal, TEntity entityExample)
        {
            try
            {
                var queryText = query.ToString();

                if (Utils.TypeHelper.IsAnonymousType(typeof(TEntity)))
                {
                    var type = typeof(TEntity);
                    var properties = type.
                        GetProperties().
                        ToDictionary(
                            x => x, 
                            x => type.
                                GetFields(BindingFlags.Instance | BindingFlags.NonPublic).
                                Where(y => y.Name.StartsWith("<" + x.Name + ">") || y.Name == "$" + x.Name).
                                FirstOrDefault()
                        );

                    var t = typeof(Dapper.SqlMapper).GetNestedType("DapperRow", BindingFlags.NonPublic);
                    if (t != null)
                    {
                        var dapperTypeContainsKey = t.GetMethod("System.Collections.Generic.IDictionary<System.String,System.Object>.ContainsKey", BindingFlags.NonPublic | BindingFlags.Instance);
                        var dapperTypeGet = t.GetMethod("System.Collections.Generic.IDictionary<System.String,System.Object>.get_Item", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (dapperTypeContainsKey != null && dapperTypeGet != null)
                        {
                            var dynamicParameters = new DynamicParameters(parameters);
                            var results = Database.Connection.Query(queryText, dynamicParameters, buffered: cacheInLocal, commandTimeout: Database.CommandTimeout);

                            return results.Select(res =>
                            {
                                var obj = (TEntity)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(TEntity));
                                properties.
                                    Where(x => x.Value != null).
                                    ForEach(x => x.Value.SetValue(obj, dapperTypeContainsKey.Invoke(res, new object[] { x.Key.Name }).Equals(true) ? dapperTypeGet.Invoke(res, new object[] { x.Key.Name }) : null));
                                return obj;
                            });
                        }
                    }

                    throw new Exception("Какие-то проблемы во время получения массива анонимных объектов.");
                }
                else
                {
                    Dapper.SqlMapper.SetTypeMap(typeof(TEntity), new DapperColumnAttributeTypeMapper<TEntity>());

                    var dynamicParameters = new DynamicParameters(parameters);
                    var results = Database.Connection.Query<TEntity>(queryText, parameters, buffered: cacheInLocal, commandTimeout: Database.CommandTimeout);
                    return results;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// См. <see cref="IDataContext.ExecuteQuery(object, object)"/>.
        /// </summary>
        public int ExecuteQuery(object query, object parameters = null)
        {
            try
            {
                if (query != null)
                {
                    var queryText = query.ToString();
                    var results = Database.Connection.Execute(queryText, parameters, commandTimeout: Database.CommandTimeout);

                    return results;
                }
                return 0;
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region InsertOrDuplicateUpdate
        class sp_res
        {
            public string Query { get; set; }
        }

        private MappingFragment GetTableMappings(Type type)
        {
            var metadata = ((IObjectContextAdapter)this).ObjectContext.MetadataWorkspace;

            // Get the part of the model that contains info about the actual CLR types
            var objectItemCollection = ((ObjectItemCollection)metadata.GetItemCollection(DataSpace.OSpace));

            // Get the entity type from the model that maps to the CLR type
            var entityType = metadata
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(e => objectItemCollection.GetClrType(e) == type);

            // Get the entity set that uses this entity type
            var entitySet = metadata
                .GetItems<EntityContainer>(DataSpace.CSpace)
                .Single()
                .EntitySets
                .Single(s => s.ElementType.Name == entityType.Name);

            // Find the mapping between conceptual and storage model for this entity set
            var mapping = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                    .Single()
                    .EntitySetMappings
                    .Single(s => s.EntitySet == entitySet);

            // Find the storage entity set (table) that the entity is mapped
            var tableMappings = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single();

            return tableMappings;
        }

        internal int InsertOrDuplicateUpdate<TEntity>(IEnumerable<TEntity> objectsIntoQuery, out object lastIdentity, params UpsertField[] updateFields)
        {
            try
            {
                if (objectsIntoQuery.Count() == 0) throw new ArgumentException("Не указано ни одного объекта для обновления.", nameof(objectsIntoQuery));
                if (updateFields == null || updateFields.Count() == 0) throw new ArgumentNullException(nameof(updateFields), "Если не указываются поля для обновления, то следует пользоваться методом Add или AddOrUpdate.");

                var entityType = typeof(TEntity);
                var mappings = GetTableMappings(entityType);
                var tableName = mappings.StoreEntitySet.Table;
                var properties = entityType.GetProperties().Join(mappings.PropertyMappings,
                                                       property => property.Name,
                                                       mapping => mapping.Property.Name,
                                                       (property, mapping) => new { Property = property, Column = (mapping as ScalarPropertyMapping).Column }).ToDictionary(x => x.Property.Name, x => x);

                var updateFields2 = updateFields.Select(x => new { source = x, mapped = properties.Values.Where(y => y.Property.Name == x.ColumnName || y.Column.Name.Equals(x.ColumnName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() });
                var updateFields2NotFound = updateFields2.Where(x => x.mapped == null).Select(x => x.source);
                if (updateFields2NotFound.Count() > 0) throw new ArgumentOutOfRangeException(nameof(updateFields), "Следующие поля не найдены: " + string.Join(", ", updateFields2NotFound));

                var customCulture = (System.Globalization.CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
                customCulture.NumberFormat.NumberDecimalSeparator = ".";
                customCulture.DateTimeFormat = new System.Globalization.DateTimeFormatInfo() { ShortDatePattern = "yyyy-MM-dd" };

                var currentCulture = Thread.CurrentThread.CurrentCulture;
                var results = 0;
                lastIdentity = null;

                try
                {
                    Thread.CurrentThread.CurrentCulture = customCulture;

                    for (int i = 0; i < objectsIntoQuery.Count(); i += 1000)
                    {
                        var rows = objectsIntoQuery.Skip(i).Take(1000).Select(x => "(" + string.Join(", ", properties.Values.Select(y => new { Property = y, Value = y.Property.GetValue(x, null) }).Select(y =>
                        {
                            var clrType = y.Property.Column.PrimitiveType.ClrEquivalentType;
                            var preValue = y.Value == null ? null : (y.Property.Property.PropertyType != clrType ? Convert.ChangeType(y.Value, clrType) : y.Value);

                            if (preValue == null) return "NULL";

                            var preValueStr = preValue.ToString();
                            var preValueStrLength = preValueStr.Length;

                            var isBinary = y.Property.Column.TypeName.Contains("binary");
                            if (isBinary)
                            {
                                var binaryData = (byte[])preValue;
                                preValueStr = "0x" + BitConverter.ToString(binaryData).Replace("-", "").ToLower();
                                preValueStrLength = binaryData.Length;
                            }

                            var castToType = y.Property.Column.TypeName;
                            if (castToType.EndsWith("char", StringComparison.InvariantCultureIgnoreCase) && preValueStr.Length > 0) castToType += $"({preValueStr.Length})";
                            else if (y.Property.Column.Precision.HasValue && y.Property.Column.Scale.HasValue) castToType += $"({y.Property.Column.Precision.Value}, {y.Property.Column.Scale.Value})";
                            //else if (y.Property.Column.Precision.HasValue) castToType += $"({y.Property.Column.Precision.Value})";

                            if (castToType.ToLower().Contains("char")) preValueStr = preValueStr.Replace("'", "''");

                            if (!isBinary) preValueStr = "'" + preValueStr + "'";

                            return "CAST(" + preValueStr + " as " + castToType + ")";
                        })) + ")");

                        var insertString = "INSERT INTO @t (" + string.Join(", ", properties.Values.Select(x => $"[{x.Column.Name}]")) + ") VALUES " + string.Join(", ", rows);
                        results += InsertOrDuplicateUpdate(tableName, insertString, out lastIdentity, updateFields2.ToDictionary(x => x.source, x => x.mapped.Column.Name));
                    }
                }
                finally
                {
                    Thread.CurrentThread.CurrentCulture = currentCulture;
                }
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        internal int InsertOrDuplicateUpdate<TEntity>(string insertQuery, params UpsertField[] updateFields)
        {
            object lastIdentity;
            return InsertOrDuplicateUpdate<TEntity>(insertQuery, out lastIdentity, updateFields);
        }

        internal int InsertOrDuplicateUpdate<TEntity>(string insertQuery, out object lastIdentity, params UpsertField[] updateFields)
        {
            try
            {
                if (updateFields == null || updateFields.Count() == 0) throw new ArgumentNullException(nameof(updateFields), "Если не указываются поля для обновления, то следует пользоваться методом Add или AddOrUpdate.");

                var mappings = GetTableMappings(typeof(TEntity));
                var tableName = mappings.StoreEntitySet.Table;
                var properties = typeof(TEntity).GetProperties().Join(mappings.PropertyMappings,
                                                       property => property.Name,
                                                       mapping => mapping.Property.Name,
                                                       (property, mapping) => new { Property = property, Column = (mapping as ScalarPropertyMapping).Column });

                var updateFields2 = updateFields.Select(x => new { source = x, mapped = properties.Where(y => y.Property.Name == x.ColumnName || y.Column.Name.Equals(x.ColumnName, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault() });
                var updateFields2NotFound = updateFields2.Where(x => x.mapped == null).Select(x => x.source);
                if (updateFields2NotFound.Count() > 0) throw new ArgumentOutOfRangeException(nameof(updateFields), "Следующие поля не найдены: " + string.Join(", ", updateFields2NotFound));

                return InsertOrDuplicateUpdate(tableName, insertQuery, out lastIdentity, updateFields2.ToDictionary(x => x.source, x => x.mapped.Column.Name));
            }
            catch (Exception)
            {
                throw;
            }
        }

        private int InsertOrDuplicateUpdate(string tableName, string insertQuery, out object lastIdentity, Dictionary<UpsertField, string> updateFields)
        {
            try
            {
                if (!insertQuery.Trim().StartsWith("insert into @t", StringComparison.InvariantCultureIgnoreCase)) throw new ArgumentException("Запрос должен начинаться с 'INSERT INTO @t'.", nameof(insertQuery));

                var insertString = insertQuery;
                var updateString = string.Join(", ", updateFields.Select(x =>
                {
                    if (x.Key.IsDirect)
                        return $"T.[{x.Value.Trim()}] = S.[{x.Value.Trim()}]";
                    else
                        return $"T.[{x.Value.Trim()}] = {x.Key.UpdateRightPart.Trim()}";
                }));

                var results = StoredProcedure<sp_res>("InsertOnDuplicate_CreateQuery", new { TableName = tableName, InsertData = insertString, UpdateData = updateString });
                var query = results.FirstOrDefault().Query;

                var results2 = ((IDataContext)this).ExecuteQuery(query + ";SELECT @CountRows AS QueryCount, SCOPE_IDENTITY() AS QueryIdentity;", null, entityExample: new { QueryCount = 0, QueryIdentity = (object)null }).ToList();
                lastIdentity = results2.First().QueryIdentity;
                return results2.First().QueryCount;
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        public ItemState GetItemState(object item)
        {
            var entry = Entry(item);
            switch (entry.State)
            {
                case EntityState.Added:
                    return ItemState.Added;

                case EntityState.Deleted:
                    return ItemState.Deleted;

                case EntityState.Detached:
                    return ItemState.Detached;

                case EntityState.Modified:
                    return ItemState.Modified;

                case EntityState.Unchanged:
                    return ItemState.Unchanged;

                default:
                    return ItemState.Detached;
            }

        }

        #endregion

        #region IDBOAccess
        public IEnumerable<TEntity> StoredProcedure<TEntity>(string procedure_name, object parameters = null) where TEntity : class
        {
            try
            {
                var results = Database.Connection.Query<TEntity>(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.CommandTimeout);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>> StoredProcedure<TEntity1, TEntity2>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
        {
            try
            {
                using (var reader = Database.Connection.QueryMultiple(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.CommandTimeout))
                {
                    return new Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>>(reader.Read<TEntity1>().ToList(), reader.Read<TEntity2>().ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>> StoredProcedure<TEntity1, TEntity2, TEntity3>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class
        {
            try
            {
                using (var reader = Database.Connection.QueryMultiple(procedure_name, parameters, commandType: System.Data.CommandType.StoredProcedure, commandTimeout: Database.CommandTimeout))
                {
                    return new Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>>(reader.Read<TEntity1>().ToList(), reader.Read<TEntity2>().ToList(), reader.Read<TEntity3>().ToList());
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        #region IDisposable
        void IDisposable.Dispose()
        {
            Dispose();

            _entityTypes = null;
            _model = null;
        }
        #endregion

        #region Применение изменений
        /// <summary>
        /// Возвращает список всех измененных объектов.
        /// </summary>
        /// <returns></returns>
        private Dictionary<DbEntityEntry, EntityState> GetChangedEntities()
        {
            if (IsReadonly) throw new ReadonlyModeExcepton();

            var entities = ChangeTracker.Entries()
                            .Where(x => x.State != EntityState.Unchanged)
                            .ToDictionary(x => x, x => x.State);

            return entities;
        }

        /// <summary>
        /// Для каждого объекта, состояние которого было изменено в результате применения изменений, вызываются методы, помеченные 
        /// атрибутом <see cref="Items.SavedInContextEventAttribute"/> для выполнения дополнительных действий после сохранения. 
        /// </summary>
        /// <param name="entities"></param>
        private void DetectSavedEntities(Dictionary<DbEntityEntry, EntityState> entities)
        {
            if (IsReadonly) throw new ReadonlyModeExcepton();

            // Core.Items.MethodMarkCallerAttribute.CallMethodsInObjects<Core.Items.SavedInContextEventAttribute>(entities.Where(x => x.Value != x.Key.State));

            foreach (var pair in entities)
            {
                if (pair.Value != pair.Key.State)
                {
                    Items.MethodMarkCallerAttribute.CallMethodsInObject<Items.SavedInContextEventAttribute>(pair.Key.Entity);
                }
            }
        }

        private void PrepareEFException(Exception exSource)
        {
            if (exSource is DbUpdateConcurrencyException)
            {
                var ex2 = exSource as DbUpdateConcurrencyException;
                var newEx = new UpdateConcurrencyException(
                        ex2.Message,
                        ex2.InnerException,
                        ex2.Entries.Select(x => new RepositoryEntryInternal(x)));

                throw newEx;
            }
            else if (exSource is DbUpdateException)
            {
                var ex2 = exSource as DbUpdateConcurrencyException;
                if (ex2 != null)
                {
                    var newEx = new UpdateException(
                            ex2.Message,
                            ex2.InnerException,
                            ex2.Entries.Select(x => new RepositoryEntryInternal(x)));

                    throw newEx;
                }
                else
                {
                    var newEx = new UpdateException(
                         exSource.Message,
                         exSource.InnerException,
                         null);
                    throw newEx;
                }
            }
            else if (exSource is System.Data.Entity.Validation.DbEntityValidationException)
            {
                var ex2 = exSource as System.Data.Entity.Validation.DbEntityValidationException;

                var newEx = new EntityValidationException(
                        ex2.Message,
                        ex2.EntityValidationErrors.Select(x => new EntityValidationResult(new RepositoryEntryInternal(x.Entry), x.ValidationErrors.Select(y => new ValidationError(y.PropertyName, y.ErrorMessage)))),
                        ex2.InnerException
                );

                throw newEx;
            }
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public override int SaveChanges()
        {
            var entities = GetChangedEntities();
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                PrepareEFException(ex);

                Debug.WriteLine("Необработанный тип исключения: {0}", ex.GetType().FullName);
                throw;
            }
            finally
            {
                DetectSavedEntities(entities);
            }
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <typeparamref name="TEntity"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <typeparam name="TEntity">Тип сущностей, для которых следует применить изменения.</typeparam>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public int SaveChanges<TEntity>() where TEntity : class
        {
            return SaveChanges(typeof(TEntity));
        }

        /// <summary>
        /// Применяет все изменения базы данных, произведенные в контексте для указанного типа объектов <paramref name="entityType"/>.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges()"/>. 
        /// </summary>
        /// <param name="entityType">Тип сущностей, для которых следует применить изменения.</param>
        /// <returns>Количество объектов, записанных в базу данных.</returns>
        public int SaveChanges(Type entityType)
        {
            if (entityType == null) throw new ArgumentNullException("entityType", "Должен быть указан тип сущностей для сохранения.");
            if (IsReadonly) throw new ReadonlyModeExcepton();

            var original = ChangeTracker.
                Entries().
                Where(x => !entityType.IsAssignableFrom(x.Entity.GetType()) && x.State != EntityState.Unchanged).
                Select(x => new { Entity = x, CurrentValues = x.CurrentValues.Clone() }).
                GroupBy(x => x.Entity.State).
                ToList();

            foreach (var entry in ChangeTracker.Entries().Where(x => !entityType.IsAssignableFrom(x.Entity.GetType())))
            {
                entry.State = EntityState.Unchanged;
            }

            var entities = GetChangedEntities();
            try
            {
                return base.SaveChanges();
            }
            catch (Exception ex)
            {
                PrepareEFException(ex);
                throw;
            }
            finally
            {
                foreach (var state in original)
                {
                    foreach (var entry in state)
                    {
                        entry.Entity.State = state.Key;
                        entry.Entity.CurrentValues.SetValues(entry.CurrentValues);
                    }
                }

                DetectSavedEntities(entities);
            }
        }

#if NET40
        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges"/>. 
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public Task<int> SaveChangesAsync()
        {
            throw new NotImplementedException("Для .NET Framework 4.0 не поддерживается асинхронное сохранение изменений.");
        }

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого сохраненного объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges"/>. 
        /// </summary>
        /// <param name="cancellationToken">Токен System.Threading.CancellationToken, который нужно отслеживать во время ожидания выполнения задачи.</param>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException("Для .NET Framework 4.0 не поддерживается асинхронное сохранение изменений.");
        }
#else
        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого затронутого объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges"/>. 
        /// </summary>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public override Task<int> SaveChangesAsync()
        {
            var entities = GetChangedEntities();

            var task = base.SaveChangesAsync();
            return Task.Factory.StartNew<int>(() =>
            {
                try
                {
                    task.Wait();
                    var rows = task.Result;
                    return rows;
                }
                catch (Exception ex)
                {
                    PrepareEFException(ex);
                    throw;
                }
                finally
                {
                    DetectSavedEntities(entities);
                }
            });
        }

        /// <summary>
        /// Асинхронно применяет все изменения базы данных, произведенные в контексте.
        /// После окончания операции для каждого сохраненного объекта выполняются методы, помеченные атрибутом <see cref="Items.SavedInContextEventAttribute"/>.
        /// Более подробную информацию см. <see cref="DbContext.SaveChanges"/>. 
        /// </summary>
        /// <param name="cancellationToken">Токен System.Threading.CancellationToken, который нужно отслеживать во время ожидания выполнения задачи.</param>
        /// <returns>Задача, представляющая асинхронную операцию сохранения. Результат задачи содержит количество объектов, записанных в базу данных.</returns>
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            var entities = GetChangedEntities();

            var task = base.SaveChangesAsync(cancellationToken);
            return Task.Factory.StartNew<int>(() =>
            {
                try
                {
                    task.Wait();
                    var rows = task.Result;
                    return rows;
                }
                catch (Exception ex)
                {
                    PrepareEFException(ex);
                    throw;
                }
                finally
                {
                    DetectSavedEntities(entities);
                }
            });
        }
#endif
        #endregion

        #region TransactionScope
        public ITransactionScope CreateScope()
        {
            return new Internal.TransactionScopeInternal();
        }

        public ITransactionScope CreateScope(TransactionScopeOption scopeOption)
        {
            return new Internal.TransactionScopeInternal(scopeOption);
        }

        public ITransactionScope CreateScope(Transaction transactionToUse)
        {
            return new Internal.TransactionScopeInternal(transactionToUse);
        }

        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TimeSpan scopeTimeout)
        {
            return new Internal.TransactionScopeInternal(scopeOption, scopeTimeout);
        }

        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions)
        {
            return new Internal.TransactionScopeInternal(scopeOption, transactionOptions);
        }

        public ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout)
        {
            return new Internal.TransactionScopeInternal(transactionToUse, scopeTimeout);
        }

        public ITransactionScope CreateScope(TransactionScopeOption scopeOption, TransactionOptions transactionOptions, EnterpriseServicesInteropOption interopOption)
        {
            return new Internal.TransactionScopeInternal(scopeOption, transactionOptions, interopOption);
        }

        public ITransactionScope CreateScope(Transaction transactionToUse, TimeSpan scopeTimeout, EnterpriseServicesInteropOption interopOption)
        {
            return new Internal.TransactionScopeInternal(transactionToUse, scopeTimeout, interopOption);
        }
        #endregion

        #region Свойства
        /// <summary>
        /// См. <see cref="IDataContext.IsReadonly"/>.
        /// </summary>
        public bool IsReadonly
        {
            get => _isReadonly;
            set
            {
                _isReadonly = value;
                UpdateReadonlyBehaviour();
            }
        }

        /// <summary>
        /// См. <see cref="IDataContext.RegisteredTypes"/>.
        /// </summary>
        public Type[] RegisteredTypes
        {
            get => _entityTypes.ToArray();
        }

        /// <summary>
        /// См. <see cref="IDataContext.QueryTimeout"/>.
        /// </summary>
        public int QueryTimeout
        {
            get => !Database.CommandTimeout.HasValue ? 30000 : Database.CommandTimeout.Value * 1000;
            set => Database.CommandTimeout = Math.Max(1, value / 1000);
        }
        #endregion

    }
}
