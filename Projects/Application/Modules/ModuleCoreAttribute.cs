using System;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Модуль обязательно должен быть помечен данным атрибутом, в противном случае возникает ошибка во время привязки типов.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ModuleCoreAttribute : Attribute
    {
        /// <summary>
        /// Создает новый экземпляр атрибута. 
        /// </summary>
        /// <param name="caption">Отображаемое имя модуля.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="caption"/> является пустой строкой или null.</exception>
        public ModuleCoreAttribute(string caption)
        {
            if (string.IsNullOrEmpty(caption)) throw new ArgumentNullException(nameof(caption));

            Caption = caption;
        }

        /// <summary>
        /// Отображаемое имя модуля.
        /// </summary>
        public string Caption
        {
            get;
        }

        /// <summary>
        /// Url-доступное имя (см. <see cref="ModuleCore{TAppCoreSelfReference}.UrlName"/>) по-умолчанию, присваиваемое модулю, если в настройках не задано значение.
        /// </summary>
        /// <seealso cref="ModuleCore{TAppCoreSelfReference}.UrlName"/>
        /// <seealso cref="Configuration.ModuleConfiguration{TAppCoreSelfReference, TModule}.UrlName"/>
        public string DefaultUrlName
        {
            get;
            set;
        }
    }
}
