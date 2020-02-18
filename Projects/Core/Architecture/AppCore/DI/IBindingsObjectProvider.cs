namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Представляет провайдер экземпляров объектов на основе привязок типов.
    /// </summary>
    public interface IBindingsObjectProvider
    {
        /// <summary>
        /// Регистрирует новый обработчик, вызываемый при активации нового экземпляра объекта.
        /// </summary>
        void RegisterInstanceActivatingHandler(IInstanceActivatingHandler handler);

        /// <summary>
        /// Регистрирует новый обработчик, вызываемый после активации нового экземпляра объекта.
        /// </summary>
        void RegisterInstanceActivatedHandler(IInstanceActivatedHandler handler);

    }
}
