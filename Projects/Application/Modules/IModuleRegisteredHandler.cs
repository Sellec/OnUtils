namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;

    //todo описание
    public interface IModuleRegisteredHandler<TAppCoreSelfReference> : IComponentSingleton<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        //todo описание
        void OnModuleInitialized<TModule>(TModule module) where TModule : ModuleCore<TAppCoreSelfReference, TModule>;
    }
}
