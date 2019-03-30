using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class UnaryExpressionFingerprint : ExpressionFingerprint
	{
		public MethodInfo Method
		{
			get;
			private set;
		}

		public UnaryExpressionFingerprint(ExpressionType nodeType, Type type, MethodInfo method) : base(nodeType, type)
		{
			this.Method = method;
		}

		public override bool Equals(object obj)
		{
			UnaryExpressionFingerprint unaryExpressionFingerprint = obj as UnaryExpressionFingerprint;
			return unaryExpressionFingerprint != null && object.Equals(this.Method, unaryExpressionFingerprint.Method) && base.Equals(unaryExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.Method);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
