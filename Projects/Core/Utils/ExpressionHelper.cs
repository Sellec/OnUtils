using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc.ExpressionUtil;

#pragma warning disable CS1591
namespace OnUtils.Utils
{
    public static class ExpressionHelper
        {
            public static string GetExpressionText(string expression)
            {
                if (!string.Equals(expression, "model", StringComparison.OrdinalIgnoreCase))
                {
                    return expression;
                }
                return string.Empty;
            }

            public static string GetExpressionText(LambdaExpression expression)
            {
                Stack<string> stack = new Stack<string>();
                Expression expression2 = expression.Body;
                while (expression2 != null)
                {
                    if (expression2.NodeType == ExpressionType.Call)
                    {
                        MethodCallExpression methodCallExpression = (MethodCallExpression)expression2;
                        if (!ExpressionHelper.IsSingleArgumentIndexer(methodCallExpression))
                        {
                            break;
                        }
                        stack.Push(ExpressionHelper.GetIndexerInvocation(methodCallExpression.Arguments.Single<Expression>(), expression.Parameters.ToArray<ParameterExpression>()));
                        expression2 = methodCallExpression.Object;
                    }
                    else if (expression2.NodeType == ExpressionType.ArrayIndex)
                    {
                        BinaryExpression binaryExpression = (BinaryExpression)expression2;
                        stack.Push(ExpressionHelper.GetIndexerInvocation(binaryExpression.Right, expression.Parameters.ToArray<ParameterExpression>()));
                        expression2 = binaryExpression.Left;
                    }
                    else if (expression2.NodeType == ExpressionType.MemberAccess)
                    {
                        MemberExpression memberExpression = (MemberExpression)expression2;
                        stack.Push("." + memberExpression.Member.Name);
                        expression2 = memberExpression.Expression;
                    }
                    else
                    {
                        if (expression2.NodeType != ExpressionType.Parameter)
                        {
                            break;
                        }
                        stack.Push(string.Empty);
                        expression2 = null;
                    }
                }
                if (stack.Count > 0 && string.Equals(stack.Peek(), ".model", StringComparison.OrdinalIgnoreCase))
                {
                    stack.Pop();
                }
                if (stack.Count > 0)
                {
                    return stack.Aggregate((string left, string right) => left + right).TrimStart(new char[]
                    {
                    '.'
                    });
                }
                return string.Empty;
            }

            private static string GetIndexerInvocation(Expression expression, ParameterExpression[] parameters)
            {
                Expression body = Expression.Convert(expression, typeof(object));
                ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
                Expression<Func<object, object>> lambdaExpression = Expression.Lambda<Func<object, object>>(body, new ParameterExpression[]
                {
                parameterExpression
                });
                Func<object, object> func;
                try
                {
                    func = CachedExpressionCompiler.Process<object, object>(lambdaExpression);
                }
                catch (InvalidOperationException innerException)
                {
                    throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The expression compiler was unable to evaluate the indexer expression &apos;{0}&apos; because it references the model parameter &apos;{1}&apos; which is unavailable..", new object[]
                    {
                    expression,
                    parameters[0].Name
                    }), innerException);
                }
                return "[" + Convert.ToString(func(null), CultureInfo.InvariantCulture) + "]";
            }

            internal static bool IsSingleArgumentIndexer(Expression expression)
            {
                MethodCallExpression methodExpression = expression as MethodCallExpression;
                return methodExpression != null && methodExpression.Arguments.Count == 1 && methodExpression.Method.DeclaringType.GetDefaultMembers().OfType<PropertyInfo>().Any((PropertyInfo p) => p.GetGetMethod() == methodExpression.Method);
            }
        }
    
}
#pragma warning restore CS1591
