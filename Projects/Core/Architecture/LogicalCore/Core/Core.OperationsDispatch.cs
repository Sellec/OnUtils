using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    #pragma warning disable CS0612
    public partial class CoreBase
    {
        internal bool CheckConstraints<TActivity>()
        {
            var constraint = TraceStudio.Utils.Types.TypeHelpers.ExtractGenericInterface(typeof(TActivity), _constraintType);
            if (constraint != null) return true;

            return typeof(TActivity).GetInterfaces().Any(x => x == _constraintType);
        }

        /// <summary>
        /// </summary>
        protected abstract bool ExecuteCommonCache<TActivity, TResult, TResponse>(Guid uniqueID, out TResponse response)
            where TResponse : Response<TResult>
            where TActivity : IActivityLong<TResult>;

        /// <summary>
        /// </summary>
        protected abstract TResponse ExecuteCommon<TActivity, TResult, TResponse>(Guid uniqueID, Type baseTypeLong, Func<TActivity, TResponse> specificExecutionMethod)
            where TResponse : Response<TResult>
            where TActivity : IActivityLong<TResult>;

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public Response<TResult> Execute<TActivity, TResult>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>
        {
            return ExecuteCommon<TActivity, TResult, Response<TResult>>(uniqueID, typeof(IActivityLong<TResult>), activity =>
            {
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    compiled.Invoke(activity);
                }

                return new Response<TResult>() { Success = true, Message = null };
            });
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public Response<TResult, TCallResult> Execute<TActivity, TResult, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>
        {
            return ExecuteCommon<TActivity, TResult, Response<TResult, TCallResult>>(uniqueID, typeof(IActivityLong<TResult>), activity =>
            {
                var callResult = default(TCallResult);
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    callResult = compiled.Invoke(activity);
                }

                return new Response<TResult, TCallResult>() { Success = true, Message = null, CallResult = callResult };
            });
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public ResponseForLongState<TResult, TState> Execute<TActivity, TResult, TState>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            return ExecuteCommon<TActivity, TResult, ResponseForLongState<TResult, TState>>(uniqueID, typeof(IActivityLong<TResult, TState>), activity =>
            {
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    compiled.Invoke(activity);
                }

                return new ResponseForLongState<TResult, TState>() { Success = true, Message = null, ActivityState = activity.GetState() };
            });
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public ResponseForLongState<TResult, TState, TCallResult> Execute<TActivity, TResult, TState, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            return ExecuteCommon<TActivity, TResult, ResponseForLongState<TResult, TState, TCallResult>>(uniqueID, typeof(IActivityLong<TResult, TState>), activity =>
            {
                var callResult = default(TCallResult);
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    callResult = compiled.Invoke(activity);
                }

                return new ResponseForLongState<TResult, TState, TCallResult>() { Success = true, Message = null, CallResult = callResult, ActivityState = activity.GetState() };
            });
        }

        /// <summary>
        /// </summary>
        protected abstract TResponse RunCommon<TActivity, TResult, TResponse>(bool isLong, Type baseTypeForLong, Func<TActivity, TResponse> specificExecutionMethod)
            where TResponse : Response<TResult>;

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public Response<TResult> Run<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>
        {
            return RunCommon<TActivity, TResult, Response<TResult>>(true, typeof(IActivityLong<TResult>), activity =>
            {
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    compiled.Invoke(activity);
                }

                return new Response<TResult>() { Success = true, Message = null };
            });
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public Response<TResult, TCallResult> Run<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>
        {
            return RunCommon<TActivity, TResult, Response<TResult, TCallResult>>(true, typeof(IActivityLong<TResult>), activity =>
            {
                var callResult = default(TCallResult);
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    callResult = compiled.Invoke(activity);
                }

                return new Response<TResult, TCallResult>() { Success = true, Message = null, CallResult = callResult };
            });
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public ResponseForLongState<TResult, TState> Run<TActivity, TResult, TState>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            return RunCommon<TActivity, TResult, ResponseForLongState<TResult, TState>>(true, typeof(IActivityLong<TResult, TState>), activity =>
            {
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    compiled.Invoke(activity);
                }

                return new ResponseForLongState<TResult, TState>() { Success = true, Message = null, ActivityState = activity.GetState() };
            });
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public ResponseForLongState<TResult, TState, TCallResult> Run<TActivity, TResult, TState, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>
        {
            return RunCommon<TActivity, TResult, ResponseForLongState<TResult, TState, TCallResult>>(true, typeof(IActivityLong<TResult, TState>), activity =>
            {
                var callResult = default(TCallResult);
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    callResult = compiled.Invoke(activity);
                }

                return new ResponseForLongState<TResult, TState, TCallResult>() { Success = true, Message = null, CallResult = callResult, ActivityState = activity.GetState() };
            });
        }

        /// <summary>
        /// Создает на сервере новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса. Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public Response<TResult> RunAndClose<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityInstant<TResult>
        {
            return RunCommon<TActivity, TResult, Response<TResult>>(false, null, activity =>
            {
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    compiled.Invoke(activity);
                }

                CloseWithDefault<TActivity, TResult>(activity);

                return new Response<TResult>() { Success = true, Message = null };
            });
        }

        /// <summary>
        /// Создает на сервере новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса. Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public Response<TResult, TCallResult> RunAndClose<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityInstant<TResult>
        {
            return RunCommon<TActivity, TResult, Response<TResult, TCallResult>>(false, null, activity =>
            {
                var callResult = default(TCallResult);
                if (activityCall != null)
                {
                    var compiled = activityCall.Compile();
                    callResult = compiled.Invoke(activity);
                }

                CloseWithDefault<TActivity, TResult>(activity);

                return new Response<TResult, TCallResult>() { Success = true, Message = null, CallResult = callResult };
            });
        }

        /// <summary>
        /// </summary>
        protected abstract void CloseWithDefault<TActivity, TResult>(TActivity activity);
    }

    public partial class Core<TOperationContract> : CoreBase
    {
        internal ConcurrentDictionary<Guid, ActivityBase> _startedActivities = new ConcurrentDictionary<Guid, ActivityBase>();
        internal ConcurrentDictionary<Guid, object> _closedActivitiesResults = new ConcurrentDictionary<Guid, object>();

        /// <summary>
        /// </summary>
        protected sealed override bool ExecuteCommonCache<TActivity, TResult, TResponse>(Guid uniqueID, out TResponse response)
        {
            var resultCandidate = ((IDictionary<Guid, object>)_closedActivitiesResults).GetValueOrDefault(uniqueID);
            if (resultCandidate == null)
            {
                response = null;
                return false;
            }

            if (resultCandidate.GetType() != typeof(TResult))
            {
                response = null;
                return false;
            }

            // todo добавить проверку на соответствие исходного типа операции запрашиваемому.

            var response2 = (TResponse)Activator.CreateInstance(typeof(TResponse), true);
            response2.Success = false;
            response2.Message = null;
            response2.UniqueID = uniqueID;
            response2.IsClosed = true;
            response2.ActivityResult = (TResult)resultCandidate;
            response2.ReturnCode = ResponseReturnCode.None;

            response = response2;
            return true;
        }

        /// <summary>
        /// </summary>
        protected sealed override TResponse ExecuteCommon<TActivity, TResult, TResponse>(Guid uniqueID, Type baseTypeLong, Func<TActivity, TResponse> specificExecutionMethod)
        {
            ActivityLong<TResult> activityBase = null;
            var responseReturn = new Func<TResponse, ResponseReturnCode?, string, TResponse>((response, code, message) =>
            {
                if (response == null) response = (TResponse)Activator.CreateInstance(typeof(TResponse), true);

                TResult result = default(TResult);
                if (activityBase != null) activityBase.TryGetResult(out result);

                response.ActivityResult = activityBase != null ? result : default(TResult);
                response.IsClosed = activityBase != null ? (bool?)activityBase.IsClosed : null;
                response.Message = code.HasValue ? message : null;
                response.ReturnCode = code.HasValue ? code.Value : ResponseReturnCode.None;
                response.Success = !code.HasValue;
                response.UniqueID = activityBase != null ? activityBase.UniqueID : Guid.Empty;

                return response;
            });

            try
            {

                var activityCandidate = ((IDictionary<Guid, ActivityBase>)_startedActivities).GetValueOrDefault(uniqueID);
                if (activityCandidate == null)
                {
                    if (ExecuteCommonCache<TActivity, TResult, TResponse>(uniqueID, out TResponse response2))
                    {
                        return response2;
                    }
                    else
                    {
                        return responseReturn(null, ResponseReturnCode.ServerUnknownOperationID, "Нет запущенной операции с указанным идентификатором.");
                    }
                }

                var activityBaseType1 = typeof(ActivityBase<TResult>);
                var types = TraceStudio.Utils.LibraryEnumeratorFactory.Enumerate<IEnumerable<Type>>(assembly => assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && activityBaseType1.IsAssignableFrom(x) && baseTypeLong.IsAssignableFrom(x) && typeof(TActivity).IsAssignableFrom(x))).SelectMany(x => x);

                var activityType = types.FirstOrDefault();
                if (activityType == null) return responseReturn(null, ResponseReturnCode.ServerUnknownOperationType, $"Неизвестный тип операции - не найдена реализация интерфейса '{typeof(TActivity).FullName}'.");

                if (activityCandidate.GetType() != activityType) responseReturn(null, ResponseReturnCode.ServerMismatchOperationType, "Тип найденной операции не соответствует запрошенному типу.");

                var activity = (TActivity)(object)activityCandidate;
                activityBase = (ActivityLong<TResult>)activityCandidate;

                TResponse responseCustom = default(TResponse);

                try
                {
                    responseCustom = specificExecutionMethod(activity);
                }
                catch (Exception)
                {
                    return responseReturn(null, ResponseReturnCode.ServerErrorUntilOperationCall, "Ошибка во время выполнения дополнительного вызова.");
                }

                return responseReturn(responseCustom, null, null);
            }
            catch (Exception ex)
            {
                return responseReturn(null, ResponseReturnCode.ServerErrorUnknown, ex.Message);
            }
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public new Response<TResult> Execute<TActivity, TResult>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>, TOperationContract
        {
            return base.Execute<TActivity, TResult>(uniqueID, activityCall);
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public new Response<TResult, TCallResult> Execute<TActivity, TResult, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>, TOperationContract
        {
            return base.Execute<TActivity, TResult, TCallResult>(uniqueID, activityCall);
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public new ResponseForLongState<TResult, TState> Execute<TActivity, TResult, TState>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>, TOperationContract
        {
            return base.Execute<TActivity, TResult, TState>(uniqueID, activityCall);
        }

        /// <summary>
        /// Обращается к продолжительной операции на сервере, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти на сервере.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        public new ResponseForLongState<TResult, TState, TCallResult> Execute<TActivity, TResult, TState, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>, TOperationContract
        {
            return base.Execute<TActivity, TResult, TState, TCallResult>(uniqueID, activityCall);
        }

        /// <summary>
        /// </summary>
        protected sealed override TResponse RunCommon<TActivity, TResult, TResponse>(bool isLong, Type baseTypeForLong, Func<TActivity, TResponse> specificExecutionMethod)
        {
            ActivityBase<TResult> activityBase = null;
            Guid? uniqueID = null;

            var responseReturn = new Func<TResponse, ResponseReturnCode?, string, TResponse>((response, code, message) =>
            {
                if (response == null) response = (TResponse)Activator.CreateInstance(typeof(TResponse), true);

                TResult result = default(TResult);
                if (activityBase != null) activityBase.TryGetResult(out result);

                response.ActivityResult = activityBase != null ? result : default(TResult);
                response.IsClosed = activityBase != null ? (bool?)activityBase.IsClosed : null;
                response.Message = code.HasValue ? message : null;
                response.ReturnCode = code.HasValue ? code.Value : ResponseReturnCode.None;
                response.Success = !code.HasValue;
                response.UniqueID = uniqueID.HasValue ? uniqueID.Value : Guid.Empty;

                return response;
            });

            try
            {
                IEnumerable<Type> types;
                if (!isLong)
                {
                    var activityBaseType = typeof(ActivityInstant<TResult>);
                    types = TraceStudio.Utils.LibraryEnumeratorFactory.Enumerate<IEnumerable<Type>>(assembly => assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && activityBaseType.IsAssignableFrom(x) && typeof(TActivity).IsAssignableFrom(x))).SelectMany(x => x);
                }
                else
                {
                    var activityBaseType1 = typeof(ActivityBase<TResult>);
                    types = TraceStudio.Utils.LibraryEnumeratorFactory.Enumerate<IEnumerable<Type>>(assembly => assembly.GetTypes().Where(x => x.IsClass && !x.IsAbstract && activityBaseType1.IsAssignableFrom(x) && baseTypeForLong.IsAssignableFrom(x) && typeof(TActivity).IsAssignableFrom(x))).SelectMany(x => x);
                }

                var activityType = types.FirstOrDefault();
                if (activityType == null) return responseReturn(null, ResponseReturnCode.ServerUnknownOperationType, "Неизвестный тип операции.");

                if (isLong && !typeof(ActivityLong<TResult>).IsAssignableFrom(activityType)) return responseReturn(null, ResponseReturnCode.ServerWrongOperationType, "Неподходящий тип операции - продолжительные операции должны наследоваться от ActivityLong<> или ActivityLong<,>.");

                if (!isLong && !typeof(ActivityInstant<TResult>).IsAssignableFrom(activityType)) return responseReturn(null, ResponseReturnCode.ServerWrongOperationType, "Неподходящий тип операции - мгновенные операции должны наследоваться от ActivityInstant<>.");

                var constructor = activityType.GetConstructor(new Type[] { });
                if (constructor == null) return responseReturn(null, ResponseReturnCode.ServerWrongOperationType, "Невозможно создать операцию - нет открытого конструктора!");

                TActivity activity = default(TActivity);
                try
                {
                    activity = (TActivity)constructor.Invoke(new object[] { });
                    activityBase = (ActivityBase<TResult>)(object)activity;
                    activityBase.logicalCoreOwner = this;
                    (activity as ModularCore.ICoreComponentMultipe).Start(_modularCore);
                }
                catch (Exception)
                {
                    try { activityBase?.Close(default(TResult)); } catch { }
                    activityBase = null;
                    return responseReturn(null, ResponseReturnCode.ServerErrorUntilOperationCreate, "Ошибка во время создания операции.");
                }

                uniqueID = isLong ? (activity as ActivityLong<TResult>).UniqueID : Guid.Empty;

                if (isLong)
                {
                    _startedActivities.AddOrUpdate(uniqueID.Value, activityBase, (k, o) => activityBase);
                }

                TResponse responseCustom = default(TResponse);
                try
                {
                    responseCustom = specificExecutionMethod(activity);
                    try { if (!isLong && !activityBase.IsClosed) activityBase.Close(default(TResult)); } catch { }
                }
                catch (Exception)
                {
                    try { if (!activityBase.IsClosed) activityBase.Close(default(TResult)); } catch { }
                    activityBase = null;
                    return responseReturn(null, ResponseReturnCode.ServerErrorUntilOperationCall, "Ошибка во время выполнения дополнительного вызова.");
                }

                return responseReturn(responseCustom, null, null);
            }
            catch (Exception ex)
            {
                return responseReturn(null, ResponseReturnCode.ServerErrorUnknown, ex.Message);
            }
            finally
            {
                try { if (activityBase != null && !isLong && !activityBase.IsClosed) activityBase.Close(default(TResult)); } catch { }
                activityBase = null;
            }
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new Response<TResult> Run<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>, TOperationContract
        {
            return base.Run<TActivity, TResult>(activityCall);
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new Response<TResult, TCallResult> Run<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>, TOperationContract
        {
            return base.Run<TActivity, TResult, TCallResult>(activityCall);
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new ResponseForLongState<TResult, TState> Run<TActivity, TResult, TState>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>, TOperationContract
        {
            return base.Run<TActivity, TResult, TState>(activityCall);
        }

        /// <summary>
        /// Создает на сервере новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new ResponseForLongState<TResult, TState, TCallResult> Run<TActivity, TResult, TState, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>, TOperationContract
        {
            return base.Run<TActivity, TResult, TState, TCallResult>(activityCall);
        }

        /// <summary>
        /// Создает на сервере новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса. Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new Response<TResult> RunAndClose<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityInstant<TResult>, TOperationContract
        {
            return base.RunAndClose<TActivity, TResult>(activityCall);
        }

        /// <summary>
        /// Создает на сервере новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает результат выполнения запроса. Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти на сервере.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        public new Response<TResult, TCallResult> RunAndClose<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityInstant<TResult>, TOperationContract
        {
            return base.RunAndClose<TActivity, TResult, TCallResult>(activityCall);
        }

        /// <summary>
        /// </summary>
        protected sealed override void CloseWithDefault<TActivity, TResult>(TActivity activity)
        {
            var activityBase = (ActivityBase<TResult>)(object)activity;
            if (!activityBase.IsClosed) activityBase.Close(default(TResult));
        }
    }

    #pragma warning restore CS0612
}
