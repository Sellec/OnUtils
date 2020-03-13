using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data
{
    /// <summary>
    /// Описывает состояние объекта в контейнере <see cref="UnitOfWorkBase"/> 
    /// </summary>
    [Flags]
    public enum ItemState
    {
        /// <summary>
        /// Объект не относится к контейнеру, не содержится ни в одном из репозиториев.
        /// </summary>
        Detached = 1,

        /// <summary>
        /// Объект прикреплен к контейнеру и для него нет непримененных изменений внутри контейнера.
        /// </summary>
        Unchanged = 2,

        /// <summary>
        /// Объект был добавлен в контейнер, но для него не были применены изменения.
        /// </summary>
        Added = 4,

        /// <summary>
        /// Объект прикреплен к контейнеру и помечен на удаление.
        /// </summary>
        Deleted = 8,

        /// <summary>
        /// Объект прикреплен к контейнеру, для него есть непримененные изменения внутри контейнера.
        /// </summary>
        Modified = 16
    }
}
