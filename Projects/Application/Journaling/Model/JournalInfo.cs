namespace OnUtils.Application.Journaling.Model
{
    /// <summary>
    /// Представляет заголовок журнала и основную информацию о журнале.
    /// </summary>
    public class JournalInfo
    {
        internal static void Fill(JournalInfo dest, DB.JournalNameDAO source)
        {
            dest.IdJournal = source.IdJournal;
            dest.Name = source.Name;
            dest.UniqueKey = source.UniqueKey;
        }

        /// <summary>
        /// Идентификатор журнала.
        /// </summary>
        public int IdJournal { get; private set; }

        /// <summary>
        /// Название журнала.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Уникальный ключ журнала
        /// </summary>
        public string UniqueKey { get; private set; }
    }
}
