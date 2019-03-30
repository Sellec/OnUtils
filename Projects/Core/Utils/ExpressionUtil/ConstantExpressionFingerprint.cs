using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class ConstantExpressionFingerprint : ExpressionFingerprint
	{
		public ConstantExpressionFingerprint(ExpressionType nodeType, Type type) : base(nodeType, type)
		{
		}

		public override bool Equals(object obj)
		{
			ConstantExpressionFingerprint constantExpressionFingerprint = obj as ConstantExpressionFingerprint;
			return constantExpressionFingerprint != null && base.Equals(constantExpressionFingerprint);
		}
	}
}
