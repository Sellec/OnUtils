using System.Reflection;

namespace OnUtils.Architecture.AppCore.Internal
{
    class AppCoreStartupInfo
    {
        public object ObjectInstance;
        public MethodInfo ConfigureBindings;
        public MethodInfo ExecuteStart;
    }
}
