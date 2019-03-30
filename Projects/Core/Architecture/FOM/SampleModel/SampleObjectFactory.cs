using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.FOM.SampleModel
{
    /// <summary>
    /// Пример интерфейса фабрики объектов типа <see cref="SampleObject"/>.
    /// </summary>
    public interface ISampleObjectFactory : IObjectFactory<SampleObject>
    {
        /// <summary>
        /// Создает и возвращает пустой экземпляр объекта.
        /// </summary>
        SampleObject CreateEmpty();
    }

    /// <summary>
    /// Реализация фабрики объектов типа <see cref="SampleObject"/>.
    /// </summary>
    public class SampleObjectFactory : ISampleObjectFactory
    {
        SampleObject ISampleObjectFactory.CreateEmpty()
        {
            return new SampleObject();
        }
    }
}
