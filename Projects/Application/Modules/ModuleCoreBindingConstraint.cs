using System.Reflection;

namespace OnUtils.Application.Modules
{
    using Architecture.AppCore.DI;

    class ModuleCoreBindingConstraint<TAppCoreSelfReference> : IBindingConstraintHandler
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        void IBindingConstraintHandler.CheckBinding(object sender, BindingConstraintEventArgs e)
        {
            if (typeof(ModuleCore<TAppCoreSelfReference>).IsAssignableFrom(e.QueryType) && !typeof(ModuleCore<TAppCoreSelfReference>).IsAssignableFrom(e.QueryType))
            {
                e.SetFailed($"Ядро приложения поддерживает только модули, наследующиеся от '{typeof(ModuleCore<TAppCoreSelfReference>).FullName}'.");
                return;
            }

            if (typeof(ModuleCore<TAppCoreSelfReference>).IsAssignableFrom(e.QueryType))
            {
                var moduleCoreAttribute = e.QueryType.GetCustomAttribute<ModuleCoreAttribute>();
                if (moduleCoreAttribute == null)
                {
                    e.SetFailed($"Тип, наследующий от '{typeof(ModuleCore<TAppCoreSelfReference>).FullName}', считается модулем и должен обладать атрибутом '{typeof(ModuleCoreAttribute).FullName}'.");
                    return;
                }
            }
        }
    }
}
