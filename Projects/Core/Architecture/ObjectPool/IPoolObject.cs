using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Architecture.ObjectPool
{
    /// <summary>
    /// Представляет объект, помещаемый в пул объектов <see cref="ObjectPool{TPoolObject}"/>.
    /// </summary>
    public interface IPoolObject
    {
    }

    /// <summary>
    /// Представляет объект в сортируемом пуле <see cref="ObjectPool{TPoolObject}"/>, где TPoolObject объявлен на базе <see cref="IPoolObjectOrdered"/>.
    /// </summary>
    public interface IPoolObjectOrdered : IPoolObject
    {
        /// <summary>
        /// Желаемый порядковый номер в пуле, возвращаемый объектом. Пул объектов сортирует список на основе порядковых номеров объектов по возрастанию.
        /// </summary>
        uint OrderInPool { get; }
    }

    /// <summary>
    /// Представляет объект, помещаемый в пул объектов <see cref="ObjectPool{TPoolObject}"/>, с дополнительным методом инициализации, вызываемым пулом при инициализации объекта.
    /// </summary>
    public interface IPoolObjectInit : IPoolObject
    {
        /// <summary>
        /// Метод, вызываемый при инициализации объекта.
        /// </summary>
        void Init();
    }
}
