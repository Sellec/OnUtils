using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class HoistingExpressionVisitor<TIn, TOut> : ExpressionVisitor
	{
		private static readonly ParameterExpression _hoistedConstantsParamExpr = Expression.Parameter(typeof(List<object>), "hoistedConstants");

		private int _numConstantsProcessed;

		private HoistingExpressionVisitor()
		{
		}

		public static Expression<Hoisted<TIn, TOut>> Hoist(Expression<Func<TIn, TOut>> expr)
		{
			HoistingExpressionVisitor<TIn, TOut> hoistingExpressionVisitor = new HoistingExpressionVisitor<TIn, TOut>();
			Expression body = hoistingExpressionVisitor.Visit(expr.Body);
			return Expression.Lambda<Hoisted<TIn, TOut>>(body, new ParameterExpression[]
			{
				expr.Parameters[0],
				HoistingExpressionVisitor<TIn, TOut>._hoistedConstantsParamExpr
			});
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			return Expression.Convert(Expression.Property(HoistingExpressionVisitor<TIn, TOut>._hoistedConstantsParamExpr, "Item", new Expression[]
			{
				Expression.Constant(this._numConstantsProcessed++)
			}), node.Type);
		}
	}
}
