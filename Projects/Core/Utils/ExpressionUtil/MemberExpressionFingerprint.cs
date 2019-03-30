using System;
using System.Linq.Expressions;
using System.Reflection;

#pragma warning disable CS0659
namespace System.Web.Mvc.ExpressionUtil
{
	internal sealed class MemberExpressionFingerprint : ExpressionFingerprint
	{
		public MemberInfo Member
		{
			get;
			private set;
		}

		public MemberExpressionFingerprint(ExpressionType nodeType, Type type, MemberInfo member) : base(nodeType, type)
		{
			this.Member = member;
		}

		public override bool Equals(object obj)
		{
			MemberExpressionFingerprint memberExpressionFingerprint = obj as MemberExpressionFingerprint;
			return memberExpressionFingerprint != null && object.Equals(this.Member, memberExpressionFingerprint.Member) && base.Equals(memberExpressionFingerprint);
		}

		internal override void AddToHashCodeCombiner(HashCodeCombiner combiner)
		{
			combiner.AddObject(this.Member);
			base.AddToHashCodeCombiner(combiner);
		}
	}
}
