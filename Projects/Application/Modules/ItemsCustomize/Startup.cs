namespace OnUtils.Application.Modules.ItemsCustomize
{
    using Architecture.AppCore;
    using Architecture.AppCore.DI;

    class Startup<TAppCoreSelfReference> : IConfigureBindings<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        void IConfigureBindings<TAppCoreSelfReference>.ConfigureBindings(IBindingsCollection<TAppCoreSelfReference> bindingsCollection)
        {
            bindingsCollection.SetSingleton<ModuleItemsCustomize<TAppCoreSelfReference>>();
        }
    }
}
