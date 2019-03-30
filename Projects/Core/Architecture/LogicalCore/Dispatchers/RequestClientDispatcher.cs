using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    #pragma warning disable CS0618

    /// <summary>
    /// Диспетчер запросов к операциям. Все запросы к операциям создаются методами данного диспетчера.
    /// Диспетчер автоматически определяет доступные локальные или удаленные логические ядра и отправляет запрос в наиболее подходящее ядро.
    /// </summary>
    public static class RequestClientDispatcher
    {
        #region Dispatch
        private static RequestLocalDispatcher _localDispatcher = new RequestLocalDispatcher();

        private static IEnumerable<IRequestRemoteDispatcher> GetDispatchersList()
        {
            return new List<IRequestRemoteDispatcher>() { RemoteDispatcher, _localDispatcher }.Where(x => x != null);
        }

        private static Task<TResponse> ReturnConnectError<TResponse>() where TResponse : Response, new()
        {
            return Task.Factory.StartNew<TResponse>(() => new TResponse() { Success = false, Message = "Не удалось найти ни одного подключения к ядрам логики.", ReturnCode = ResponseReturnCode.ClientUnknownError });
        }

        /// <summary>
        /// Задает или возвращает текущий диспетчер запросов к удаленным ядрам.
        /// </summary>
        public static IRequestRemoteDispatcher RemoteDispatcher = null;

        /// <summary>
        /// Создает новый экземпляр запроса на создание в ядре логики новой операции, реализующей интерфейс <typeparamref name="TActivity"/>.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в ядре логики.</typeparam>
        /// <returns>Объект запроса для указания дальнейших действий - доп. вызов метода в операции, отправка запроса.</returns>
        public static RequestStart<TActivity> Start<TActivity>() where TActivity : IActivityBase
        {
            return new RequestStart<TActivity>() { activityType = typeof(TActivity), isStart = true };
        }

        /// <summary>
        /// Создает новый экземпляр запроса на поиск в ядре логики операции с идентификатором <paramref name="uniqueID"/>, реализующей интерфейс <typeparamref name="TActivity"/>.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в ядре логики.</typeparam>
        /// <returns>Объект запроса для указания дальнейших действий - доп. вызов метода в операции, отправка запроса.</returns>
        public static RequestExecutePre<TActivity> FindByID<TActivity>(Guid uniqueID) where TActivity : IActivityLongBase
        {
            return new RequestExecutePre<TActivity>() { activityType = typeof(TActivity), isFind = true, guid = uniqueID };
        }

        /// <summary>
        /// Обращается к продолжительной операции в удаленном ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти в удаленном ядре логики.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        internal static Task<Response<TResult>> Execute<TActivity, TResult>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Execute<TActivity, TResult>(uniqueID, activityCall);
            }
            return ReturnConnectError<Response<TResult>>();
        }

        /// <summary>
        /// Обращается к продолжительной операции в удаленном ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти в удаленном ядре логики.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        internal static Task<Response<TResult, TCallResult>> Execute<TActivity, TResult, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Execute<TActivity, TResult, TCallResult>(uniqueID, activityCall);
            }
            return ReturnConnectError<Response<TResult, TCallResult>>();
        }

        /// <summary>
        /// Обращается к продолжительной операции в удаленном ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти в удаленном ядре логики.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        internal static Task<ResponseForLongState<TResult, TState>> Execute<TActivity, TResult, TState>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Execute<TActivity, TResult, TState>(uniqueID, activityCall);
            }
            return ReturnConnectError<ResponseForLongState<TResult, TState>>();
        }

        /// <summary>
        /// Обращается к продолжительной операции в удаленном ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти в удаленном ядре логики.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        internal static Task<ResponseForLongState<TResult, TState, TCallResult>> Execute<TActivity, TResult, TState, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Execute<TActivity, TResult, TState, TCallResult>(uniqueID, activityCall);
            }
            return ReturnConnectError<ResponseForLongState<TResult, TState, TCallResult>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<Response<TResult>> Run<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Run<TActivity, TResult>(activityCall);
            }
            return ReturnConnectError<Response<TResult>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<Response<TResult, TCallResult>> Run<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Run<TActivity, TResult, TCallResult>(activityCall);
            }
            return ReturnConnectError<Response<TResult, TCallResult>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<ResponseForLongState<TResult, TState>> Run<TActivity, TResult, TState>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Run<TActivity, TResult, TState>(activityCall);
            }
            return ReturnConnectError<ResponseForLongState<TResult, TState>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<ResponseForLongState<TResult, TState, TCallResult>> Run<TActivity, TResult, TState, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.Run<TActivity, TResult, TState, TCallResult>(activityCall);
            }
            return ReturnConnectError<ResponseForLongState<TResult, TState, TCallResult>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<Response<TResult>> RunAndClose<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityInstant<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.RunAndClose<TActivity, TResult>(activityCall);
            }
            return ReturnConnectError<Response<TResult>>();
        }

        /// <summary>
        /// Создает в удаленном ядре логики новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        internal static Task<Response<TResult, TCallResult>> RunAndClose<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityInstant<TResult>
        {
            foreach (var dispatcher in GetDispatchersList())
            {
                return dispatcher.RunAndClose<TActivity, TResult, TCallResult>(activityCall);
            }
            return ReturnConnectError<Response<TResult, TCallResult>>();
        }

        #endregion

        #region Local logical cores
        internal static List<CoreBase> _runningInstances = new List<CoreBase>();

        /// <summary>
        /// Список экземпляров ядер логики, запущенных в данный момент.
        /// </summary>
        public static ReadOnlyCollection<CoreBase> RunningInstances
        {
            get => _runningInstances.AsReadOnly();
        }

        #endregion

    }

    #pragma warning restore CS0618
}
