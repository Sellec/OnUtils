using System;
using System.Linq.Expressions;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class TypeBinaryExpressionFingerprint : ExpressionFingerprint
	{
		public Type TypeOperand
		{
			get;
			private set;
		}

		public TypeBinaryExpressionFingerprint(ExpressionType nodeType, Type type, Type typeOperand) : base(nodeType, type)
		{
			this.TypeOperand = typeOperand;
		}

		public override bool Equals(object obj)
		{
			TypeBinaryExpressionFingerprint typeBinaryExpressionFingerprint = obj as TypeBinaryExpressionFingerprint;
			return typeBinaryExpressionFingerprint != null && object.Equals(this.TypeOperand, typeBinaryExpressionFingerprint.TypeOperand) && base.Equals(typeBinaryExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.TypeOperand);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
