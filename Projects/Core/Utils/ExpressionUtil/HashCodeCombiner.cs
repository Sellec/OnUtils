using System;
using System.Collections;

namespace System.Web.Mvc.ExpressionUtil
{
	internal class HashCodeCombiner
	{
		private long _combinedHash64 = 5381L;

		public int CombinedHash
		{
			get
			{
				return this._combinedHash64.GetHashCode();
			}
		}

		public void AddFingerprint(ExpressionFingerprint fingerprint)
		{
			if (fingerprint != null)
			{
				fingerprint.AddToHashCodeCombiner(this);
				return;
			}
			this.AddInt32(0);
		}

		public void AddEnumerable(IEnumerable e)
		{
			if (e == null)
			{
				this.AddInt32(0);
				return;
			}
			int num = 0;
			foreach (object current in e)
			{
				this.AddObject(current);
				num++;
			}
			this.AddInt32(num);
		}

		public void AddInt32(int i)
		{
			this._combinedHash64 = ((this._combinedHash64 << 5) + this._combinedHash64 ^ (long)i);
		}

		public void AddObject(object o)
		{
			int i = (o != null) ? o.GetHashCode() : 0;
			this.AddInt32(i);
		}
	}
}
