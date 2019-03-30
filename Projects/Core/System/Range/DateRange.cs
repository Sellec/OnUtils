using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    /// <summary>
    /// Представляет диапазон дат с начальной и конечной датой.
    /// </summary>
    public class DateRange : IRange<DateTime>
    {
        /// <summary>
        /// Возвращает новый объект <see cref="DateRange"/> на основе начала периода <paramref name="start"/> и конца периода <paramref name="end"/>.
        /// </summary>
        public static DateRange FromDates(DateTime start, DateTime end)
        {
            return new DateRange(start, end);
        }

        /// <summary>
        /// Возвращает новый объект <see cref="DateRange"/> на основе начала периода <paramref name="start"/> и конца периода <paramref name="end"/>.
        /// Значения автоматически выравниваются - если <paramref name="end"/> меньше, чем <paramref name="start"/>, то в качестве начала периода будет использован <paramref name="end"/>, а в качестве конца периода - <paramref name="start"/>.
        /// </summary>
        public DateRange(DateTime start, DateTime end)
        {
            Start = end > start ? start : end;
            End = end > start ? end : start;
        }

        /// <summary>
        /// Проверяет, входит ли указанная дата <paramref name="value"/> в текущий диапазон.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Includes(DateTime value)
        {
            return (Start <= value) && (value <= End);
        }

        /// <summary>
        /// Проверяет, входит ли указанный диапазон дат <paramref name="range"/> в текущий диапазон.
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public bool Includes(IRange<DateTime> range)
        {
            return (Start <= range.Start) && (range.End <= End);
        }

        /// <summary>
        /// Дата начала периода.
        /// </summary>
        public DateTime Start { get; private set; }

        /// <summary>
        /// Дата конца периода.
        /// </summary>
        public DateTime End { get; private set; }

        /// <summary>
        /// </summary>
        public override string ToString()
        {
            return $"{Start.ToString()} - {End.ToString()}";
        }

    }
}
