namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup<TAppCoreSelfReference> : IConfigureBindings<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        void IConfigureBindings<TAppCoreSelfReference>.ConfigureBindings(IBindingsCollection<TAppCoreSelfReference> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Modules.CoreModule.CoreModule<TAppCoreSelfReference>>();
            bindingsCollection.SetSingleton<Modules.UsersManagement.ModuleUsersManagement<TAppCoreSelfReference>>();
        }
    }
}
