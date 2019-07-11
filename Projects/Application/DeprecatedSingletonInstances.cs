using System;

namespace OnUtils.Application
{
    /// <summary>
    /// Будет удалено в будущих версиях.
    /// </summary>
    public static class DeprecatedSingletonInstances
    {
        [Obsolete("Будет удалено в будущих версиях.")]
        public static Modules.ModulesManager ModulesManager { get; set; }
    }
}
