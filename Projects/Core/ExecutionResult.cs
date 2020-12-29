namespace OnUtils
{
    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public struct ExecutionResult
    {
        /// <summary>
        /// </summary>
        public ExecutionResult(bool isSuccess, string message = null)
        {
            IsSuccess = isSuccess;
            Message = message;
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
    public struct ExecutionResult<TResult>
    {
        /// <summary>
        /// </summary>
        public ExecutionResult(bool isSuccess, string message = null, TResult result = default(TResult))
        {
            IsSuccess = isSuccess;
            Message = message;
            Result = result;
        }

        /// <summary>
        /// Признак успешности выполнения.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Текстовое сообщение об ошибке или об успехе. Может быть пустым.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Данные, полученные в процессе выполнения.
        /// </summary>
        public TResult Result { get; }
    }
}
