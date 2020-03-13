using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data
{
    /// <summary>
    /// Предоставляет методы для работы с областью транзакции.
    /// Все применения изменений, выполняемые внутри области, будут объединены в общую транзакцию.
    /// Если метод <see cref="Commit"/> не будет вызван, то во время освобождения объекта области (<see cref="IDisposable.Dispose"/>) транзакция будет откачена.
    /// Оказывает действие только на работу следующих методов:
    ///     1. <see cref="UnitOfWorkBase.SaveChanges()"/>;
    ///     2. <see cref="UnitOfWorkBase.SaveChanges(Type)"/>;
    ///     3. <see cref="UnitOfWorkBase.SaveChanges{TEntity}"/>;
    ///     
    /// </summary>
    public interface ITransactionScope : IDisposable
    {
        /// <summary>
        /// Указывает, что все операции в области успешно завершены.
        /// </summary>
        void Commit();
    }
}
