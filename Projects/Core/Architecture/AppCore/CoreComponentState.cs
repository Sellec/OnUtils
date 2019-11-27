namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Описывает состояние компонента ядра.
    /// </summary>
    public enum CoreComponentState
    {
        /// <summary>
        /// Не запущен.
        /// </summary>
        None,

        /// <summary>
        /// Запускается.
        /// </summary>
        Starting,

        /// <summary>
        /// Запущен.
        /// </summary>
        Started,

        /// <summary>
        /// Остановлен.
        /// </summary>
        Stopped
    }
}
