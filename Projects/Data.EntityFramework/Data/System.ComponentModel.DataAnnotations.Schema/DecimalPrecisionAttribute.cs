using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// Позволяет задать точность и размерность поля, хранящего десятичное число.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class DecimalPrecisionAttribute : Attribute
    {
        /// <summary>
        /// </summary>
        public DecimalPrecisionAttribute(byte precision, byte scale)
        {
            Precision = precision;
            Scale = scale;

        }

        /// <summary>
        /// Размерность поля (кол-во знаков в числе).
        /// </summary>
        public byte Precision { get; set; }

        /// <summary>
        /// Точность поля (кол-во знаков в дробной части).
        /// </summary>
        public byte Scale { get; set; }

    }
}
