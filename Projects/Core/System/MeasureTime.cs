using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Класс для измерения промежутков времени. Удобен для измерения производительности.
    /// </summary>
    public class MeasureTime
    {

        /// <summary>
        /// Автоматически вызывает <see cref="Start"/>.
        /// </summary>
        public MeasureTime()
        {
            this.StartTime = DateTime.Now;
        }

        /// <summary>
        /// Начинает новое измерение. Вызывается автоматически при создании нового экземпляра <see cref="MeasureTime"/>.
        /// </summary>
        public void Start()
        {
            this.StartTime = DateTime.Now;
        }

        /// <summary>
        /// Возвращает время с последнего вызова <see cref="Start"/>.
        /// </summary>
        /// <returns></returns>
        public TimeSpan Calculate(bool startNew = true)
        {
            var span = DateTime.Now - StartTime;
            if (startNew) Start();
            return span;

        }

        /// <summary>
        /// Дата начала измерения. Новое измерение можно начать с помощью вызова <see cref="Start"/>.
        /// </summary>
        public DateTime StartTime { get; private set; }

        /// <summary>
        /// Автоматически вызывает <see cref="Calculate"/> и выводит результат измерения в виде <see cref="TimeSpan.TotalMilliseconds"/>.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Calculate().TotalMilliseconds.ToString();
        }
    }
}