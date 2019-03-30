using System.Linq.Expressions;

namespace OnUtils.Data.EntityFramework.Internal
{
    class DbHelpers
    {
        // System.Data.Entity.Internal.DbHelpers
        public static bool TryParsePath(Expression expression, out string path)
        {
            path = null;
            Expression expression2 = expression.RemoveConvert();
            MemberExpression memberExpression = expression2 as MemberExpression;
            MethodCallExpression methodCallExpression = expression2 as MethodCallExpression;
            if (memberExpression != null)
            {
                string name = memberExpression.Member.Name;
                string text;
                if (!DbHelpers.TryParsePath(memberExpression.Expression, out text))
                {
                    return false;
                }
                path = ((text == null) ? name : (text + "." + name));
            }
            else if (methodCallExpression != null)
            {
                if (methodCallExpression.Method.Name == "Select" && methodCallExpression.Arguments.Count == 2)
                {
                    string text2;
                    if (!DbHelpers.TryParsePath(methodCallExpression.Arguments[0], out text2))
                    {
                        return false;
                    }
                    if (text2 != null)
                    {
                        LambdaExpression lambdaExpression = methodCallExpression.Arguments[1] as LambdaExpression;
                        if (lambdaExpression != null)
                        {
                            string text3;
                            if (!DbHelpers.TryParsePath(lambdaExpression.Body, out text3))
                            {
                                return false;
                            }
                            if (text3 != null)
                            {
                                path = text2 + "." + text3;
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            return true;
        }
    }
}
