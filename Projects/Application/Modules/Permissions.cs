using System;
using System.Collections.Generic;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Хранит список ключей разрешений (см. <see cref="Permission.Key"/>
    /// </summary>
    public class Permissions : List<Guid>
    {
        /// <summary>
        /// Создает новый экземпляр класса <see cref="Permissions"/>. Если <paramref name="source"/> не пуст, то используется в качестве источника значений.
        /// </summary>
        public Permissions(IEnumerable<Guid> source) : base(source)
        {
        }
    }
}
