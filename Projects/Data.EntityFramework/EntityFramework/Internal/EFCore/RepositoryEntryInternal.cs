using EntityState = Microsoft.EntityFrameworkCore.EntityState;
using EntryType = Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry;

namespace OnUtils.Data.EntityFramework.Internal
{
    using Data;

    class RepositoryEntryInternal : IRepositoryEntry
    {
        private EntryType _entry = null;

        public RepositoryEntryInternal(EntryType entry)
        {
            _entry = entry;
        }

        public object Entity
        {
            get { return _entry.Entity; }
        }

        public ItemState State
        {
            get
            {
                switch (_entry.State)
                {
                    case EntityState.Added:
                        return ItemState.Added;

                    case EntityState.Deleted:
                        return ItemState.Deleted;

                    case EntityState.Detached:
                        return ItemState.Detached;

                    case EntityState.Modified:
                        return ItemState.Modified;

                    case EntityState.Unchanged:
                        return ItemState.Unchanged;

                    default:
                        return ItemState.Detached;
                }
            }
        }
    }
}
