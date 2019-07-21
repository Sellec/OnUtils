using OnUtils.Items;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;

namespace OnUtils.Application.Items
{
    using Modules.ItemsCustomize;
    using Modules.ItemsCustomize.Data;
    using Modules.ItemsCustomize.Field;
    using Modules.ItemsCustomize.Proxy;
    using Modules.ItemsCustomize.Scheme;

    [System.Diagnostics.DebuggerDisplay("ItemBase: id={ID}")]
    public abstract partial class ItemBase<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        [Newtonsoft.Json.JsonIgnore]
        internal DefaultSchemeWData _fields = null;

        [Newtonsoft.Json.JsonIgnore]
        private ReadOnlyDictionary<string, FieldData> _fieldsNamed = null;

        [ConstructorInitializer]
        private void FieldsInitializer()
        {
            if (!(this is IItemCustomized)) return;

            var moduleLink = ModuleItemsCustomize<TAppCoreSelfReference>._moduleLink;
            if (_fields == null && moduleLink != null)
            {
                moduleLink.RegisterToQuery(this);
            }

        }

        [SavedInContextEvent]
        private void FieldsSaver()
        {
            if (!(this is IItemCustomized)) return;

            var moduleLink = ModuleItemsCustomize<TAppCoreSelfReference>._moduleLink;
            if (_fields == null && moduleLink != null)
            { 
                moduleLink.SaveItemFields(this);
            }
        }

        private void UpdateFieldsValue()
        {
            if (_fields != null)
            {
                var schemeItem = SchemeItemAttribute.GetValueFromObject(this);
                if (_fields._schemeItemSource != null && schemeItem != null && (schemeItem.IdItemType != _fields._schemeItemSource.IdItemType || schemeItem.IdItem != _fields._schemeItemSource.IdItem))
                {
                    _fields = null;
                    _fieldsNamed = null;
                }
            }

            var moduleLink = ModuleItemsCustomize<TAppCoreSelfReference>._moduleLink;
            if (_fields == null && moduleLink != null)
            {
                moduleLink.RegisterToQuery(this);
                moduleLink.CheckDeffered(GetType());

                    if (_fields == null)
                    {
                        _fields = moduleLink.GetItemFields(this);
                        if (_fields == null) _fieldsNamed = null;
                    }
            }

            if (_fields == null)
            {
                _fields = ProxyHelper.CreateTypeObjectFromParent<DefaultSchemeWData, IField>(new DefaultScheme());
                if (_fields == null) _fieldsNamed = null;
            }

            if (_fields != null && _fieldsNamed == null)
            {
                _fieldsNamed = new ReadOnlyDictionary<string, FieldData>(_fields.Where(x => !string.IsNullOrEmpty(x.Value.alias)).ToDictionary(x => x.Value.alias, x => x.Value));
            }
        }

        /// <summary>
        /// Свойство, содержащее ПОЛНУЮ схему полей для текущего объекта. Для получения конкретной схемы полей следует обращаться к методу <see cref="DefaultSchemeWData.GetScheme(uint)"/> (см. описание метода).
        /// См. также <see cref="FieldsDynamicBase"/> для обращения к полям через <see cref="IField.alias"/>.
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
        internal protected DefaultSchemeWData FieldsBase
        {
            get
            {
                if (!(this is IItemCustomized)) throw new InvalidOperationException($"Для доступа к расширению настраиваемых полей необходимо наследовать интерфейс '{typeof(IItemCustomized).FullName}'.");

                UpdateFieldsValue();
                return _fields;
            }
        }

        /// <summary>
        /// Динамическое свойство для обращения к полям объекта не через <see cref="DefaultSchemeWData.this[int]"/> или <see cref="fieldsNamedBase.this[String]"/>, а напрямую по имени FieldsDynamic.field_xx или FieldsDynamic.{alias}.
        /// Свойство <see cref="FieldsBase"/> представляет собой прокси-объект, сгенерированный на основе класса <see cref="DefaultSchemeWData"/> с динамически генерируемыми дополнительными свойствами, основанными на полях, содержащихся в полной схеме.
        /// Например, если в полной схеме содержится поле с Id=155 и Alias="TestField", то прокси-объект будет иметь динамические свойства field_155 и TestField. Эти свойства напрямую ссылаются на <see cref="FieldData.Value"/>, т.о. можно напрямую получить значение поля №155, просто обратившись к FieldsDynamic.TestField.
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonConverter(typeof(CustomPropertyConverter))]
        protected dynamic FieldsDynamicBase
        {
            get
            {
                if (!(this is IItemCustomized)) throw new InvalidOperationException($"Для доступа к расширению настраиваемых полей необходимо наследовать интерфейс '{typeof(IItemCustomized).FullName}'.");

                UpdateFieldsValue();
                return _fields;
            }
        }

        //[NotMapped]
        //Пришлось убрать, иначе возникает конфликт в метаданных - собирается коллекция 
        //_propertyMetadata = ModelMetadata.PropertiesAsArray.ToDictionaryFast(m => m.PropertyName, StringComparer.OrdinalIgnoreCase);
        //и fields конфликтует с Fields.
        /// <summary>
        /// См. <see cref="FieldsBase"/>.
        /// </summary>
        [NotMapped]
        [Newtonsoft.Json.JsonConverter(typeof(CustomPropertyConverter))]
        protected DefaultSchemeWData fieldsBase
        {
            get
            {
                if (!(this is IItemCustomized)) throw new InvalidOperationException($"Для доступа к расширению настраиваемых полей необходимо наследовать интерфейс '{typeof(IItemCustomized).FullName}'.");

                UpdateFieldsValue();
                return _fields;
            }
        }

        /// <summary>
        /// Возвращает коллекцию именованных полей со значениями для объекта. Не может быть равен null (см. <see cref="FieldsBase"/>).
        /// </summary>
        [NotMapped]
        protected ReadOnlyDictionary<string, FieldData> fieldsNamedBase
        {
            get
            {
                if (!(this is IItemCustomized)) throw new InvalidOperationException($"Для доступа к расширению настраиваемых полей необходимо наследовать интерфейс '{typeof(IItemCustomized).FullName}'.");

                UpdateFieldsValue();
                return _fieldsNamed;
            }
        }


    }
}
