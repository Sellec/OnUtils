namespace OnUtils.Application.Modules.Extensions
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup : IConfigureBindings
    {
        void IConfigureBindings<ApplicationCore>.ConfigureBindings(IBindingsCollection<ApplicationCore> bindingsCollection)
        {
            bindingsCollection.SetTransient<CustomFields.ExtensionCustomsFieldsBase>();
        }
    }
}
