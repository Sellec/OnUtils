namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Представляет обработчик, вызываемый после активации нового экземпляра объекта.
    /// </summary>
    public interface IInstanceActivatedHandler
    {
        /// <summary>
        /// Вызывается, когда создан и инициализирован новый экземпляр объекта <paramref name="instance"/> на основании затребованного типа <typeparamref name="TRequestedType"/>.
        /// </summary>
        /// <remarks>
        /// Если в данном методе возникает исключение, это не прерывает процесс активации экземпляра объекта.
        /// </remarks>
        void OnInstanceActivated<TRequestedType>(object instance);
    }
}
