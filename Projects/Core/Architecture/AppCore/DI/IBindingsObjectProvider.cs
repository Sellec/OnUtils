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
        void RegisterInstanceActivatedHandler(IInstanceActivatedHandler handler);
    }
}
