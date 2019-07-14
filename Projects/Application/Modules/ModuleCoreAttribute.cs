﻿using System;

namespace OnUtils.Application.Modules
{
    /// <summary>
    /// Интерфейс модуля обязательно должен быть помечен данным атрибутом, в противном случае при инициализации модуля возникает исключение <see cref="Exceptions.ModuleInitException"/>.
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
        /// Url-доступное имя (см. <see cref="ModuleCore.UrlName"/>) по-умолчанию, присваиваемое модулю, если в настройках (см. <see cref="Configuration.ModuleConfiguration{TModule}.UrlName"/>) не задано значение.
        /// </summary>
        /// <seealso cref="ModuleCore.UrlName"/>
        /// <seealso cref="Configuration.ModuleConfiguration{TModule}.UrlName"/>
        public string DefaultUrlName
        {
            get;
            set;
        }
    }
}