using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    /// <summary>
    /// Результат выполнения запроса к серверу к операции.
    /// </summary>
    public abstract class Response
    {
        /// <summary>
        /// </summary>
        internal Response()
        {
        }

        /// <summary>
        /// Статус успешности выполнения запроса к операции. 
        /// Равен false, если во время работы клиентского или серверного диспетчера возникла ошибка, либо во время выполнения метода в операции возникло необработанное исключение. 
        /// Возвращает true, если в вызываемых методах операции не возникло исключений. Код ошибки, описывающий возникшую ситуацию, можно получить через <see cref="ReturnCode"/>.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Содержит текст ошибки, если <see cref="Success"/> равно false.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Идентификатор операции, в контексте которой выполнялся запрос. Для мгновенных операций (см. <see cref="IActivityInstant{TResult}"/>) всегда равен <see cref="Guid.Empty"/>.
        /// </summary>
        public Guid UniqueID { get; set; }

        /// <summary>
        /// Признак закрытия операции. Равен null, если диспетчеры не смогли обратиться к конкретной операции. Равен состоянию закрытия операции, если операция была найдена.
        /// </summary>
        public bool? IsClosed { get; set; }

        /// <summary>
        /// Код ответа сервера.
        /// </summary>
        public ResponseReturnCode ReturnCode { get; set; }
    }

    /// <summary>
    /// Результат выполнения операции определенного типа.
    /// </summary>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    public class Response<TResult> : Response
    {
        /// <summary>
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Результат выполнения операции.
        /// </summary>
        public TResult ActivityResult { get; set; }
    }

    /// <summary>
    /// Результат выполнения операции определенного типа с состоянием.
    /// </summary>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    /// <typeparam name="TState">Тип состояния операции для операций с состоянием.</typeparam>
    public class ResponseForLongState<TResult, TState> : Response<TResult>
    {
        /// <summary>
        /// </summary>
        public ResponseForLongState()
        {
        }

        /// <summary>
        /// Информация о состоянии операции в момент отправки ответа с сервера.
        /// </summary>
        public TState ActivityState { get; set; }
    }

    /// <summary>
    /// Результат выполнения операции определенного типа с результатом выполнения дополнительного вызова.
    /// </summary>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    /// <typeparam name="TCallResult">Тип результата дополнительного вызова.</typeparam>
    public class Response<TResult, TCallResult> : Response<TResult>
    {
        /// <summary>
        /// </summary>
        public Response()
        {
        }

        /// <summary>
        /// Результат выполнения дополнительного вызова при обращении к операции.
        /// </summary>
        public TCallResult CallResult { get; set; }
    }

    /// <summary>
    /// Результат выполнения операции определенного типа с состоянием с результатом выполнения дополнительного вызова.
    /// </summary>
    /// <typeparam name="TResult">Тип результата.</typeparam>
    /// <typeparam name="TCallResult">Тип результата дополнительного вызова.</typeparam>
    /// <typeparam name="TState">Тип состояния операции для операций с состоянием.</typeparam>
    public class ResponseForLongState<TResult, TState, TCallResult> : ResponseForLongState<TResult, TState>
    {
        /// <summary>
        /// </summary>
        public ResponseForLongState()
        {
        }

        /// <summary>
        /// Результат выполнения дополнительного вызова при обращении к операции.
        /// </summary>
        public TCallResult CallResult { get; set; }
    }

}
