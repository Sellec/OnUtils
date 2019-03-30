using System;
using System.Collections.Generic;
using System.Text;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Хранит список ключей разрешений (см. <see cref="Permission.Key"/>
    /// </summary>
    public class PermissionsList : List<Guid>
    {
        /// <summary>
        /// Создает новый экземпляр класса <see cref="PermissionsList"/>. Если <paramref name="source"/> не пуст, то используется в качестве источника значений.
        /// </summary>
        public PermissionsList(IEnumerable<Guid> source) : base(source)
        {
        }
    }
}
