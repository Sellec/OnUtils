namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Описывает состояние компонента ядра.
    /// </summary>
    public enum CoreComponentState
    {
        /// <summary>
        /// Компонент не запущен.
        /// </summary>
        None,

        /// <summary>
        /// Компонент запущен.
        /// </summary>
        Started,

        /// <summary>
        /// Компонент остановлен.
        /// </summary>
        Stopped
    }
}
