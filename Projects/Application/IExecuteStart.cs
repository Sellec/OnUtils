namespace OnUtils.Application
{
    using Architecture.AppCore;

    /// <summary>
    /// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, 
    /// реализующих данный интерфейс, после чего для каждого экземпляра вызывается метод <see cref="IExecuteStart{TAppCore}.ExecuteStart(TAppCore)"/>.
    /// </summary>
    public interface IExecuteStart : IExecuteStart<ApplicationCore>
    {
    }
}
