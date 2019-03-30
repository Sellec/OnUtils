using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class IndexExpressionFingerprint : ExpressionFingerprint
	{
		public PropertyInfo Indexer
		{
			get;
			private set;
		}

		public IndexExpressionFingerprint(ExpressionType nodeType, Type type, PropertyInfo indexer) : base(nodeType, type)
		{
			this.Indexer = indexer;
		}

		public override bool Equals(object obj)
		{
			IndexExpressionFingerprint indexExpressionFingerprint = obj as IndexExpressionFingerprint;
			return indexExpressionFingerprint != null && object.Equals(this.Indexer, indexExpressionFingerprint.Indexer) && base.Equals(indexExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.Indexer);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
