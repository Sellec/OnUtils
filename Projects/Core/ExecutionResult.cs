using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils
{
    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public class ExecutionResult
    {
        /// <summary>
        /// </summary>
        public ExecutionResult(bool isSuccess, string message = null)
        {
            this.IsSuccess = isSuccess;
            this.Message = message;
        }

        /// <summary>
        /// Признак успешности выполнения.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Текстовое сообщение об ошибке или об успехе. Может быть пустым.
        /// </summary>
        public string Message { get; }
    }

    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции с данными, полученными в процессе выполнения.
    /// </summary>
    /// <typeparam name="TResult">Тип данных, полученных в процессе выполнения.</typeparam>
    public class ExecutionResult<TResult> : ExecutionResult
    {
        /// <summary>
        /// </summary>
        public ExecutionResult(bool isSuccess, string message = null, TResult result = default(TResult)) : base(isSuccess, message)
        {
            Result = result;
        }

        /// <summary>
        /// Данные, полученные в процессе выполнения.
        /// </summary>
        public TResult Result { get; }
    }

}
