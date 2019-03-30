using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable CS1591
namespace OnUtils
{
    using Utils;

    public enum ResultType
    {
        Success,
        UnhandledError
    }

    public enum ResultContextType
    {
        Success,
        ContextError,
        UnhandledError
    }

    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Создает экземпляр <see cref="Result"/> с признаком успешности выполнения.
        /// </summary>
        public Result()
        {
            ResultType = ResultType.Success;
            UnknownError = null;
        }

        /// <summary>
        /// Создает экземпляр <see cref="Result"/>, сообщающий, что во время выполнения возникла непредвиденная ошибка с исключением <paramref name="exception"/>.
        /// </summary>
        public Result(Exception exception)
        {
            ResultType = ResultType.UnhandledError;
            UnknownError = exception;
        }

        public ResultType ResultType { get; }

        public Exception UnknownError { get; }
    }

    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public class ResultValue<TValue>
    {
        /// <summary>
        /// Создает экземпляр <see cref="ResultValue{TValue}"/> с признаком успешности выполнения и значением <paramref name="value"/>.
        /// </summary>
        public ResultValue(TValue value)
        {
            ResultType = ResultType.Success;
            Value = value;
            UnknownError = null;
        }

        /// <summary>
        /// Создает экземпляр <see cref="ResultValue{TValue}"/>, сообщающий, что во время выполнения возникла непредвиденная ошибка с исключением <paramref name="exception"/>.
        /// </summary>
        public ResultValue(Exception exception)
        {
            ResultType = ResultType.UnhandledError;
            Value = default(TValue);
            UnknownError = exception;
        }

        public ResultType ResultType { get; }

        public TValue Value { get; }

        public Exception UnknownError { get; }
    }

    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public class ResultContext<TContextError>
    {
        /// <summary>
        /// Создает экземпляр <see cref="ResultContext{TContextError}"/> с признаком успешности выполнения.
        /// </summary>
        public ResultContext()
        {
            ResultType = ResultContextType.Success;
            UnknownError = null;
        }

        /// <summary>
        /// Создает экземпляр <see cref="ResultContext{TContextError}"/>, сообщающий, что во время выполнения возникла ошибка <paramref name="contextError"/>.
        /// </summary>
        public ResultContext(TContextError contextError)
        {
            ResultType = ResultContextType.ContextError;
            ContextError = contextError;
        }

        /// <summary>
        /// Создает экземпляр <see cref="ResultContext{TContextError}"/>, сообщающий, что во время выполнения возникла непредвиденная ошибка с исключением <paramref name="exception"/>.
        /// </summary>
        public ResultContext(Exception exception)
        {
            ResultType = ResultContextType.UnhandledError;
            UnknownError = exception;
        }

        public ResultContextType ResultType { get; }

        public TContextError ContextError { get; }

        public Exception UnknownError { get; }
    }

    /// <summary>
    /// Представляет результат выполнения какого-либо метода или операции.
    /// </summary>
    public class ResultValueContext<TValue, TContextError>
    {
        /// <summary>
        /// Создает экземпляр <see cref="ResultValueContext{TValue, TContextError}"/> с признаком успешности выполнения.
        /// </summary>
        public ResultValueContext(TValue value)
        {
            ResultType = ResultContextType.Success;
            Value = value;
            UnknownError = null;
        }

        /// <summary>
        /// Создает экземпляр <see cref="ResultValueContext{TValue, TContextError}"/>, сообщающий, что во время выполнения возникла ошибка <paramref name="contextError"/>.
        /// </summary>
        public ResultValueContext(TContextError contextError)
        {
            ResultType = ResultContextType.ContextError;
            ContextError = contextError;
        }

        /// <summary>
        /// Создает экземпляр <see cref="ResultValueContext{TValue, TContextError}"/>, сообщающий, что во время выполнения возникла непредвиденная ошибка с исключением <paramref name="exception"/>.
        /// </summary>
        public ResultValueContext(Exception exception)
        {
            ResultType = ResultContextType.UnhandledError;
            UnknownError = exception;
        }

        public TValue Value { get; }

        public ResultContextType ResultType { get; }

        public TContextError ContextError { get; }

        public Exception UnknownError { get; }
    }
}
