using System;
using System.Linq.Expressions;

using Modular = TraceStudio.Utils.Architecture.Core.Modular;

#pragma warning disable CS0618
namespace TraceStudio.Utils.Architecture.LogicalCore
{
    public partial class Core<TOperationContract>
    {
        /// <summary>
        /// Базовый класс для операций, для внутреннего пользования. Следует пользоваться наследниками - <see cref="ActivityInstant{TResult}"/> и <see cref="ActivityLong{TResult, TState}"/>.
        /// </summary>
        public abstract class ActivityBase : ModularCore.CoreComponentBase, ModularCore.ICoreComponentMultipe, IActivityBase
        {
            internal bool _hasResult = false;
            internal object _result = null;
            internal Core<TOperationContract> logicalCoreOwner = null;

            /// <summary>
            /// </summary>
            internal ActivityBase()
            {
                IsClosed = false;
            }

            /// <summary>
            /// </summary>
            protected sealed override void OnStart()
            {
                OnCreateActivity();
            }

            /// <summary>
            /// </summary>
            protected sealed override void OnStop()
            {
                OnCloseActivity();
            }

            /// <summary>
            /// Вызывается при создании операции.
            /// </summary>
            protected virtual void OnCreateActivity()
            {

            }

            /// <summary>
            /// Вызывается при завершении операции.
            /// </summary>
            protected virtual void OnCloseActivity()
            {

            }

            /// <summary>
            /// Указывает, завершена ли операция.
            /// </summary>
            public bool IsClosed { get; internal set; }
        }

        /// <summary>
        /// Базовый класс для операций, для внутреннего пользования. Следует пользоваться наследниками - <see cref="ActivityInstant{TResult}"/> и <see cref="ActivityLong{TResult, TState}"/>.
        /// </summary>
        public abstract class ActivityBase<TResult> : ActivityBase
        {
            /// <summary>
            /// </summary>
            internal ActivityBase()
            {
            }

            /// <summary>
            /// Завершает операцию с указанным результатом.
            /// </summary>
            /// <exception cref="InvalidOperationException">Генерируется, если операция уже завершена.</exception>
            public void Close(TResult result)
            {
                if (IsClosed) throw new InvalidOperationException("Операция уже завершена.");

                _result = result;
                _hasResult = true;
                IsClosed = true;

                (this as ModularCore.ICoreComponent).Stop();

                var type = TraceStudio.Utils.Types.TypeHelpers.ExtractGenericType(this.GetType(), typeof(ActivityLong<,>));
                if (type != null)
                {
                    var activity = this as ActivityLong<TResult>;
                    logicalCoreOwner._startedActivities.TryRemove(activity.UniqueID, out var newActivity);
                    logicalCoreOwner._closedActivitiesResults.AddOrUpdate(activity.UniqueID, result, (k, v) => result);
                }
            }

            /// <summary>
            /// Позволяет получить результат выполнения операции, если операция уже была завершена.
            /// </summary>
            /// <param name="result">Возвращает результат выполнения операции, если она уже закрыта, или default(TResult), если операция еще не закрыта.</param>
            /// <returns>Возвращает true и помещает результат выполнения в <paramref name="result"/>, если операция была завершена. Возвращает false и помещает default(TResult) в <paramref name="result"/>, если операция еще не завершена.</returns>
            public bool TryGetResult(out TResult result)
            {
                result = _hasResult ? (TResult)_result : default(TResult);
                return _hasResult;
            }
        }

        /// <summary>
        /// Базовый класс для мгновенных операций.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        public abstract class ActivityInstant<TResult> : ActivityBase<TResult>, IActivityInstant<TResult>
        {

        }

        /// <summary>
        /// Базовый класс для продолжительных операций.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        public abstract class ActivityLong<TResult> : ActivityBase<TResult>, IActivityLong<TResult>
        {
            /// <summary>
            /// </summary>
            protected ActivityLong()
            {
                UniqueID = Guid.NewGuid();
            }

            /// <summary>
            /// Уникальный идентификатор операции.
            /// </summary>
            public Guid UniqueID { get; }
        }

        /// <summary>
        /// Базовый класс для продолжительных операций.
        /// </summary>
        /// <typeparam name="TResult">Тип результата операции.</typeparam>
        /// <typeparam name="TState">Тип состояния операции.</typeparam>
        public abstract class ActivityLong<TResult, TState> : ActivityLong<TResult>, IActivityLong<TResult, TState>
        {
            /// <summary>
            /// Возвращает текущее состояние операции.
            /// </summary>
            /// <returns></returns>
            public TState GetState()
            {
                return OnGetState();
            }

            #region Переопределение поведения
            /// <summary>
            /// Вызывается, когда необходимо получить текущее состояние операции.
            /// </summary>
            /// <returns></returns>
            protected virtual TState OnGetState()
            {
                return default(TState);
            }
            #endregion

        }
    }
}
#pragma warning restore CS0618
