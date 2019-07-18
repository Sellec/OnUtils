namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    ///// <summary>
    ///// При создании и запуске ядра создаются экземпляры всех неабстрактных классов, имеющих открытый беспараметрический конструктор, реализующих данный интерфейс,
    ///// после чего для каждого экземпляра вызывается метод <see cref="IConfigureBindings{TAppCore}.ConfigureBindings(IBindingsCollection{TAppCore})"/>.
    ///// </summary>
    //public interface IConfigureBindings<TAppCoreSelfReference> : IConfigureBindings<ApplicationCore>
    //{
    //}
}
