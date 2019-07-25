namespace OnUtils.Application.Modules.ItemsCustomize.DB
{
    using OnUtils.Data;

#pragma warning disable CS1591 // todo внести комментарии.
    public class Context : UnitOfWorkBase
    {
        public IRepository<CustomFieldsData> CustomFieldsDatas { get; }

        public IRepository<CustomFieldsField> CustomFieldsFields { get; }

        public IRepository<CustomFieldsScheme> CustomFieldsSchemes { get; }

        public IRepository<CustomFieldsSchemeData> CustomFieldsSchemeDatas { get; }

        public IRepository<CustomFieldsValue> CustomFieldsValues { get; }

        public IRepository<CustomFieldsValueType> CustomFieldsValueTypes { get; }

        public IRepository<Application.DB.ItemParent> ItemParent { get; }

    }
}
