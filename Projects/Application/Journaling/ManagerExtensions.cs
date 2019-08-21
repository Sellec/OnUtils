using System;
using System.Linq;

namespace OnUtils.Application
{
    using Application;
    using Architecture.AppCore;
    using Application.Items;
    using Application.Journaling;
    using Application.Journaling.DB;
    using ExecutionResultJournalName = ExecutionResult<Application.Journaling.DB.JournalNameDAO>;

    /// <summary>
    /// Методы расширений для <see cref="JournalingManager{TAppCoreSelfReference}"/>.
    /// </summary>
    public static class ManagerExtensions
    {
        internal static Type GetJournalType(Type type)
        {
            var typeGeneric = OnUtils.Types.TypeHelpers.ExtractGenericInterface(type, typeof(ITypedJournalComponent<>));
            if (typeGeneric != null) return typeGeneric.GetGenericArguments()[0];

            return type;
        }

        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="nameJournal">См. <see cref="JournalNameDAO.Name"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="nameJournal"/> представляет пустую строку или null.</exception>
        public static ExecutionResult RegisterJournal<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, string nameJournal)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterJournalTyped(component.GetType(), nameJournal);
        }

        #region RegisterEvent
        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEvent<TAppCoreSelfReference>(component, eventType, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEvent<TAppCoreSelfReference>(component, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            //return component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterJournalTyped(type, nameJournal);
            return component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterEvent(component.GetType(), eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion

        #region RegisterEventForItem
        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.IdRelatedItem"/> и <see cref="JournalDAO.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEventForItem<TAppCoreSelfReference>(component, relatedItem, eventType, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.IdRelatedItem"/> и <see cref="JournalDAO.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEventForItem<TAppCoreSelfReference>(component, relatedItem, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале на основе компонента <paramref name="component"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="JournalDAO.IdRelatedItem"/> и <see cref="JournalDAO.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="JournalDAO.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="JournalDAO.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="JournalDAO.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="JournalDAO.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="JournalDAO.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterEventForItem(component.GetType(), relatedItem, eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion
    }
}
