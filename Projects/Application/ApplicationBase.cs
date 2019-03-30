namespace OnUtils.Application
{
    using Architecture.AppCore;

    /// <summary>
    /// Ядро приложения.
    /// </summary>
    public abstract class ApplicationBase<TSelfReference> : AppCore<TSelfReference> where TSelfReference : ApplicationBase<TSelfReference>
    {
        /// <summary>
        /// </summary>
        public ApplicationBase() 
        {
        }

        #region Методы
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            OnApplicationStartBase();
            OnApplicationStart();
        }

        private void OnApplicationStartBase()
        {

        }
        #endregion

        #region Для перегрузки в реализации ядер для MVC и Core.
        /// <summary>
        /// Вызывается единственный раз при запуске ядра.
        /// </summary>
        protected abstract void OnApplicationStart();

        #endregion

    
    }
}
