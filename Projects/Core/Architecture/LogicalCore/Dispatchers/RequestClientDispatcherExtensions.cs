using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace System
{
    using TraceStudio.Utils.Architecture.LogicalCore;

    #pragma warning disable CS0618

    /// <summary>
    /// </summary>
    public static class RequestClientDispatcherExtensions
    {
        private static object InternalExecute(object query, System.Reflection.MethodInfo method, params Type[] typeArguments)
        {
            if (!(query is RequestBase)) throw new InvalidOperationException("Некорректный тип запроса.");

            var query2 = query as RequestBase;

            var typeArguments2 = new List<Type>();
            typeArguments2.Add(query2.activityType);
            typeArguments2.AddRange(typeArguments);
            var methodCompiled = method.MakeGenericMethod(typeArguments2.ToArray());
            if (query2.isFind)
            {
                return methodCompiled.Invoke(null, new object[] { query2.guid, query2.call2 });
            }
            else
            {
                return methodCompiled.Invoke(null, new object[] { query2.call2 });
            }
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<Response<TResult>> Run<TResult>(this IRequestExecute<IActivityLong<TResult>> activityStartQuery)
        {
            return (Task<Response<TResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Execute<IActivityLong<TResult>, TResult>(Guid.Empty, dx => dx.ToString())), typeof(TResult));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе дополнительного вызова.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<Response<TResult, TCallResult>> Run<TResult, TCallResult>(this RequestExecute<IActivityLong<TResult>, TCallResult> activityStartQuery)
        {
            return (Task<Response<TResult, TCallResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Execute<IActivityLong<TResult>, TResult, string>(Guid.Empty, dx => dx.ToString())), typeof(TResult), typeof(TCallResult));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<ResponseForLongState<TResult, TState>> Run<TResult, TState>(this IRequestExecute<IActivityLong<TResult, TState>> activityStartQuery)
        {
            return (Task<ResponseForLongState<TResult, TState>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Execute<IActivityLong<TResult, TState>, TResult, TState>(Guid.Empty, dx => dx.ToString())), typeof(TResult), typeof(TState));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе дополнительного вызова.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<ResponseForLongState<TResult, TState, TCallResult>> Run<TResult, TState, TCallResult>(this IRequestExecute<IActivityLong<TResult, TState>, TCallResult> activityStartQuery)
        {
            return (Task<ResponseForLongState<TResult, TState, TCallResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Execute<IActivityLong<TResult, TState>, TResult, TState, string>(Guid.Empty, dx => dx.ToString())), typeof(TResult), typeof(TState), typeof(TCallResult));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<Response<TResult>> Run<TResult>(this IRequestStart<IActivityBase<TResult>> activityStartQuery)
        {
            if (!(activityStartQuery is RequestBase)) throw new InvalidOperationException("Некорректный тип запроса.");

            var query2 = activityStartQuery as RequestBase;

            if (TraceStudio.Utils.Types.TypeHelpers.ExtractGenericInterface(query2.activityType, typeof(IActivityInstant<>)) != null)
            {
                return (Task<Response<TResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.RunAndClose<IActivityInstant<TResult>, TResult>(dx => dx.ToString())), typeof(TResult));
            }
            else if (TraceStudio.Utils.Types.TypeHelpers.ExtractGenericInterface(query2.activityType, typeof(IActivityLong<>)) != null)
            {
                return (Task<Response<TResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Run<IActivityLong<TResult>, TResult>(dx => dx.ToString())), typeof(TResult));
            }
            else
            {
                throw new InvalidOperationException("Не удается опознать тип операции.");
            }
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе дополнительного вызова.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<Response<TResult, TCallResult>> Run<TResult, TCallResult>(this IRequestStart<IActivityLong<TResult>, TCallResult> activityStartQuery)
        {
            return (Task<Response<TResult, TCallResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Run<IActivityLong<TResult>, TResult, string>(dx => dx.ToString())), typeof(TResult), typeof(TCallResult));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<ResponseForLongState<TResult, TState>> Run<TResult, TState>(this IRequestStart<IActivityLong<TResult, TState>> activityStartQuery)
        {
            return (Task<ResponseForLongState<TResult, TState>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Run<IActivityLong<TResult, TState>, TResult, TState>(dx => dx.ToString())), typeof(TResult), typeof(TState));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе дополнительного вызова.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<ResponseForLongState<TResult, TState, TCallResult>> Run<TResult, TState, TCallResult>(this IRequestStart<IActivityLong<TResult, TState>, TCallResult> activityStartQuery)
        {
            return (Task<ResponseForLongState<TResult, TState, TCallResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.Run<IActivityLong<TResult, TState>, TResult, TState, string>(dx => dx.ToString())), typeof(TResult), typeof(TState), typeof(TCallResult));
        }

        /// <summary>
        /// Запускает выполнение запроса <paramref name="activityStartQuery"/> с указанными параметрами в новой задаче.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе дополнительного вызова.</typeparam>
        /// <param name="activityStartQuery">Запрос на поиск операции.</param>
        public static Task<Response<TResult, TCallResult>> Run<TResult, TCallResult>(this IRequestStart<IActivityInstant<TResult>, TCallResult> activityStartQuery)
        {
            return (Task<Response<TResult, TCallResult>>)InternalExecute(activityStartQuery, GetClientDispatcherMethod(() => RequestClientDispatcher.RunAndClose<IActivityInstant<TResult>, TResult, string>(dx => dx.ToString())), typeof(TResult), typeof(TCallResult));
        }

        private static System.Reflection.MethodInfo GetClientDispatcherMethod(Expression<Action> action)
        {
            var body = action.Body as MethodCallExpression;
            return body.Method.GetGenericMethodDefinition();
        }

    }

    #pragma warning restore CS0618
}

