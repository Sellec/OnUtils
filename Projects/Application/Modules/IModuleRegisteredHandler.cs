namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;

    //todo описание
    public interface IModuleRegisteredHandler : IComponentSingleton<ApplicationCore>
    {
        //todo описание
        void OnModuleInitialized<TModule>(TModule module) where TModule : ModuleCore<TModule>;
    }
}
