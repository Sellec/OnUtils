using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class ParameterExpressionFingerprint : ExpressionFingerprint
	{
		public int ParameterIndex
		{
			get;
			private set;
		}

		public ParameterExpressionFingerprint(ExpressionType nodeType, Type type, int parameterIndex) : base(nodeType, type)
		{
			this.ParameterIndex = parameterIndex;
		}

		public override bool Equals(object obj)
		{
			ParameterExpressionFingerprint parameterExpressionFingerprint = obj as ParameterExpressionFingerprint;
			return parameterExpressionFingerprint != null && this.ParameterIndex == parameterExpressionFingerprint.ParameterIndex && base.Equals(parameterExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddInt32(this.ParameterIndex);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
