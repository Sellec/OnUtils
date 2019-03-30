using System;
using System.Linq.Expressions;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    #pragma warning disable CS0618

    /// <summary>
    /// Запрос на создание операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>.
    /// </summary>
    public interface IRequestStart<out TActivity> where TActivity : IActivityBase
    {
    }

    /// <summary>
    /// Запрос на создание операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>.
    /// </summary>
    public sealed class RequestStart<TActivity> : RequestBase, IRequestStart<TActivity> where TActivity : IActivityBase
    {
        /// <summary>
        /// Дополняет запрос указанием на вызов дополнительного метода.
        /// </summary>
        public RequestStart<TActivity> WithCall<TCallResult>(Expression<Action<TActivity>> activityCall)
        {
            if (activityCall == null) throw new ArgumentNullException(nameof(activityCall));
            return new RequestStart<TActivity>() { call2 = activityCall, activityType = typeof(TActivity), isStart = true };
        }

        /// <summary>
        /// Дополняет запрос указанием на вызов дополнительного метода, возвращающего результат.
        /// </summary>
        public RequestStart<TActivity, TCallResult> WithCall<TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall)
        {
            if (activityCall == null) throw new ArgumentNullException(nameof(activityCall));
            return new RequestStart<TActivity, TCallResult>() { call2 = activityCall, activityType = typeof(TActivity), isStart = true };
        }
    }

    /// <summary>
    /// Запрос на создание операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, дополненный указанием на вызов дополнительного метода, возвращающего результат.
    /// </summary>
    public interface IRequestStart<out TActivity, TCallResult> where TActivity : IActivityBase
    {
    }

    /// <summary>
    /// Запрос на создание операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, дополненный указанием на вызов дополнительного метода, возвращающего результат.
    /// </summary>
    public sealed class RequestStart<TActivity, TCallResult> : RequestBase, IRequestStart<TActivity, TCallResult> where TActivity : IActivityBase
    {
    }

    #pragma warning restore CS0618
}
