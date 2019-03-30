namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Представляет общий интерфейс компонента ядра.
    /// </summary>
    public interface IComponent<out TAppCore>
    {
        /// <summary>
        /// Вызывается при остановке/удалении компонента.
        /// </summary>
        void Stop();

        /// <summary>
        /// Возвращает текущее состояние компонента ядра.
        /// </summary>
        /// <returns></returns>
        CoreComponentState GetState();

        /// <summary>
        /// Возвращает объект ядра, к которому привязан компонент.
        /// </summary>
        /// <returns></returns>
        TAppCore GetAppCore();
    }

    /// <summary>
    /// Представляет общий интерфейс компонента ядра, для которого в ядре может существовать только один экземпляр.
    /// </summary>
    public interface IComponentSingleton<out TAppCore> : IComponent<TAppCore>
    {

    }

    /// <summary>
    /// Представляет общий интерфейс компонента ядра, для которого в ядре может существовать множество экземпляров.
    /// </summary>
    public interface IComponentTransient<out TAppCore> : IComponent<TAppCore>
    {

    }


}

