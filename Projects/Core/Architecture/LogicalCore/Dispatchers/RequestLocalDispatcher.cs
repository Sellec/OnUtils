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
    class RequestLocalDispatcher : IRequestRemoteDispatcher
    {
        private CoreBase FindLocalCore<TActivity>()
        {
            return RequestClientDispatcher.RunningInstances.Where(x => x.CheckConstraints<TActivity>()).FirstOrDefault();
        }

        private TResponse ReturnErrorNotFoundCore<TResponse>(TResponse response) where TResponse : Response
        {
            response.Success = false;
            response.ReturnCode = ResponseReturnCode.ServerWrongOperationType;
            response.Message = "Не найдено ядро, поддерживающее указанную операцию.";
            return response;
        }

        Task<Response<TResult>> IRequestRemoteDispatcher.Execute<TActivity, TResult>(Guid uniqueID, Expression<Action<TActivity>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult>()) : core.Execute<TActivity, TResult>(uniqueID, activityCall);
            });
        }

        Task<Response<TResult, TCallResult>> IRequestRemoteDispatcher.Execute<TActivity, TResult, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult, TCallResult>()) : core.Execute<TActivity, TResult, TCallResult>(uniqueID, activityCall);
            });
        }

        Task<ResponseForLongState<TResult, TState>> IRequestRemoteDispatcher.Execute<TActivity, TResult, TState>(Guid uniqueID, Expression<Action<TActivity>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new ResponseForLongState<TResult, TState>()) : core.Execute<TActivity, TResult, TState>(uniqueID, activityCall);
            });
        }

        Task<ResponseForLongState<TResult, TState, TCallResult>> IRequestRemoteDispatcher.Execute<TActivity, TResult, TState, TCallResult>(Guid uniqueID, Expression<Func<TActivity, TCallResult>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new ResponseForLongState<TResult, TState, TCallResult>()) : core.Execute<TActivity, TResult, TState, TCallResult>(uniqueID, activityCall);
            });
        }

        Task<Response<TResult>> IRequestRemoteDispatcher.Run<TActivity, TResult>(Expression<Action<TActivity>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult>()) : core.Run<TActivity, TResult>(activityCall);
            });
        }

        Task<Response<TResult, TCallResult>> IRequestRemoteDispatcher.Run<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult, TCallResult>()) : core.Run<TActivity, TResult, TCallResult>(activityCall);
            });
        }

        Task<ResponseForLongState<TResult, TState>> IRequestRemoteDispatcher.Run<TActivity, TResult, TState>(Expression<Action<TActivity>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new ResponseForLongState<TResult, TState>()) : core.Run<TActivity, TResult, TState>(activityCall);
            });
        }

        Task<ResponseForLongState<TResult, TState, TCallResult>> IRequestRemoteDispatcher.Run<TActivity, TResult, TState, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new ResponseForLongState<TResult, TState, TCallResult>()) : core.Run<TActivity, TResult, TState, TCallResult>(activityCall);
            });
        }

        Task<Response<TResult>> IRequestRemoteDispatcher.RunAndClose<TActivity, TResult>(Expression<Action<TActivity>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult>()) : core.RunAndClose<TActivity, TResult>(activityCall);
            });
        }

        Task<Response<TResult, TCallResult>> IRequestRemoteDispatcher.RunAndClose<TActivity, TResult, TCallResult>(Expression<Func<TActivity, TCallResult>> activityCall)
        {
            return Task.Factory.StartNew(() =>
            {
                var core = FindLocalCore<TActivity>();
                return core == null ? ReturnErrorNotFoundCore(new Response<TResult, TCallResult>()) : core.RunAndClose<TActivity, TResult, TCallResult>(activityCall);
            });
        }
    }
}

