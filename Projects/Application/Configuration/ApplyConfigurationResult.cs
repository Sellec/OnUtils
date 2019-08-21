namespace OnUtils.Application.Configuration
{
    /// <summary>
    /// Варианты результата выполнения функции <see cref="ModuleConfigurationManipulator{TAppCoreSelfReference, TModule}.ApplyConfiguration{TConfiguration}(TConfiguration)"/>.
    /// </summary>
    public enum ApplyConfigurationResult
    {
        /// <summary>
        /// Отсутствует доступ к сохранению конфигурации модуля. См. <see cref="Modules.ModulesConstants.PermissionSaveConfiguration"/>.
        /// </summary>
        PermissionDenied,

        /// <summary>
        /// Сохранение прошло успешно.
        /// </summary>
        Success,

        /// <summary>
        /// Ошибка сохранения. 
        /// </summary>
        Failed,
    }
}
