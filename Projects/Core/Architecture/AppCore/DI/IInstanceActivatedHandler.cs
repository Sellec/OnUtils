namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Представляет обработчик, вызываемый при активации нового экземпляра объекта.
    /// </summary>
    public interface IInstanceActivatedHandler
    {
        /// <summary>
        /// Вызывается, когда создан новый экземпляр объекта <paramref name="instance"/> на основании затребованного типа <typeparamref name="TRequestedType"/>.
        /// </summary>
        void OnInstanceActivated<TRequestedType>(object instance);
    }
}
