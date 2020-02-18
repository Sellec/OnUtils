namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Представляет обработчик, вызываемый при активации нового экземпляра объекта.
    /// </summary>
    public interface IInstanceActivatingHandler
    {
        /// <summary>
        /// Вызывается, когда создан новый экземпляр объекта <paramref name="instance"/> на основании затребованного типа <typeparamref name="TRequestedType"/>.
        /// </summary>
        /// <remarks>
        /// Если в данном методе возникает исключение, это прерывает процесс активации экземпляра объекта.
        /// </remarks>
        void OnInstanceActivating<TRequestedType>(object instance);
    }
}
