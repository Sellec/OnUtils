using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Data
{
    /// <summary>
    /// Описывает методы для использования сохраненных процедур и функций в источнике данных.
    /// </summary>
    public interface IDBOAccess
    {
        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры. 
        /// Результат выполнения запроса возвращается в виде перечисления объектов типа <typeparamref name="TEntity"/>.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        IEnumerable<TEntity> StoredProcedure<TEntity>(string procedure_name, object parameters = null) where TEntity : class;

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>> StoredProcedure<TEntity1, TEntity2>(string procedure_name, object parameters = null) 
            where TEntity1 : class 
            where TEntity2 : class;

        /// <summary>
        /// Возвращает результат выполнения сохраненной процедуры, возвращающей несколько наборов данных. 
        /// Результат выполнения запроса возвращается в виде нескольких перечислений объектов указанных типов.
        /// Результат выполнения запроса не кешируется.
        /// </summary>
        /// <param name="procedure_name">Название сохраненной процедуры.</param>
        /// <param name="parameters">
        /// Объект, содержащий свойства с именами, соответствующими параметрам сохраненной процедуры.
        /// Это может быть анонимный тип, например, для СП с параметром "@Date" объявленный так: new { Date = DateTime.Now }
        /// </param>
        Tuple<IEnumerable<TEntity1>, IEnumerable<TEntity2>, IEnumerable<TEntity3>> StoredProcedure<TEntity1, TEntity2, TEntity3>(string procedure_name, object parameters = null)
            where TEntity1 : class
            where TEntity2 : class
            where TEntity3 : class;

    }
}
