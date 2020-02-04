namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// При загрузке сборок в текущий домен приложения после запуска ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="ExecuteStartLazy(TAppCore)"/>.
    /// </summary>
    public interface IExecuteStartLazy<in TAppCore>
    {
        /// <summary>
        /// Вызывается при загрузке сборки после выполнения привязок и автозапуска компонентов.
        /// </summary>
        void ExecuteStartLazy(TAppCore core);
    }
}
