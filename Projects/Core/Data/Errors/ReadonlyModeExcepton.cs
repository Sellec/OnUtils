using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OnUtils.Data.Errors
{
    /// <summary>
    /// Исключение, которое выдается при попытке изменить или добавить данные в репозиторий или контейнер UnitOfWork, связанные с контекстом, находящимся в режиме чтения.
    /// </summary>
    public class ReadonlyModeExcepton : InvalidOperationException
    {
        /// <summary>
        /// </summary>
        public ReadonlyModeExcepton() : base("Контекст находится в режиме чтения данных. Добавление новых сущностей, изменение существующих, отслеживание и применение изменений невозможно.")
        {

        }
    }
}
