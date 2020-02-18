using System;

namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Базовая реализация компонента ядра.
    /// </summary>
    public abstract class CoreComponentBase<TAppCore> : IComponentStartable<TAppCore>, IComponent<TAppCore>, IDisposable
    {
        #region ICoreComponent
        private CoreComponentState _state = CoreComponentState.None;

        /// <summary>
        /// Запускает компонент с указанием ядра-владельца. Запуск выполняется всего один раз.
        /// </summary>
        void IComponentStartable<TAppCore>.Start(TAppCore core)
        {
            if (_state == CoreComponentState.None)
            {
                try
                {
                    AppCore = core;
                    OnStarting();
                }
                finally
                {
                    _state = CoreComponentState.Started;
                }
            }
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.Stop"/>.
        /// </summary>
        void IComponent<TAppCore>.Stop()
        {
            (this as IDisposable).Dispose();
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.GetState"/>.
        /// </summary>
        public CoreComponentState GetState()
        {
            return _state;
        }

        /// <summary>
        /// См. <see cref="IComponent{TAppCore}.GetAppCore"/>.
        /// </summary>
        public TAppCore GetAppCore()
        {
            return AppCore;
        }
        #endregion

        #region IDisposable Support
        void IDisposable.Dispose()
        {
            if (_state == CoreComponentState.Started)
            {
                try
                {
                    OnStop();
                }
                finally
                {
                    _state = CoreComponentState.Stopped;
                }
            }
        }
        #endregion

        #region Property
        /// <summary>
        /// Объект ядра приложения, к которому относится компонент.
        /// </summary>
        public TAppCore AppCore { get; private set; }
        #endregion

        #region Переопределение при наследовании
        /// <summary>
        /// Вызывается при запуске компонента.
        /// </summary>
        /// <remarks>
        /// Если в данном методе возникает исключение, это прерывает процесс активации экземпляра объекта.
        /// </remarks>
        protected virtual void OnStarting()
        {
        }

        /// <summary>
        /// Вызывается при запуске компонента.
        /// </summary>
        /// <remarks>
        /// Если в данном методе возникает исключение, это не прерывает процесс активации экземпляра объекта.
        /// </remarks>
        internal protected virtual void OnStarted()
        {
        }

        /// <summary>
        /// Вызывается при остановке компонента, либо при вызове Dispose. Вызывается всего один раз. Все ресурсы должны освобождаться именно в этом методе. См. также <see cref="IComponent{TAppCore}.Stop"/>.
        /// </summary>
        protected virtual void OnStop()
        {
        }
        #endregion

    }

}