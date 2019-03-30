using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OnUtils.Utils
{
    /// <summary>
    /// Обеспечивает дополнительные возможности при сериализации объектов в JSON.
    /// </summary>
    public sealed class JsonContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Список свойств и полей, которые будут игнорироваться при сериализации объекта.
        /// </summary>
        public string[] IgnorePropertiesAndFields { get; set; }

        /// <summary>
        /// </summary>
        protected sealed override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);
            return props
                    .Where(p => IgnorePropertiesAndFields == null || !IgnorePropertiesAndFields.Contains(p.PropertyName))
                    .ToList();
        }
    }
}
