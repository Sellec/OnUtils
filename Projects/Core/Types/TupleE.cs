using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Представляет изменяемый вариант <see cref="Tuple{T1, T2}"/>.
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <typeparam name="T2"></typeparam>
    public class TupleE<T1, T2>
    {
        /// <summary>
        /// См. <see cref="Tuple{T1, T2}.Tuple(T1, T2)"/>.
        /// </summary>
        public TupleE(T1 item1 = default(T1), T2 item2 = default(T2))
        {
            this.Item1 = item1;
            this.Item2 = item2;
        }

        /// <summary>
        /// См. <see cref="Tuple{T1, T2}.Item1"/>.
        /// </summary>
        public T1 Item1 { get; set; }

        /// <summary>
        /// См. <see cref="Tuple{T1, T2}.Item2"/>.
        /// </summary>
        public T2 Item2 { get; set; }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}, {1}", Item1, Item2);
        }
    }
}