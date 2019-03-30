namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="ExecuteStart(TAppCore)"/>.
    /// </summary>
    public interface IExecuteStart<in TAppCore>
    {
        /// <summary>
        /// Вызывается единственный раз при запуске ядра после вызова <see cref="AppCore{TAppCore}.OnBindingsAutoStart"/> и до вызова <see cref="AppCore{TAppCore}.OnStart"/>.
        /// </summary>
        void ExecuteStart(TAppCore core);
    }
}
