using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnUtils.Application.Items
{
    using Modules;
    using OnUtils.Items;

    /// <summary>
    /// Базовый класс для сущностей.
    /// Предоставляет некоторый набор методов и виртуальных свойств, используемых во многих расширениях и частях движка.
    /// Некоторые же части движка работают ТОЛЬКО с объектами, унаследованными от <see cref="ItemBase"/> (например, расширение CustomFields).
    /// Поддерживает атрибут <see cref="ConstructorInitializerAttribute"/> для методов класса. 
    /// </summary>
    [Serializable]
    public abstract partial class ItemBase<TAppCoreSelfReference>
        where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
    {
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        private Lazy<ModuleCore<TAppCoreSelfReference>> _owner = null;

        /// <summary>
        /// Создает новый экземпляр сущности.
        /// </summary>
        public ItemBase()
        {
            _owner = new Lazy<ModuleCore<TAppCoreSelfReference>>(() => ApplicationCoreHolder.Get<TAppCoreSelfReference>()?.Get<ItemsManager<TAppCoreSelfReference>>()?.GetModuleForItemType(GetType()));
            MethodMarkCallerAttribute.CallMethodsInObject<ConstructorInitializerAttribute>(this);
        }

        #region Свойства
        /// <summary>
        /// Возвращает идентификатор объекта.
        /// Должен быть переопределен в класса-потомке и, для сущностей из БД, привязан к целочисленному свойству-идентификатору.
        /// </summary>
        [NotMapped]
        public abstract int ID
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает название (заголовок) объекта.
        /// Должен быть переопределен в класса-потомке. Например, для сущностей из БД, может возвращать заголовок статьи, логин или никнейм пользователя и т.п.
        /// </summary>
        [NotMapped]
        public abstract string Caption
        {
            get;
            set;
        }

        /// <summary>
        /// Возвращает и задает дату последнего изменения объекта, если поддерживается классом-потомком.
        /// По-умолчанию (без переопределения в классе-потомке) возвращает null.
        /// </summary>
        [NotMapped]
        public virtual DateTime DateChangeBase
        {
            get; set;
        }

        /// <summary>
        /// Модуль, к которому относится объект. Может быть пустым.
        /// Привязка к модулю важна для работы некоторых методов и некоторого функционала движка.
        /// </summary>
        /// <seealso cref="ItemsManager{TAppCoreSelfReference}.RegisterModuleItemType{TItemBase, TModule}"/>
        [NotMapped]
        [Newtonsoft.Json.JsonIgnore]
        public virtual ModuleCore<TAppCoreSelfReference> OwnerModule
        {
            get => _owner.Value;
        }

        #endregion

        /// <summary>
        /// Возвращает <see cref="Caption"/> при приведении к строке.
        /// </summary>
        public override string ToString()
        {
            return Caption;
        }
    }
}