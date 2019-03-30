using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.FOM.SampleModel
{
    /// <summary>
    /// Пример интерфейса манипулятора объектами типа <see cref="SampleObject"/>.
    /// </summary>
    public interface ISampleObjectManipulator : IObjectManipulator<SampleObject>
    {
        /// <summary>
        /// Записывает в лог информацию о переданном объекте.
        /// </summary>
        /// <param name="obj">Объект, информацию о котором следует записать. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Возникает, если значение <paramref name="obj"/> равно null.</exception>
        void WriteIntoLogs(SampleObject obj);
    }

    /// <summary>
    /// Реализация манипулятора объектами типа <see cref="SampleObject"/>.
    /// </summary>
    public class SampleObjectManipulator : ISampleObjectManipulator
    {
        void ISampleObjectManipulator.WriteIntoLogs(SampleObject obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
        }
    }
}
