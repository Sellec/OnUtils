namespace OnUtils.Architecture.AppCore.DI
{
    /// <summary>
    /// Представляет обработчик привязок, вызываемый при добавлении новой привязки в коллекцию <see cref="BindingsCollection{TAppCore}"/>.
    /// Позволяет запретить создание определенной привязки, если она не соответствует правилам.
    /// </summary>
    public interface IBindingConstraintHandler
    {
        /// <summary>
        /// Вызывается при попытке добавить новую привязку.
        /// </summary>
        /// <param name="sender">Объект коллекции привязки, вызывающий проверку.</param>
        /// <param name="e">Аргументы вызова.</param>
        void CheckBinding(object sender, BindingConstraintEventArgs e);
    }
}
