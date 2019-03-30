using System;

namespace OnUtils.Data.EntityFramework.Internal.DB
{
    using System.Collections.Concurrent;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // <summary>
    // A LINQ expression visitor that finds <see cref="DbQuery" /> uses with equivalent
    // <see cref="ObjectQuery" /> instances.
    // </summary>
    internal class DbQueryVisitor : ExpressionVisitor
    {
        private const BindingFlags SetAccessBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private ConcurrentDictionary<Type, Func<ObjectQuery, object>> _wrapperFactories = null;

        private static Type dbQueryVisitorType = null;
        private static FieldInfo dbQueryVisitorField = null;

        static DbQueryVisitor()
        {
            dbQueryVisitorType = typeof(System.Data.Entity.DbContext).Assembly.GetTypes().Where(x => x.Name == "DbQueryVisitor").FirstOrDefault();
            dbQueryVisitorField = dbQueryVisitorType.GetField("_wrapperFactories", BindingFlags.Static | BindingFlags.NonPublic);
        }

        public DbQueryVisitor()
        {
            _wrapperFactories = dbQueryVisitorField.GetValue() as ConcurrentDictionary<Type, Func<ObjectQuery, object>>;
        }

        #region Overriden visitors

        // <summary>
        // Replaces calls to DbContext.Set() with an expression for the equivalent <see cref="ObjectQuery" />.
        // </summary>
        // <param name="node"> The node to replace. </param>
        // <returns> A new node, which may have had the replacement made. </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            //Check.NotNull(node, "node");

            if (typeof(DbContext).IsAssignableFrom(node.Method.DeclaringType))
            {
                var memberExpression = node.Object as MemberExpression;
                if (memberExpression != null)
                {
                    // Only try to invoke the method if it is on the context, is not parameterless, and is not attributed
                    // as a function.
                    var context = GetContextFromConstantExpression<DbContext>(memberExpression.Expression, memberExpression.Member);
                    if (context != null
                        && !node.Method.GetCustomAttributes<DbFunctionAttribute>(inherit: false).Any()
                        && node.Method.GetParameters().Length == 0)
                    {
                        var expression =
                            CreateObjectQueryConstant(
                                node.Method.Invoke(context, SetAccessBindingFlags, null, null, null));
                        if (expression != null)
                        {
                            return expression;
                        }
                    }
                }
            }
            else if (typeof(Data.IRepository).IsAssignableFrom(node.Type) || node.Type.Name.StartsWith("RepositoryPropertyWrapper", StringComparison.OrdinalIgnoreCase))
            {
                var memberExpression = node.Object as MemberExpression;
                if (memberExpression != null)
                {
                    var context = GetContextFromConstantExpression<object>(memberExpression.Expression, memberExpression.Member);

                    var expression = CreateObjectQueryConstant(node.Method.Invoke(context, SetAccessBindingFlags, null, null, null));
                    if (expression != null)
                    {
                        return expression;
                    }
                }
            }

            return base.VisitMethodCall(node);
        }

        // <summary>
        // Replaces a <see cref="DbQuery" /> or <see cref="DbQuery{T}" /> property with a constant expression
        // for the underlying <see cref="ObjectQuery" />.
        // </summary>
        // <param name="node"> The node to replace. </param>
        // <returns> A new node, which may have had the replacement made. </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            //Check.NotNull(node, "node");

            var propInfo = node.Member as PropertyInfo;
            var memberExpression = node.Expression as MemberExpression;

            if (propInfo != null
                && memberExpression != null
                && typeof(IQueryable).IsAssignableFrom(propInfo.PropertyType))
            {
                if (typeof(DbContext).IsAssignableFrom(node.Member.DeclaringType))
                {
                    var context = GetContextFromConstantExpression<DbContext>(memberExpression.Expression, memberExpression.Member);
                    if (context != null)
                    {
                        var expression =
                            CreateObjectQueryConstant(propInfo.GetValue(context, SetAccessBindingFlags, null, null, null));
                        if (expression != null)
                        {
                            return expression;
                        }
                    }
                }
                else if (typeof(Data.IRepository).IsAssignableFrom(node.Type) || node.Type.Name.StartsWith("RepositoryPropertyWrapper", StringComparison.OrdinalIgnoreCase))
                {
                    var context = GetContextFromConstantExpression<object>(memberExpression.Expression, memberExpression.Member);
                    if (context != null)
                    {
                        var expression =
                            CreateObjectQueryConstant(propInfo.GetValue(context, SetAccessBindingFlags, null, null, null));
                        if (expression != null)
                        {
                            return expression;
                        }
                    }
                }
            }

