using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class MethodCallExpressionFingerprint : ExpressionFingerprint
	{
		public MethodInfo Method
		{
			get;
			private set;
		}

		public MethodCallExpressionFingerprint(ExpressionType nodeType, Type type, MethodInfo method) : base(nodeType, type)
		{
			this.Method = method;
		}

		public override bool Equals(object obj)
		{
			MethodCallExpressionFingerprint methodCallExpressionFingerprint = obj as MethodCallExpressionFingerprint;
			return methodCallExpressionFingerprint != null && object.Equals(this.Method, methodCallExpressionFingerprint.Method) && base.Equals(methodCallExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.Method);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
