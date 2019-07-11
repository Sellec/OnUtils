namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            bindingsCollection.RegisterBindingConstraintHandler(new ModuleCoreBindingConstraint());
        }
    }
}
