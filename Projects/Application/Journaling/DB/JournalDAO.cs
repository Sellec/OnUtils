#pragma warning disable CS1591
namespace OnUtils.Application.Journaling.DB
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Journal")]
    public class JournalDAO
    {
        [Key]
        public int IdJournalData { get; set; }

        public int IdJournal { get; set; }

        public EventType EventType { get; set; }

        [Required, MaxLength(300)]
        public string EventInfo { get; set; }

        public string EventInfoDetailed { get; set; }

        public string ExceptionDetailed { get; set; }

        public DateTime DateEvent { get; set; }

        public int? IdUser { get; set; }

        public int? IdRelatedItem { get; set; }

        public int? IdRelatedItemType { get; set; }
    }
}
