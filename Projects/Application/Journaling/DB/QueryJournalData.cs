#pragma warning disable CS1591
namespace OnUtils.Application.Journaling.DB
{
    public class QueryJournalData
    {
        public JournalDAO JournalData { get; set; }

        public JournalNameDAO JournalName { get; set; }

        public Application.DB.UserBase User { get; set; }
    }
}
