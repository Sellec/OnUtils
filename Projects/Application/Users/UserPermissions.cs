using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Application.Users
{
    /// <summary>
    /// Хранит список разрешений пользователя в виде пар {идентификатор модуля:список разрешений}.
    /// </summary>
    public class UserPermissions : IReadOnlyDictionary<Guid, Modules.Permissions>
    {
        private Dictionary<Guid, Modules.Permissions> _source = null;

        /// <summary>
        /// Создает новый экземпляр класса <see cref="UserPermissions"/>. Если <paramref name="source"/> не пуст, то используется в качестве источника значений.
        /// </summary>
        public UserPermissions(Dictionary<Guid, Modules.Permissions> source = null)
        {
            _source = source == null ? new Dictionary<Guid, Modules.Permissions>() : new Dictionary<Guid, Modules.Permissions>(source);
        }

        /// <summary>
        /// Создает новый экземпляр класса <see cref="UserPermissions"/>. Если <paramref name="source"/> не пуст, то используется в качестве источника значений.
        /// </summary>
        public UserPermissions(Dictionary<Guid, List<Guid>> source)
        {
            _source = source == null ? new Dictionary<Guid, Modules.Permissions>() : source.ToDictionary(x=>x.Key, x=>new Modules.Permissions(x.Value));
        }

        /// <summary>
        /// Возвращает список разрешений пользователя, относящихся к указанному модулю.
        /// </summary>
        /// <param name="moduleID">Идентификатор модуля.</param>
        /// <returns>Возвращает список разрешений, если указанный модуль присутствует в списке. Если модуль не найден, генерирует исключение <see cref="KeyNotFoundException"/>.</returns>
        /// <exception cref="KeyNotFoundException">Возникает, если указанный модуль отсутствует в данном списке разрешений.</exception>
        public Modules.Permissions this[Guid moduleID] => _source[moduleID];

        /// <summary>
        /// Возвращает список всех модулей, для которых найдены разрешения.
        /// </summary>
        public IEnumerable<Guid> Keys => _source.Keys;

        /// <summary>
        /// Возвращает список всех наборов разрешений, относящихся к модулям в данном списке разрешений.
        /// </summary>
        public IEnumerable<Modules.Permissions> Values => _source.Values;

        /// <summary>
        /// Возвращает количество модулей с разрешениями в данном списке разрешений.
        /// </summary>
        public int Count => _source.Count;

        /// <summary>
        /// Возвращает true, если в данном списке разрешений присутствуют разрешения модуля с идентификатором <paramref name="moduleID"/>. Возвращает false, если такой модуль отсутствует в списке.
        /// </summary>
        public bool ContainsKey(Guid moduleID)
        {
            return _source.ContainsKey(moduleID);
        }

        /// <summary>
        /// Возвращает список разрешений, относящихся к указанному модулю.
        /// </summary>
        /// <param name="moduleID">Идентификатор модуля.</param>
        /// <param name="value">После возвращения из метода содержит список разрешений, относящихся к указанному модулю, если указанный модуль присутствует в списке. В противном случае содержит null.</param>
        /// <returns>Возвращает true, если в данном списке разрешений присутствуют разрешения модуля с идентификатором <paramref name="moduleID"/>. Возвращает false, если такой модуль отсутствует в списке.</returns>
        public bool TryGetValue(Guid moduleID, out Modules.Permissions value)
        {
            return _source.TryGetValue(moduleID, out value);
        }

        /// <summary>
        /// Возвращает перечислитель, позволяющий перебирать пары {модуль:разрешения модуля} в данном списке разрешений.
        /// </summary>
        public IEnumerator<KeyValuePair<Guid, Modules.Permissions>> GetEnumerator()
        {
            return _source.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _source.GetEnumerator();
        }
    }
}
