using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data
{
    /// <summary>
    /// Экземпляры данного класса предоставляют доступ к сведениям о сущностях, отслеживаемых контекстом данных <see cref="IDataContext"/>, и возможность управления этими сущностями. 
    /// </summary>
    public interface IRepositoryEntry
    {
        /// <summary>
        /// Возвращает сущность.
        /// </summary>
        object Entity { get; }

        /// <summary>
        /// Возвращает или задает состояние сущности.
        /// </summary>
        ItemState State { get; }
    }
}
