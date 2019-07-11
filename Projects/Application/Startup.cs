namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Modules.CoreModule.CoreModule>();
            bindingsCollection.SetSingleton<Modules.UsersManagement.ModuleUsersManagement>();
        }
    }
}
