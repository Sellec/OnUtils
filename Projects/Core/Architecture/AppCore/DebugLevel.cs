namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Уровень детализации логов.
    /// </summary>
    public enum DebugLevel : int
    {
        /// <summary>
        /// Логи отключены.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Только общая информация
        /// </summary>
        Common = 1,

        /// <summary>
        /// Детализированная информация
        /// </summary>
        Detailed = 2
    }
}
