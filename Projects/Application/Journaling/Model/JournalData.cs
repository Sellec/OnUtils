namespace OnUtils.Application.Journaling.Model
{
    using Application.DB;
    using Items;
    using System;

    /// <summary>
    /// Представляет запись в журнале.
    /// </summary>
    public class JournalData
    {
        internal static void Fill(JournalData dest, DB.JournalDAO source, JournalInfo journal, Users.UserInfo user)
        {
            dest.IdJournalData = source.IdJournalData;
            dest.JournalInfo = journal;
            dest.EventType = source.EventType;
            dest.EventInfo = source.EventInfo;
            dest.EventInfoDetailed = source.EventInfoDetailed;
            dest.ExceptionDetailed = source.ExceptionDetailed;
            dest.DateEvent = source.DateEvent;
            dest.User = user;
            dest.IdRelatedItem = source.IdRelatedItem;
            dest.IdRelatedItemType = source.IdRelatedItemType;
        }

        /// <summary>
        /// Идентификатор записи журнала.
        /// </summary>
        public int IdJournalData { get; private set; }

        /// <summary>
        /// Информация о журнале, к которому относятся записи.
        /// </summary>
        public JournalInfo JournalInfo { get; private set; }

        /// <summary>
        /// Тип события.
        /// </summary>
        public EventType EventType { get; private set; }

        /// <summary>
        /// Основная информация о событии.
        /// </summary>
        public string EventInfo { get; private set; }

        /// <summary>
        /// Детализированная информация о событии.
        /// </summary>
        public string EventInfoDetailed { get; private set; }

        /// <summary>
        /// Информация об исключении, если событие сопровождалось возникновением исключения.
        /// </summary>
        public string ExceptionDetailed { get; private set; }

        /// <summary>
        /// Дата фиксации события.
        /// </summary>
        public DateTime DateEvent { get; private set; }

        /// <summary>
        /// Информация о пользователе, создавшем запись.
        /// </summary>
        public Users.UserInfo User { get; private set; }

        /// <summary>
        /// Идентификатор объекта, с которым связано событие. Связанный объект возможно получить, когда задано значение <see cref="IdRelatedItem"/> и <see cref="IdRelatedItemType"/>.
        /// </summary>
        /// <seealso cref="ItemBase{TAppCoreSelfReference}.ID"/>.
        public int? IdRelatedItem { get; private set; }

        /// <summary>
        /// Идентификатор типа объекта, с которым связано событие. Связанный объект возможно получить, когда задано значение <see cref="IdRelatedItem"/> и <see cref="IdRelatedItemType"/>.
        /// </summary>
        /// <see cref="ItemType.IdItemType"/>
        public int? IdRelatedItemType { get; private set; }

    }
}
