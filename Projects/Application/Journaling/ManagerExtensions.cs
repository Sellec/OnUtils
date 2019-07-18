using System;

namespace OnUtils.Application
{
    using Architecture.AppCore;
    using Items;
    using Journaling;
    using Journaling.DB;
    using ExecutionResultJournalName = ExecutionResult<Journaling.DB.JournalName>;

    /// <summary>
    /// Методы расширений для <see cref="JournalingManager"/>.
    /// </summary>
    public static class ManagerExtensions
    {
        /// <summary>
        /// Регистрирует новый журнал или обновляет старый на основе типа <typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="nameJournal">См. <see cref="JournalName.Name"/>.</param>
        /// <returns>Возвращает объект <see cref="ExecutionResultJournalName"/> со свойством <see cref="ExecutionResult.IsSuccess"/> в зависимости от успешности выполнения операции. В случае ошибки свойство <see cref="ExecutionResult.Message"/> содержит сообщение об ошибке.</returns>
        /// <exception cref="ArgumentNullException">Возникает, если <paramref name="nameJournal"/> представляет пустую строку или null.</exception>
        public static ExecutionResult RegisterJournal<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, string nameJournal)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return new ExecutionResult(false, "test");// component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterJournalTyped<TApplicationComponent>(nameJournal);
        }

        #region RegisterEvent
        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента<typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEvent<TAppCoreSelfReference>(component, eventType, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента <typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="Journal.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEvent<TAppCoreSelfReference>(component, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента <typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="Journal.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="Journal.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult<int?> RegisterEvent<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return new ExecutionResult<int?>(false, "test");// component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterEvent<TApplicationComponent>(eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion

        #region RegisterEventForItem
        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента<typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="Journal.IdRelatedItem"/> и <see cref="Journal.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEventForItem<TAppCoreSelfReference>(component, relatedItem, eventType, eventInfo, eventInfoDetailed, null);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента <typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="Journal.IdRelatedItem"/> и <see cref="Journal.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <param name="exception">См. <see cref="Journal.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return ManagerExtensions.RegisterEventForItem<TAppCoreSelfReference>(component, relatedItem, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        /// <summary>
        /// Регистрирует новое событие в журнале, основанном на типе компонента <typeparamref name="TApplicationComponent"/>.
        /// </summary>
        /// <param name="component">Компонент приложения (см. <see cref="IComponentSingleton{TAppCore}"/>) для которого регистрируется событие.</param>
        /// <param name="relatedItem">См. <see cref="Journal.IdRelatedItem"/> и <see cref="Journal.IdRelatedItemType"/>.</param>
        /// <param name="eventType">См. <see cref="Journal.EventType"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journal.EventInfo"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journal.EventInfoDetailed"/>.</param>
        /// <param name="eventTime">См. <see cref="Journal.DateEvent"/>. Если передано значение null, то событие записывается на момент вызова метода.</param>
        /// <param name="exception">См. <see cref="Journal.ExceptionDetailed"/>.</param>
        /// <returns>Возвращает объект с результатом выполнения операции. Если во время добавления события в журнал возникла ошибка, она будет отражена в сообщении <see cref="ExecutionResult.Message"/>.</returns>
        public static ExecutionResult RegisterEventForItem<TAppCoreSelfReference>(this IComponentSingleton<TAppCoreSelfReference> component, ItemBase<TAppCoreSelfReference> relatedItem, EventType eventType, string eventInfo, string eventInfoDetailed = null, DateTime? eventTime = null, Exception exception = null)
            where TAppCoreSelfReference : ApplicationCore<TAppCoreSelfReference>
        {
            return new ExecutionResult(false, "test");// component.GetAppCore().Get<JournalingManager<TAppCoreSelfReference>>().RegisterEventForItem<TApplicationComponent>(relatedItem, eventType, eventInfo, eventInfoDetailed, eventTime, exception);
        }
        #endregion
    }
}
