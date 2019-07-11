using System;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Application.Modules
{
    using Application.Users;
    using Architecture.AppCore;
    using Configuration;

    /// <summary>
    /// Базовый класс для всех модулей. Обязателен при реализации любых модулей, т.к. при задании привязок в DI проверяется наследование именно от этого класса.
    /// </summary>
    public abstract class ModuleCore : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>
    {
        #region Константы
        public const int CategoryType = 1;
        public const int ItemType = 2;

        /// <summary>
        /// Обозначает ключ разрешения для сохранения настроек модуля.
        /// </summary>
        public static readonly Guid PermissionSaveConfiguration = "perm_configSave".GenerateGuid();

        #endregion

        private Dictionary<Guid, Permission> _permissions = new Dictionary<Guid, Permission>();
        private Guid _moduleBaseID = Guid.Empty;
        internal int _moduleId;
        internal string _moduleCaption;
        internal string _moduleUrlName;

        private List<Extensions.ModuleExtension> _extensions = new List<Extensions.ModuleExtension>();

        /// <summary>
        /// Создает новый экземпляр класса.
        /// </summary>
        [Obsolete("Следует использовать ModuleCore<TSelfReference>")]
        internal ModuleCore()
        {
        }

        #region Инициализация и остановка
        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
            OnModuleStart();
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
            OnModuleStop();
        }
        #endregion

        /// <summary>
        /// Вызывается при запуске модуля.
        /// </summary>
        internal protected virtual void OnModuleStart()
        {

        }

        /// <summary>
        /// Вызывается при остановке модуля.
        /// </summary>
        internal protected virtual void OnModuleStop()
        {

        }

        internal virtual void InitModule()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Определение дополнительных ресурсов модуля. Вызывается во время инициализации.
        /// </summary>
        protected virtual void InitModuleCustom()
        {

        }
        #endregion

        #region Разрешения
        /// <summary>
        /// Возвращает список разрешений модуля.
        /// </summary>
        public IEnumerable<Permission> GetPermissions()
        {
            return _permissions.Values;
        }

        /// <summary>
        /// Регистрация разрешения для системы доступа. Если разрешение с таким ключом <paramref name="key"/> уже существует, оно будет перезаписано новым.
        /// </summary>
        /// <param name="key">См. <see cref="Permission.Key"/>.</param>
        /// <param name="caption">См. <see cref="Permission.Caption"/>.</param>
        /// <param name="description">См. <see cref="Permission.Description"/>.</param>
        /// <param name="ignoreSuperuser">См. <see cref="Permission.IgnoreSuperuser"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="caption"/> является пустой строкой или null.</exception>
        public void RegisterPermission(string key, string caption, string description = null, bool ignoreSuperuser = false)
        {
            RegisterPermission(string.IsNullOrEmpty(key) ? Guid.Empty : key.GenerateGuid(), caption, description, ignoreSuperuser);
        }

        /// <summary>
        /// Регистрация разрешения для системы доступа. Если разрешение с таким ключом <paramref name="key"/> уже существует, оно будет перезаписано новым.
        /// </summary>
        /// <param name="key">См. <see cref="Permission.Key"/>.</param>
        /// <param name="caption">См. <see cref="Permission.Caption"/>.</param>
        /// <param name="description">См. <see cref="Permission.Description"/>.</param>
        /// <param name="ignoreSuperuser">См. <see cref="Permission.IgnoreSuperuser"/>.</param>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="caption"/> является пустой строкой или null.</exception>
        public void RegisterPermission(Guid key, string caption, string description = null, bool ignoreSuperuser = false)
        {
            if (string.IsNullOrEmpty(caption)) throw new ArgumentNullException(nameof(caption));
            _permissions[key] = new Permission() { Caption = caption, Description = description, IgnoreSuperuser = ignoreSuperuser, Key = key };
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с текущим контекстом (см. <see cref="UserContextManager{TApplication}.GetCurrentUserContext"/>).
        /// </summary>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        public CheckPermissionResult CheckPermission(string key)
        {
            return CheckPermission(AppCore.GetUserContextManager().GetCurrentUserContext(), key.GenerateGuid());
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с контекстом <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Контекст пользователя.</param>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если context равен null.</exception>
        public CheckPermissionResult CheckPermission(IUserContext context, string key)
        {
            return CheckPermission(context, key.GenerateGuid());
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с текущим контекстом (см. <see cref="Users.UserContextManager{TApplication}.GetCurrentUserContext"/>).
        /// </summary>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        public CheckPermissionResult CheckPermission(Guid key)
        {
            return CheckPermission(AppCore.Get<Users.UserContextManager>().GetCurrentUserContext(), key);
        }

        /// <summary>
        /// Проверяет, доступно ли указанное разрешение <paramref name="key"/> пользователю, ассоциированному с контекстом <paramref name="context"/>.
        /// </summary>
        /// <param name="context">Контекст пользователя.</param>
        /// <param name="key">Уникальный ключ разрешения. См. <see cref="Permission.Key"/>.</param>
        /// <returns>Возвращает результат проверки.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="context"/> равен null.</exception>
        public CheckPermissionResult CheckPermission(IUserContext context, Guid key)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            Permission permData = null;
            if (!_permissions.TryGetValue(key, out permData) && key != ModulesConstants.PermissionAccessUser) return CheckPermissionResult.PermissionNotFound;
            if (key == ModulesConstants.PermissionAccessUser) return context.IsGuest ? CheckPermissionResult.Denied : CheckPermissionResult.Allowed;
            if (!permData.IgnoreSuperuser && context.IsSuperuser) return CheckPermissionResult.Allowed;

            Permissions userPermissionsInModule = null;
            return (context.Permissions?.TryGetValue(ModuleID, out userPermissionsInModule) == true && userPermissionsInModule?.Contains(key) == true)
                ? CheckPermissionResult.Allowed : CheckPermissionResult.Denied;
        }
        #endregion

        #region Информация о модуле
        /// <summary>
        /// Возвращает идентификатор модуля <see cref="ID"/>, представленный в виде GUID.
        /// </summary>
        public Guid ModuleID
        {
            get => _moduleBaseID;
        }

        /// <summary>
        /// Возвращает идентификатор модуля в базе данных.
        /// </summary>
        public int ID
        {
            get => _moduleId;
            internal set
            {
                _moduleId = value;
                _moduleBaseID = GuidIdentifierGenerator.GenerateGuid(GuidType.Module, value);
            }
        }

        /// <summary>
        /// Возвращает url-доступное название модуля. Не может быть пустым.
        /// Порядок определения значения свойства следующий:
        /// 1) Если задано - <see cref="ModuleCoreAttribute.DefaultUrlName"/>;
        /// 2) Если задано - <see cref="ModuleConfiguration{TModule}.UrlName"/>;
        /// 3) Если предыдущие пункты не вернули значения - используется <see cref="UniqueName"/>.
        /// </summary>
        /// <seealso cref="ModuleCoreAttribute.DefaultUrlName"/>
        /// <seealso cref="ModuleConfiguration{TModule}.UrlName"/>
        public virtual string UrlName
        {
            get => _moduleUrlName;
        }

        /// <summary>
        /// Возвращает отображаемое название модуля.
        /// </summary>
        public string Caption
        {
            get => _moduleCaption;
        }

        /// <summary>
        /// Возвращает идентификатор модуля.
        /// </summary>
        public int IdModule
        {
            get => ID;
        }

        /// <summary>
        /// Возвращает query-тип модуля.
        /// </summary>
        public virtual Type QueryType
        {
            get => GetType();
        }

        /// <summary>
        /// Возвращает уникальное имя модуля на основе <see cref="Type.FullName"/> query-типа модуля.
        /// </summary>
        public Guid UniqueName
        {
            get => QueryType.FullName.GenerateGuid();
        }

        #endregion

        #region Блок расширений
        /// <summary>
        /// Указывает, что в модуле должно быть доступно расширение <typeparamref name="TExtension"/>.
        /// </summary>
        /// <typeparam name="TExtension"></typeparam>
        public void RegisterExtension<TExtension>() where TExtension : Extensions.ModuleExtension
        {
            if (!HasExtension<TExtension>())
            {
                //Type _type = null;

                //LibraryEnumeratorFactory.Enumerate((assembly) =>
                //{
                //    if (_type == null)
                //    {
                //        var t = assembly.GetType(typeof(TExtension).FullName, false);
                //        if (t != null) _type = t;
                //    }
                //});

                var extension = AppCore.Create<TExtension>();
                if (extension == null) throw new InvalidOperationException("Указанное расширение не зарегистрировано в привязках типов.");
                extension.Module = this;
                extension.Start(AppCore);
                extension._initializeCustoms();
                _extensions.Add(extension);
            }
        }

        private bool HasExtension<TExtension>(bool IsForAdmin = false) where TExtension : Extensions.ModuleExtension
        {
            var tt = typeof(TExtension);
            var extension = (from p in _extensions
                             where (p as TExtension) != null && p.Attributes.IsAdminPart == IsForAdmin
                             select p).FirstOrDefault();

            return (extension != null);
        }

        private TExtension GetExtension<TExtension>(bool IsForAdmin = false) where TExtension : Extensions.ModuleExtension
        {
            var tt = typeof(TExtension);
            var extension = (from p in _extensions
                             where (p as TExtension) != null && p.Attributes.IsAdminPart == IsForAdmin
                             select p).FirstOrDefault();

            return (TExtension)extension;
        }

        /// <summary>
        /// Возвращает список подключенных расширений.
        /// </summary>
        public List<Extensions.ModuleExtension> GetExtensions()
        {
            return _extensions;
        }

        /////**
        ////* Возвращает строку с текущими ошибками расширений. 
        ////* 
        ////* @return string
        ////*/
        ////protected function extensionsGetErrors()
        ////{
        ////    $errors = array();
        ////    foreach ( $this.mExtensions as $k=>$v) 
        ////    {
        ////        if (strlen($v.Error) > 0) $errors[] = $v.Error;
        ////    }

        ////    return implode("; ", $errors);
        ////}
        #endregion

        #region Блок функций, переопределение которых может потребоваться для расширений и других модулей
        /// <summary>
        /// Возвращает список типов объектов, используемых в модуле.
        /// По-умолчанию возвращает <see cref="Items.ItemTypeFactory.ItemType"/> и <see cref="Items.ItemTypeFactory.CategoryType"/>.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<DB.ItemType> GetItemTypes()
        {
            return new DB.ItemType[] { Items.ItemTypeFactory.ItemType, Items.ItemTypeFactory.CategoryType };
        }

        /// <summary>
        /// Возвращает список идентификатор=название указанного типа для текущего модуля. Например, это может быть список категорий.
        /// </summary>
        /// <param name="IdItemType"></param>
        /// <param name="SortOrder"></param>
        /// <param name="_params"></param>
        /// <returns></returns>
        public virtual Types.NestedLinkCollection GetItems(int IdItemType, params object[] _params)
        {
            return null;
        }

        /// <summary>
        /// Возвращает ссылку для переданного объекта.
        /// Вызывается в случае, когда для объекта не был найден адрес в системе маршрутизации по ключу <see cref="Routing.RoutingConstants.MAINKEY"/>.
        /// </summary>
        /// <returns></returns>
        public virtual Uri GenerateLink(Items.ItemBase item)
        {
            throw new NotImplementedException(string.Format("Метод 'GenerateLink' класса '{0}' не определен в производном классе '{1}'", typeof(ModuleCore).FullName, this.GetType().FullName));
        }

        /// <summary>
        /// Возвращает ссылку для переданного объекта.
        /// Вызывается для объектов, для которых не был найден адрес в системе маршрутизации по ключу <see cref="Routing.RoutingConstants.MAINKEY"/>.
        /// </summary>
        /// <returns></returns>
        public virtual IReadOnlyDictionary<Items.ItemBase, Uri> GenerateLinks(IEnumerable<Items.ItemBase> items)
        {
            return items.ToDictionary(x => x, x => GenerateLink(x));
        }

        /// <summary>
        /// Уничтожает и выгружает модуль.
        /// </summary>
        public virtual void Dispose()
        {

        }
        #endregion

        #region Расширения
        /// <summary>
        /// Предоставляет доступ к расширению настраиваемых полей.
        /// </summary>
        public Modules.Extensions.CustomFields.ExtensionCustomsFieldsBase Fields
        {
            get => GetExtension<Modules.Extensions.CustomFields.ExtensionCustomsFieldsBase>();
        }

        /// <summary>
        /// Предоставляет доступ к расширению ЧПУ.
        /// </summary>
        internal Modules.Extensions.ExtensionUrl.ExtensionUrl Urls
        {
            get => GetExtension<Modules.Extensions.ExtensionUrl.ExtensionUrl>();
        }
        #endregion

    }

    /// <summary>
    /// Базовый класс для всех модулей. Обязателен при реализации любых модулей, т.к. при задании привязок в DI проверяется наследование именно от этого класса.
    /// </summary>
    /// <typeparam name="TSelfReference">Должен ссылаться сам на себя.</typeparam>
    public abstract class ModuleCore<TSelfReference> : ModuleCore
        where TSelfReference : ModuleCore<TSelfReference>
    {
        internal ModuleConfigurationManipulator<TSelfReference> _configurationManipulator = null;

        /// <summary>
        /// Создает новый объект модуля. 
        /// </summary>
#pragma warning disable CS0618
        public ModuleCore()
#pragma warning restore CS0618
        {
            if (!typeof(TSelfReference).IsAssignableFrom(this.GetType())) throw new TypeAccessException($"Параметр-тип {nameof(TSelfReference)} должен находиться в цепочке наследования текущего типа.");
        }

        #region Настройки
        /// <summary>
        /// Возвращает объект типа <typeparamref name="TConfiguration"/>, содержащий параметры модуля.
        /// Возвращенный объект находится в режиме "только для чтения" - изменение параметров невозможно, попытка выполнить set вызовет <see cref="InvalidOperationException"/>.
        /// Все объекты конфигурации, созданные путем вызова этого метода, манипулируют одним набором значений. 
        /// </summary>
        /// <exception cref="InvalidOperationException">Возникает, если модуль не зарегистрирован.</exception>
        /// <seealso cref="ModuleConfigurationManipulator{TModule}"/>
        /// <seealso cref="GetConfigurationManipulator"/>
        public TConfiguration GetConfiguration<TConfiguration>()
            where TConfiguration : ModuleConfiguration<TSelfReference>, new()
        {
            return _configurationManipulator.GetUsable<TConfiguration>();
        }

        /// <summary>
        /// Возвращает манипулятор конфигурацией, предоставляющий возможности получения и редактирования конфигурации.
        /// </summary>
        public ModuleConfigurationManipulator<TSelfReference> GetConfigurationManipulator()
        {
            return _configurationManipulator;
        }

        /// <summary>
        /// Вызывается при сохранении настроек модуля. Если для <paramref name="args"/> был вызван <see cref="ConfigurationApplyEventArgs{TModule}.SetFailed(int)"/>, то <see cref="ModuleConfigurationManipulator{TModule}.ApplyConfiguration{TConfiguration}(TConfiguration)"/> вернет <see cref="ApplyConfigurationResult.Failed"/>.
        /// </summary>
        /// <param name="args">Содержит применяемые настройки модуля.</param>
        /// <seealso cref="GetConfigurationManipulator"/>
        /// <seealso cref="ModuleConfigurationManipulator{TModule}.ApplyConfiguration{TConfiguration}(TConfiguration)"/>
        internal protected virtual void OnConfigurationApply(ConfigurationApplyEventArgs<TSelfReference> args)
        {

        }

        /// <summary>
        /// Вызывается после успешного сохранения и применения настроек модуля.
        /// </summary>
        /// <seealso cref="GetConfigurationManipulator"/>
        /// <seealso cref="ModuleConfigurationManipulator{TModule}.ApplyConfiguration{TConfiguration}(TConfiguration)"/>
        internal protected virtual void OnConfigurationApplied()
        {

        }
        #endregion

        internal override void InitModule()
        {
            RegisterPermission(PermissionSaveConfiguration, "Сохранение настроек модуля");
            RegisterPermission(ModulesConstants.PermissionManage, "Управление модулем");

            InitModuleCustom();
            //RegisterAction("extensionsGetData");
        }

        /// <summary>
        /// Возвращает url-доступное название модуля. Не может быть пустым.
        /// Порядок определения значения свойства следующий:
        /// 1) Если задано - <see cref="ModuleCoreAttribute.DefaultUrlName"/>;
        /// 2) Если задано - <see cref="ModuleConfiguration{TModule}.UrlName"/>;
        /// 3) Если предыдущие пункты не вернули значения - используется <see cref="ModuleCore.UniqueName"/>.
        /// </summary>
        /// <seealso cref="ModuleCoreAttribute.DefaultUrlName"/>
        /// <seealso cref="ModuleConfiguration{TModule}.UrlName"/>
        public sealed override string UrlName
        {
            get
            {
                if (string.IsNullOrEmpty(_moduleUrlName)) _moduleUrlName = UniqueName.ToString();
                return _moduleUrlName;
            }
        }

        /// <summary>
        /// Возвращает query-тип модуля.
        /// </summary>
        public sealed override Type QueryType
        {
            get => typeof(TSelfReference);
        }

    }
}