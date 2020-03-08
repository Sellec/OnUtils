using Dapper;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OnUtils.Data.EntityFramework.Internal
{
    using Data;
    using Internal.DB;
    using UnitOfWork;

    static class DataContextInternalModelFactory
    {
        private static ConcurrentDictionary<string, Tuple<DbCompiledModel, string>> _compiledModels = new ConcurrentDictionary<string, Tuple<DbCompiledModel, string>>();

        internal static Tuple<DbCompiledModel, string> GetCompiledModel(Type[] entityTypes, Func<DbModelBuilder, string, string> callModelCreating)
        {
            var modelKey = string.Join(";", entityTypes.Select(x => x.FullName).OrderBy(x => x));

            Tuple<DbCompiledModel, string> compiledModel = null;
            if (!_compiledModels.TryGetValue(modelKey, out compiledModel))
            {
                compiledModel = _compiledModels.GetOrAdd(modelKey, (key) =>
                {
                    var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.Latest);
                    foreach (var type in entityTypes)
                    {
                        modelBuilder.RegisterEntityType(type);
                        Dapper.SqlMapper.SetTypeMap(type, Activator.CreateInstance(typeof(DapperColumnAttributeTypeMapper<>).MakeGenericType(type)) as SqlMapper.ITypeMap);
                    }

                    modelBuilder.Properties<decimal>().Configure(x => x.HasPrecision(18, 6));

                    modelBuilder.Conventions.Remove<DecimalPropertyConvention>();
                    modelBuilder.Conventions.Add(new DecimalPropertyConvention(38, 18));

                    foreach (Type classType in entityTypes)
                    {
                        foreach (var propAttr in classType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttributes<DecimalPrecisionAttribute>(true).FirstOrDefault() != null).Select(
                               p => new { prop = p, attr = p.GetCustomAttributes<DecimalPrecisionAttribute>(true).FirstOrDefault() }))
                        {

                            var entityConfig = modelBuilder.GetType().GetMethod("Entity").MakeGenericMethod(classType).Invoke(modelBuilder, null);
                            var param = ParameterExpression.Parameter(classType, "c");
                            var property = Expression.Property(param, propAttr.prop.Name);
                            var lambdaExpression = Expression.Lambda(property, true, new ParameterExpression[] { param });

                            DecimalPropertyConfiguration decimalConfig;
                            if (propAttr.prop.PropertyType.IsGenericType && propAttr.prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property" && p.ReturnType == typeof(DecimalPropertyConfiguration)).ElementAt(1);
                                decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
                            }
                            else
                            {
                                MethodInfo methodInfo = entityConfig.GetType().GetMethods().Where(p => p.Name == "Property" && p.ReturnType == typeof(DecimalPropertyConfiguration)).ElementAt(0);
                                decimalConfig = methodInfo.Invoke(entityConfig, new[] { lambdaExpression }) as DecimalPropertyConfiguration;
                            }

                            decimalConfig.HasPrecision(propAttr.attr.Precision, propAttr.attr.Scale);
                        }
                    }

                    var connectionStringBase = GetConnectionString(entityTypes);
                    var connectionString = callModelCreating(modelBuilder, connectionStringBase);
                    if (string.IsNullOrEmpty(connectionString)) connectionString = connectionStringBase;

                    if (string.IsNullOrEmpty(connectionString))
                        throw new InvalidOperationException($"Невозможно получить строку подключения. Задайте поставщик строк подключения в {nameof(DataAccessManager)}.{nameof(DataAccessManager.SetConnectionStringResolver)} или верните строку подключения через свойство {typeof(IModelAccessor).FullName}.{nameof(IModelAccessor.ConnectionString)} в методе {typeof(UnitOfWorkBase).FullName}.OnModelCreating.");

                    using (var connection = new SqlConnection(connectionString))
                    {
                        var model = modelBuilder.Build(connection);
                        return new Tuple<DbCompiledModel, string>(model.Compile(), connectionString);
                    }
                });
            }

            return compiledModel;
        }

        internal static string GetConnectionString(Type[] entityTypes)
        {
            var connectionStringResolver = DataAccessManager.GetConnectionStringResolver();
            if (connectionStringResolver == null) return null;

            var connectionString = connectionStringResolver.ResolveConnectionStringForDataContext(entityTypes ?? new Type[0]);
            var connectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
            connectionStringBuilder.MaxPoolSize = Math.Max(10000, connectionStringBuilder.MaxPoolSize);

            return connectionStringBuilder.ToString();
        }
    }

}
