using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.FOM
{
    /// <summary>
    /// Общий интерфейс манипулятора объектами типа <typeparamref name="T"/>. Может выполнять любые операции с уже СУЩЕСТВУЮЩИМИ объектами. Созданием и получением НОВЫХ объектов занимается фабрика соответствующих объектов (см. <see cref="IObjectFactory{T}"/>). 
    /// <para>См. также описание модели в примере <see cref="SampleModel.SampleModel"/>.</para>
    /// </summary>
    public interface IObjectManipulator<T> where T : class
    {
    }
}