            return base.VisitMember(node);
        }

        #endregion

        #region Helpers

        // <summary>
        // Gets a <see cref="DbContext" /> value from the given member, or returns null
        // if the member doesn't contain a DbContext instance.
        // </summary>
        // <param name="expression"> The expression for the object for the member, which may be null for a static member. </param>
        // <param name="member"> The member. </param>
        // <returns> The context or null. </returns>
        private static T GetContextFromConstantExpression<T>(Expression expression, MemberInfo member) where T : class
        {
            //DebugCheck.NotNull(member);

            if (expression == null)
            {
                // Static field/property access
                return GetContextFromMember<T>(member, null);
            }

            //Retrieve the context value from the encapsulated scope
            var value = GetExpressionValue(expression);
            if (value != null)
            {
                return GetContextFromMember<T>(member, value);
            }

            return null;
        }

        // <summary>
        // Tries to retrieve the value of an expression
        // If the expression is a constant, it returns the constant value.
        // If the expression is a field or property access on an expression, it returns the value of it recursively.
        // Otherwise it returns null
        // </summary>
        // <param name="expression">The expression</param>
        // <returns> The expression value. </returns>
        private static object GetExpressionValue(Expression expression)
        {
            //If the given expression is a constant, we just return its value
            var constantExpression = expression as ConstantExpression;
            if (constantExpression != null)
            {
                return constantExpression.Value;
            }

            //If the given expression is a member access on an inner expression, we recursively retrieve the value of the inner expression, and get the member value from it.
            var memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                var asField = memberExpression.Member as FieldInfo;
                if (asField != null)
                {
                    var innerValue = GetExpressionValue(memberExpression.Expression);

                    if (innerValue != null)
                    {
                        return asField.GetValue(innerValue);
                    }
                }

                var asProperty = memberExpression.Member as PropertyInfo;
                if (asProperty != null)
                {
                    var innerValue = GetExpressionValue(memberExpression.Expression);

                    if (innerValue != null)
                    {
                        return asProperty.GetValue(innerValue, null);
                    }
                }
            }

            return null;
        }

        // <summary>
        // Gets the <see cref="DbContext" /> instance from the given instance or static member, returning null
        // if the member does not contain a DbContext instance.
        // </summary>
        // <param name="member"> The member. </param>
        // <param name="value"> The value of the object to get the instance from, or null if the member is static. </param>
        // <returns> The context instance or null. </returns>
        private static T GetContextFromMember<T>(MemberInfo member, object value) where T : class
        {
            //DebugCheck.NotNull(member);

            var asField = member as FieldInfo;
            if (asField != null)
            {
                return asField.GetValue(value) as T;
            }
            var asProperty = member as PropertyInfo;
            if (asProperty != null)
            {
                return asProperty.GetValue(value, null) as T;
            }
            return null;
        }

        // <summary>
        // Takes a <see cref="DbQuery{T}" /> or <see cref="DbQuery" /> and creates an expression
        // for the underlying <see cref="ObjectQuery{T}" />.
        // </summary>
        private Expression CreateObjectQueryConstant(object dbQuery)
        {
            var objectQuery = ExtractObjectQuery(dbQuery);

            if (objectQuery != null)
            {
                var elementType = objectQuery.GetType().GetGenericArguments().Single();

                Func<ObjectQuery, object> factory;
                if (!_wrapperFactories.TryGetValue(elementType, out factory))
                {
                    var genericType = typeof(ReplacementDbQueryWrapper<>).MakeGenericType(elementType);
                    var factoryMethod = genericType.GetDeclaredMethod("Create", typeof(ObjectQuery));
                    factory =
                        (Func<ObjectQuery, object>)
                        Delegate.CreateDelegate(typeof(Func<ObjectQuery, object>), factoryMethod);
                    _wrapperFactories.TryAdd(elementType, factory);
                }

                var replacement = factory(objectQuery);

                var newConstant = Expression.Constant(replacement, replacement.GetType());
                return Expression.Property(newConstant, "Query");
            }

            return null;
        }

        // <summary>
        // Takes a <see cref="DbQuery{T}" /> or <see cref="DbQuery" /> and extracts the underlying <see cref="ObjectQuery{T}" />.
        // </summary>
        private static ObjectQuery ExtractObjectQuery(object dbQuery)
        {
            var internalQuery = DbQueryProvider.GetIInternalQuery(dbQuery as IQueryable);
            if (internalQuery != null)
            {
                var property_InternalQuery_ObjectQuery = internalQuery.GetType().GetProperty("ObjectQuery", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return property_InternalQuery_ObjectQuery.GetValue(internalQuery, null) as ObjectQuery;
            }

            return null;
        }

        #endregion
    }
}
