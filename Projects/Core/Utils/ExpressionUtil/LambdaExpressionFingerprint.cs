using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class LambdaExpressionFingerprint : ExpressionFingerprint
	{
		public LambdaExpressionFingerprint(ExpressionType nodeType, Type type) : base(nodeType, type)
		{
		}

		public override bool Equals(object obj)
		{
			LambdaExpressionFingerprint lambdaExpressionFingerprint = obj as LambdaExpressionFingerprint;
			return lambdaExpressionFingerprint != null && base.Equals(lambdaExpressionFingerprint);
		}
	}
}
