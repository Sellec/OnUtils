using OnUtils.Data;

namespace OnUtils.Application.Messaging.DB
{
    class DataContext : Application.DB.CoreContextBase
    {
        public IRepository<MessageQueue> MessageQueue { get; set; }
        public IRepository<MessageQueueHistory> MessageQueueHistory { get; set; }
    }
}
