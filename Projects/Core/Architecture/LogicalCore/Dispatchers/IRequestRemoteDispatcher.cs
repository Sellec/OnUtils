using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    /// <summary>
    /// Интерфейс диспетчера клиентских запросов, т.е. запросов от GUI, запросов от web-интерфейса (должно быть преобразование к JS), запросов от Android/iOS, к серверной части, 
    /// т.е. внутренней логике, не зависящей от реализации GUI.
    /// </summary>
    public interface IRequestRemoteDispatcher
    {
        /// <summary>
        /// Обращается к продолжительной операции в удаленном ядре логики, реализующей интерфейс <typeparamref name="TActivity"/>, с идентификатором <paramref name="uniqueID"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="uniqueID">Идентификатор операции, которую следует найти в удаленном ядре логики.</param>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Не может быть null.</param>
        /// <exception cref="ArgumentNullException">Генерируется, если <paramref name="activityCall"/> равен null.</exception>
        Task<Response<TResult>> Execute<TActivity, TResult>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>;

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
        Task<Response<TResult, TCallResult>> Execute<TActivity, TResult, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>;

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
        Task<ResponseForLongState<TResult, TState>> Execute<TActivity, TResult, TState>(Guid uniqueID, Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>;

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
        Task<ResponseForLongState<TResult, TState, TCallResult>> Execute<TActivity, TResult, TState, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>;

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<Response<TResult>> Run<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult>;

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<Response<TResult, TCallResult>> Run<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult>;

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<ResponseForLongState<TResult, TState>> Run<TActivity, TResult, TState>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityLong<TResult, TState>;

        /// <summary>
        /// Создает в удаленном ядре логики новую продолжительную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<ResponseForLongState<TResult, TState, TCallResult>> Run<TActivity, TResult, TState, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityLong<TResult, TState>;

        /// <summary>
        /// Создает в удаленном ядре логики новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<Response<TResult>> RunAndClose<TActivity, TResult>(Expression<Action<TActivity>> activityCall) where TActivity : IActivityInstant<TResult>;

        /// <summary>
        /// Создает в удаленном ядре логики новую моментальную операцию, реализующую интерфейс <typeparamref name="TActivity"/>, выполняет <paramref name="activityCall"/>,
        /// возвращает задачу, представляющую запрос к удаленному ядру с результатом выполнения запроса.
        /// Операция после выполнения автоматически закрывается.
        /// </summary>
        /// <typeparam name="TActivity">Интерфейс операции, реализацию которого следует найти в удаленном ядре логики.</typeparam>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TCallResult">Тип возвращаемого значения в методе, вызываемом в <paramref name="activityCall"/>.</typeparam>
        /// <param name="activityCall">Выражение, содержащее вызов одного из методов интерфейса <typeparamref name="TActivity"/>. Указанный вызов выполняется после создания операции. Может быть null.</param>
        Task<Response<TResult, TCallResult>> RunAndClose<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall) where TActivity : IActivityInstant<TResult>;
    }
}
