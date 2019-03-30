using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class BinaryExpressionFingerprint : ExpressionFingerprint
	{
		public MethodInfo Method
		{
			get;
			private set;
		}

		public BinaryExpressionFingerprint(ExpressionType nodeType, Type type, MethodInfo method) : base(nodeType, type)
		{
			this.Method = method;
		}

		public override bool Equals(object obj)
		{
			BinaryExpressionFingerprint binaryExpressionFingerprint = obj as BinaryExpressionFingerprint;
			return binaryExpressionFingerprint != null && object.Equals(this.Method, binaryExpressionFingerprint.Method) && base.Equals(binaryExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.Method);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
