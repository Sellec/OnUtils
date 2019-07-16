using System;

namespace OnUtils.Application.Configuration
{
    using Journaling;

    /// <summary>
    /// Предоставляет информацию о сохраняемых настройках модуля в событие <see cref="Modules.ModuleCore{TAppCoreSelfReference, TSelfReference}.OnConfigurationApply(ConfigurationApplyEventArgs{TAppCoreSelfReference, TSelfReference})"/>.
    /// </summary>
    public class ConfigurationApplyEventArgs<TAppCoreSelfReference, TModule> : EventArgs
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        where TModule : Modules.ModuleCore<TAppCoreSelfReference, TModule>
    {
        internal ConfigurationApplyEventArgs(ModuleConfiguration<TAppCoreSelfReference, TModule> configuration)
        {
            Configuration = configuration;
            IsSuccess = true;
        }

        /// <summary>
        /// Указывает, что сохранение настроек должно быть прервано. 
        /// </summary>
        /// <param name="idJournalData">Идентификатор записи в журнале с информацией об ошибке.</param>
        /// <seealso cref="JournalingManager{TAppCoreSelfReference}.GetJournalData(int)"/>
        public void SetFailed(int idJournalData)
        {
            IsSuccess = false;
            IdJournalData = idJournalData;
        }

        /// <summary>
        /// Новые настройки
        /// </summary>
        public ModuleConfiguration<TAppCoreSelfReference, TModule> Configuration { get; }

        internal bool IsSuccess { get; set; }

        internal int IdJournalData { get; set; }
    }
}
