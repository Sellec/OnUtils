namespace OnUtils.Application.Modules.Extensions
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup<TAppCoreSelfReference> : IConfigureBindings<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        void IConfigureBindings<TAppCoreSelfReference>.ConfigureBindings(IBindingsCollection<TAppCoreSelfReference> bindingsCollection)
        {
            bindingsCollection.SetTransient<CustomFields.ExtensionCustomsFieldsBase<TAppCoreSelfReference>>();
        }
    }
}
