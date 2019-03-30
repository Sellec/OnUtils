using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Представляет диапазон значений.
    /// Значения автоматически выравниваются - если <see cref="BaseRange{TValue}.End"/>  меньше, чем <see cref="BaseRange{TValue}.Start"/>, то в качестве начала диапазона будет использован <see cref="BaseRange{TValue}.End"/>, а в качестве конца диапазона - <see cref="BaseRange{TValue}.Start"/>.
    /// </summary>
    public class BaseRange<TValue> : IRange<TValue>
    {
        private IComparer<TValue> _comparer = null;

        /// <summary>
        /// Возвращает новый объект на основе текстового значения диапазона.
        /// </summary>
        public BaseRange(string sourceValue, string separator, IComparer<TValue> comparer)
        {
            _comparer = comparer;

            ParseFromString(sourceValue, separator);

            Normalize();
        }

        /// <summary>
        /// Возвращает новый объект на основе начала диапазона <paramref name="start"/> и конца диапазона <paramref name="end"/>.
        /// </summary>
        public BaseRange(TValue start, TValue end, IComparer<TValue> comparer)
        {
            _comparer = comparer;

            Start = start;
            End = end;

            Normalize();
        }

        /// <summary>
        /// Нормализация значений диапазона.
        /// </summary>
        protected virtual void Normalize()
        {
            var compare = _comparer.Compare(Start, End);
            if (compare> 0)
            {
                var d = End;
                End = Start;
                Start = d;
            }
        }

        /// <summary>
        /// Загружает значения диапазона из строки. 
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="separator"></param>
        public bool ParseFromString(string sourceValue, string separator)
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceValue))
                {
                    var split = !string.IsNullOrEmpty(separator) ? sourceValue.Split(new string[] { separator }, 2, StringSplitOptions.None) : new string[] { sourceValue };
                    var from = split.Length >= 1 ? split[0] : "";
                    var to = split.Length >= 2 ? split[1] : "";

                    TValue from1, to1;
                    if (TryConvert(from, out from1) && TryConvert(to, out to1))
                    {
                        Start = from1;
                        End = to1;
                        Normalize();
                    }
                    else return false;
                }
                else
                {
                    Start = default(TValue);
                    End = default(TValue);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Проверяет, входит ли указанное значение <paramref name="value"/> в текущий диапазон.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Includes(TValue value)
        {
            var compareStart = _comparer.Compare(Start, value);
            var compareEnd = _comparer.Compare(End, value);

            return (compareStart <= 0) && (compareEnd >= 0);
        }

        /// <summary>
        /// Проверяет, входит ли указанный диапазон <paramref name="range"/> в текущий диапазон.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool Includes(IRange<TValue> range)
        {
            var compareStart = _comparer.Compare(Start, range.Start);
            var compareEnd = _comparer.Compare(End, range.End);

            return (compareStart <= 0) && (compareEnd >= 0);
        }

        /// <summary>
        /// Преобразует строковое значение в <typeparamref name="TValue"/>. В случае ошибки генерируется исключение.
        /// </summary>
        /// <param name="source">Исходное значение.</param>
        /// <returns>Преобразованное значение.</returns>
        protected virtual TValue Convert(string source)
        {
            return default(TValue);
        }

        /// <summary>
        /// Пытается преобразовать строковое значение в <typeparamref name="TValue"/>. В случае ошибки возвращает false.
        /// </summary>
        /// <param name="source">Исходное значение.</param>
        /// <param name="value">Результат преобразования.</param>
        /// <returns>Статус операции.</returns>
        protected virtual bool TryConvert(string source, out TValue value)
        {
            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Начало диапазона.
        /// </summary>
        public TValue Start { get; private set; }

        /// <summary>
        /// Конец диапазона.
        /// </summary>
        public TValue End { get; private set; }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return $"{Start.ToString()} - {End.ToString()}";
        }

    }
}
