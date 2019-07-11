using System.Reflection;

namespace OnUtils.Application.Modules
{
    using Architecture.AppCore.DI;

    class ModuleCoreBindingConstraint : IBindingConstraintHandler
    {
        void IBindingConstraintHandler.CheckBinding(object sender, BindingConstraintEventArgs e)
        {
            if (typeof(ModuleCore).IsAssignableFrom(e.QueryType) && !typeof(ModuleCore).IsAssignableFrom(e.QueryType))
            {
                e.SetFailed($"Ядро приложения поддерживает только модули, наследующиеся от '{typeof(ModuleCore).FullName}'.");
                return;
            }

            if (typeof(ModuleCore).IsAssignableFrom(e.QueryType))
            {
                var moduleCoreAttribute = e.QueryType.GetCustomAttribute<ModuleCoreAttribute>();
                if (moduleCoreAttribute == null)
                {
                    e.SetFailed($"Тип, наследующий от '{typeof(ModuleCore).FullName}', считается модулем и должен обладать атрибутом '{typeof(ModuleCoreAttribute).FullName}'.");
                    return;
                }
            }
        }
    }
}
