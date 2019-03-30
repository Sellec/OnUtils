using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    #pragma warning disable CS0618

    /// <summary>
    /// Общий интерфейс операций. Не должен использоваться напрямую.
    /// </summary>
    [Obsolete("Этот интерфейс не должен использоваться напрямую. Используйте IActivityInstant<>, IActivityLong<>, IActivityLong<,>")]
    public interface IActivityBase
    {
    }

    /// <summary>
    /// Общий интерфейс операций. Не должен использоваться напрямую.
    /// </summary>
    [Obsolete("Этот интерфейс не должен использоваться напрямую. Используйте IActivityInstant<>, IActivityLong<>, IActivityLong<,>")]
    public interface IActivityBase<TResult> : IActivityBase
    {
    }

    /// <summary>
    /// Обозначает мгновенную операцию, т.е. операцию без состояния, не хранимую в памяти сервера. 
    /// При обращении к подобной операции создается новый экземпляр, возвращающий результат. После получения результата экземпляр уничтожается, т.е. к нему нельзя обратиться дважды.
    /// </summary>
    /// <typeparam name="TResult">Тип результата операции.</typeparam>
    public interface IActivityInstant<TResult> : IActivityBase<TResult>
    {
    }

    /// <summary>
    /// Общий интерфейс продолжительных операций. Не должен использоваться напрямую.
    /// </summary>
    [Obsolete("Этот интерфейс не должен использоваться напрямую. Используйте IActivityLong<>, IActivityLong<,>")]
    public interface IActivityLongBase : IActivityBase
    {
    }

    /// <summary>
    /// Обозначает продолжительную операцию, хранимую в памяти сервера. 
    /// При обращении к подобной операции создается новый экземпляр с уникальным идентификатором, помещаемый в память сервера. 
    /// Экземпляр уничтожается после закрытия сессии пользователя, либо вручную вызовом метода <see cref="IActivityLong{TResult}.Close(TResult)"/>, т.е. 
    /// к операции можно обращаться многократно на протяжении всего времени жизни.
    /// </summary>
    /// <typeparam name="TResult">Тип результата операции.</typeparam>
    public interface IActivityLong<TResult> : IActivityBase<TResult>, IActivityLongBase
    {
        /// <summary>
        /// Закрывает операцию с указанным результатом. 
        /// </summary>
        void Close(TResult result);

        /// <summary>
        /// Уникальный идентификатор операции.
        /// </summary>
        Guid UniqueID { get; }
    }

    /// <summary>
    /// Обозначает продолжительную операцию, т.е. операцию с состоянием, хранимую в памяти сервера. 
    /// При обращении к подобной операции создается новый экземпляр с уникальным идентификатором, помещаемый в память сервера. 
    /// Экземпляр уничтожается после закрытия сессии пользователя, либо вручную вызовом метода <see cref="IActivityLong{TResult}.Close(TResult)"/>, т.е. 
    /// к операции можно обращаться многократно на протяжении всего времени жизни.
    /// </summary>
    /// <typeparam name="TResult">Тип результата операции.</typeparam>
    /// <typeparam name="TState">Тип, хранящий данные о состоянии операции.</typeparam>
    public interface IActivityLong<TResult, TState> : IActivityLong<TResult>
    {
        /// <summary>
        /// Возвращает текущее состояние операции.
        /// </summary>
        /// <returns></returns>
        TState GetState();
    }

    #pragma warning disable CS0618
}
