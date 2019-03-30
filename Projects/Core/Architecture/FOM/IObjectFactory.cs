using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Architecture.FOM
{
    /// <summary>
    /// Общий интерфейс фабриики объектов типа <typeparamref name="T"/>. Должен выполнять только операции получения и создания объектов, то есть Create/Get. Для выполнения всех остальных операций (сохранение, удаление, всё остальное) должен существовать манипулятор (см. <see cref="IObjectManipulator{T}"/>).
    /// <para>См. также описание модели в примере <see cref="SampleModel.SampleModel"/>.</para>
    /// </summary>
    public interface IObjectFactory<T> where T : class
    {
    }
}
