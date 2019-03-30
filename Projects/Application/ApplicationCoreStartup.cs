namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class ApplicationCoreStartup<TApplication> : IConfigureBindings<TApplication>
        where TApplication : AppCore<TApplication>
    {
        void IConfigureBindings<TApplication>.ConfigureBindings(IBindingsCollection<TApplication> bindingsCollection)
        {
            bindingsCollection.SetSingleton<Modules.ModulesManager<TApplication>>();
            bindingsCollection.SetSingleton<Users.UserContextManager<TApplication>>();
        }
    }
}
