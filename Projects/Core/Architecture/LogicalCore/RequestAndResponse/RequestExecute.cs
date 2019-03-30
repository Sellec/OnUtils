using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    #pragma warning disable CS0618

    /// <summary>
    /// Запрос на поиск операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с указанным идентификатором.
    /// </summary>
    public sealed class RequestExecutePre<TActivity> : RequestBase where TActivity : IActivityBase
    {
        /// <summary>
        /// Дополняет запрос указанием на вызов дополнительного метода.
        /// </summary>
        public RequestExecute<TActivity> WithCall(Expression<Action<TActivity>> activityCall)
        {
            if (activityCall == null) throw new ArgumentNullException(nameof(activityCall));
            return new RequestExecute<TActivity>() { call2 = activityCall, activityType = typeof(TActivity), isFind = true, guid = this.guid };
        }

        /// <summary>
        /// Дополняет запрос указанием на вызов дополнительного метода, возвращающего результат.
        /// </summary>
        public RequestExecute<TActivity, TCallResult> WithCall<TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall)
        {
            if (activityCall == null) throw new ArgumentNullException(nameof(activityCall));
            return new RequestExecute<TActivity, TCallResult>() { call2 = activityCall, activityType = typeof(TActivity), isFind = true, guid = this.guid };
        }
    }

    /// <summary>
    /// Запрос на поиск операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с указанным идентификатором, дополненный указанием на вызов дополнительного метода.
    /// </summary>
    public interface IRequestExecute<out TActivity> where TActivity : IActivityBase
    {
    }

    /// <summary>
    /// Запрос на поиск операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с указанным идентификатором, дополненный указанием на вызов дополнительного метода.
    /// </summary>
    public sealed class RequestExecute<TActivity> : RequestBase, IRequestExecute<TActivity> where TActivity : IActivityBase
    {
    }

    /// <summary>
    /// Запрос на поиск операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с указанным идентификатором, дополненный указанием на вызов дополнительного метода, возвращающего результат.
    /// </summary>
    public interface IRequestExecute<out TActivity, TCallResult> where TActivity : IActivityBase
    {
    }

    /// <summary>
    /// Запрос на поиск операции в ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с указанным идентификатором, дополненный указанием на вызов дополнительного метода, возвращающего результат.
    /// </summary>
    public sealed class RequestExecute<TActivity, TCallResult> : RequestBase, IRequestExecute<TActivity, TCallResult> where TActivity : IActivityBase
    {
    }

    #pragma warning restore CS0618
}
