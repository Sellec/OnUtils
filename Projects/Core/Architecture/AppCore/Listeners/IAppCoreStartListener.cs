namespace OnUtils.Architecture.AppCore.Listeners
{
    /// <summary>
    /// Применим для компонентов, созданных и инициализированных во время запуска ядра.
    /// </summary>
    public interface IAppCoreStartListener
    {
        /// <summary>
        /// Вызывается по окончанию запуска ядра в случае, если компонент был создан во время запуска ядра.
        /// </summary>
        /// <remarks>На момент вызова метода статус ядра всё еще <see cref="CoreComponentState.Starting"/>.</remarks>
        void OnAppCoreStarted();
    }
}
