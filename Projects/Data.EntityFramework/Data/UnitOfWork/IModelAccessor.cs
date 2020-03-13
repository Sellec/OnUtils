namespace OnUtils.Data.UnitOfWork
{
    /// <summary>
    /// Представляет интерфейс для доступа к процессу построения модели.
    /// </summary>
    public interface IModelAccessor
    {
        /// <summary>
        /// Позволяет получить или задать строку подключения, с которой будет работать экземпляр <see cref="UnitOfWorkBase"/>.
        /// </summary>
        string ConnectionString { get; set; }
    }
}
