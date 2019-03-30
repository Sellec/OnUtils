using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Data
{
    /// <summary>
    /// Наследование от этого интерфейса позволяет получать экземпляр <typeparamref name="TUnitOfWork"/> путем вызова только одного метода <see cref="UnitOfWorkAccessorExtension.CreateUnitOfWork{TUnitOfWork}(IUnitOfWorkAccessor{TUnitOfWork})"/> в любом месте класса.
    /// </summary>
    /// <typeparam name="TUnitOfWork"></typeparam>
    public interface IUnitOfWorkAccessor<TUnitOfWork> where TUnitOfWork : UnitOfWorkBase, new()
    {
    }
}

namespace System
{
    using OnUtils.Data;

    /// <summary>
    /// Предоставляет методы расширений для <see cref="IUnitOfWorkAccessor{TUnitOfWork}"/>.
    /// </summary>
    public static class UnitOfWorkAccessorExtension
    {
        /// <summary>
        /// Создает экземпляр контейнера <typeparamref name="TUnitOfWork"/>.
        /// </summary>
        public static TUnitOfWork CreateUnitOfWork<TUnitOfWork>(this IUnitOfWorkAccessor<TUnitOfWork> accessor) where TUnitOfWork : UnitOfWorkBase, new()
        {
            return new TUnitOfWork();
        }
    }
}
