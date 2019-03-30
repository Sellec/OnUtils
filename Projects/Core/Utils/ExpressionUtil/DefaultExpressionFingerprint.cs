using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class DefaultExpressionFingerprint : ExpressionFingerprint
	{
		public DefaultExpressionFingerprint(ExpressionType nodeType, Type type) : base(nodeType, type)
		{
		}

		public override bool Equals(object obj)
		{
			DefaultExpressionFingerprint defaultExpressionFingerprint = obj as DefaultExpressionFingerprint;
			return defaultExpressionFingerprint != null && base.Equals(defaultExpressionFingerprint);
		}
	}
}
