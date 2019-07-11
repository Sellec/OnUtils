namespace OnUtils.Application.Messaging
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
        }
    }
}

