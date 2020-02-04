namespace OnUtils.Architecture.AppCore
{
    using DI;

    /// <summary>
    /// При загрузке сборок в текущий домен приложения после запуска ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="ConfigureBindingsLazy(DI.IBindingsCollection{TAppCore})"/>.
    /// </summary>
    public interface IConfigureBindingsLazy<out TAppCore>
    {
        /// <summary>
        /// Вызывается при загрузке сборки для настройки привязок. Если привязки для указываемых query-типов уже существуют, то новые привязки будут проигнорированы.
        /// </summary>
        void ConfigureBindingsLazy(DI.IBindingsCollection<TAppCore> bindingsCollection);
    }
}
