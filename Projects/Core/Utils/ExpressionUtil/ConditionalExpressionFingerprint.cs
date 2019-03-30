using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class ConditionalExpressionFingerprint : ExpressionFingerprint
	{
		public ConditionalExpressionFingerprint(ExpressionType nodeType, Type type) : base(nodeType, type)
		{
		}

		public override bool Equals(object obj)
		{
			ConditionalExpressionFingerprint conditionalExpressionFingerprint = obj as ConditionalExpressionFingerprint;
			return conditionalExpressionFingerprint != null && base.Equals(conditionalExpressionFingerprint);
		}
	}
}
