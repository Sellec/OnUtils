using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Представляет диапазон чисел.
    /// </summary>
    public class DecimalRange : BaseRange<decimal>
    {
        private class c : IComparer<decimal>
        {
            public int Compare(decimal x, decimal y)
            {
                return x < y ? -1 : (x > y ? 1 : 0);
            }
        }

        /// <summary>
        /// </summary>
        public DecimalRange(decimal start, decimal end) : base(start, end, new c())
        {
        }

        /// <summary>
        /// </summary>
        public DecimalRange(string sourceValue, string separator) : base(sourceValue, separator, new c())
        {
        }

        /// <summary>
        /// </summary>
        protected override decimal Convert(string source)
        {
            return string.IsNullOrEmpty(source) ? 0 : decimal.Parse(source);
        }

        /// <summary>
        /// </summary>
        protected override bool TryConvert(string source, out decimal value)
        {
            return decimal.TryParse(source, out value);
        }

    }
}
