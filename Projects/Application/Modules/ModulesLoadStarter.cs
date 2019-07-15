namespace OnUtils.Application.Modules
{
    using Architecture.AppCore;

    class ModulesLoadStarter<TAppCoreSelfReference> : CoreComponentBase<TAppCoreSelfReference>, IComponentSingleton<TAppCoreSelfReference>, IAutoStart
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        public ModulesLoadStarter()
        {
        }

        protected override void OnStart()
        {
            AppCore.GetModulesManager().StartModules();
        }

        protected override void OnStop()
        {
        }
    }
}
