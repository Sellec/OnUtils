using OnUtils.Data;

#pragma warning disable CS1591
namespace OnUtils.Application.Journaling.DB
{
    public class DataContext : Application.DB.CoreContext
    {
        public IRepository<JournalDAO> Journal { get; }

        public IRepository<JournalNameDAO> JournalName { get; }
    }
}
