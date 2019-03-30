using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Описывает объект, представляющий диапазон значений типа <typeparamref name="T"/>.
    /// </summary>
    public interface IRange<T>
    {
        /// <summary>
        /// Начальное (минимальное, стартовое) значение.
        /// </summary>
        T Start { get; }

        /// <summary>
        /// Конечное (последнее, максимальное) значение.
        /// </summary>
        T End { get; }

        /// <summary>
        /// Проверяет, входит ли значение <paramref name="value"/> в диапазон.
        /// </summary>
        bool Includes(T value);

        /// <summary>
        /// Проверяет, входит ли диапазон <paramref name="range"/> в диапазон.
        /// </summary>
        bool Includes(IRange<T> range);
    }
}
