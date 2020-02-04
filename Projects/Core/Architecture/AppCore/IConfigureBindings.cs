namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="ConfigureBindings(DI.IBindingsCollection{TAppCore})"/>.
    /// </summary>
    public interface IConfigureBindings<out TAppCore>
    {
        /// <summary>
        /// Вызывается единственный раз при создании ядра перед его запуском для настройки привязки.
        /// </summary>
        void ConfigureBindings(DI.IBindingsCollection<TAppCore> bindingsCollection);
    }
}
