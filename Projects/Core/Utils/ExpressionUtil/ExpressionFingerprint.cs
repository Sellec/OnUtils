using System;
using System.Linq.Expressions;

namespace System.Web.Mvc.ExpressionUtil
{
	internal abstract class ExpressionFingerprint
	{
		public ExpressionType NodeType
		{
			get;
			private set;
		}

		public Type Type
		{
			get;
			private set;
		}

		protected ExpressionFingerprint(ExpressionType nodeType, Type type)
		{
			this.NodeType = nodeType;
			this.Type = type;
		}

		internal virtual void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddInt32((int)this.NodeType);
			combiner.AddObject(this.Type);
		}

		protected bool Equals(ExpressionFingerprint other)
		{
			return other != null && this.NodeType == other.NodeType && object.Equals(this.Type, other.Type);
		}

		public override bool Equals(object obj)
		{
			return this.Equals(obj as ExpressionFingerprint);
		}

		public override int GetHashCode()
		{
			HashCodeCombiner hashCodeCombiner = new HashCodeCombiner();
			this.AddToHashCodeCombiner(hashCodeCombiner);
			return hashCodeCombiner.CombinedHash;
		}
	}
}
