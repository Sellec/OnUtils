using System.Linq.Expressions;

namespace OnUtils.Data.EntityFramework.Internal
{
    static class ExpressionExtensions
    {
        // System.Data.Entity.Utilities.ExpressionExtensions
        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression.NodeType == ExpressionType.Convert || expression.NodeType == ExpressionType.ConvertChecked)
            {
                expression = ((UnaryExpression)expression).Operand;
            }
            return expression;
        }

    }
}