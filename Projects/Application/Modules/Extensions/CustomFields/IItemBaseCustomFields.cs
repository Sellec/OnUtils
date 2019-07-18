using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using OnUtils.Items;

namespace OnUtils.Application.Modules.Extensions.CustomFields
{
    using Data;
    using Field;
    using Proxy;
    using Scheme;

    public interface IItemBaseCustomFields
    {
        /// <summary>
        /// Свойство, содержащее ПОЛНУЮ схему полей для текущего объекта. Для получения конкретной схемы полей следует обращаться к методу <see cref="DefaultSchemeWData.GetScheme(uint)"/> (см. описание метода).
        /// См. также <see cref="FieldsDynamic"/> для обращения к полям через <see cref="IField.alias"/>.
        /// При изменении свойства, помеченного атрибутом <see cref="SchemeItemAttribute"/>, данные полей автоматически обновляются из базы с учетом нового значения свойства. 
        /// Таким образом, во время обработки данных, пришедших из формы, не рекомендуется менять <see cref="SchemeItemAttribute"/> идентификатор, чтобы не потерять данные из формы.
        /// Значение свойства не может быть равно null. При получении значения свойства будет возвращен объект, либо сгенерировано исключение <see cref="InvalidOperationException"/> (см. описание секции исключений). 
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Возникает, если при получении значений полей для объекта произошла ошибка. 
        /// В свойстве <see cref="Exception.InnerException"/> содержится информация о возникшем исключении.
        /// Если во время получения значений полей возникло исключение (т.е. информация не была получена), то при повторном обращении к свойству произойдет повторная попытка получить значения полей.
        /// Таким образом, свойство никогда не вернет null, только объект с данными, либо сгенерирует исключение.
        /// </exception>
        [NotMapped]
        [Newtonsoft.Json.JsonConverter(typeof(CustomPropertyConverter))]
        DefaultSchemeWData Fields { get; }

        /// <summary>
        /// Динамическое свойство для обращения к полям объекта не через <see cref="DefaultSchemeWData.this[int]"/> или <see cref="fieldsNamed.this[String]"/>, а напрямую по имени FieldsDynamic.field_xx или FieldsDynamic.{alias}.
        /// Свойство <see cref="Fields"/> представляет собой прокси-объект, сгенерированный на основе класса <see cref="DefaultSchemeWData"/> с динамически генерируемыми дополнительными свойствами, основанными на полях, содержащихся в полной схеме.
        /// Например, если в полной схеме содержится поле с Id=155 и Alias="TestField", то прокси-объект будет иметь динамические свойства field_155 и TestField. Эти свойства напрямую ссылаются на <see cref="FieldData.Value"/>, т.о. можно напрямую получить значение поля №155, просто обратившись к FieldsDynamic.TestField.
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonConverter(typeof(CustomPropertyConverter))]
        dynamic FieldsDynamic { get; }

        //[NotMapped]
        //Пришлось убрать, иначе возникает конфликт в метаданных - собирается коллекция 
        //_propertyMetadata = ModelMetadata.PropertiesAsArray.ToDictionaryFast(m => m.PropertyName, StringComparer.OrdinalIgnoreCase);
        //и fields конфликтует с Fields.
        /// <summary>
        /// См. <see cref="Fields"/>.
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonConverter(typeof(CustomPropertyConverter))]
        DefaultSchemeWData fields { get; }

        /// <summary>
        /// Возвращает коллекцию именованных полей со значениями для объекта. Не может быть равен null (см. <see cref="Fields"/>).
        /// </summary>
        [NotMapped]
        ReadOnlyDictionary<string, FieldData> fieldsNamed { get; }
    }
}
