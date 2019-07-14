﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace OnUtils.Application.ServiceMonitor
{
    using Architecture.AppCore;
    using Data;
    using Journaling.DB;

    /// <summary>
    /// Монитор сервисов. Позволяет вносить и получать информацию о сервисах, о их состоянии, событиях и пр.
    /// </summary>
    public class Monitor : CoreComponentBase<ApplicationCore>, IComponentSingleton<ApplicationCore>, IUnitOfWorkAccessor<UnitOfWork<Journal>>
    {
        private static ConcurrentDictionary<Guid, JournalName> _servicesJournalsList = new ConcurrentDictionary<Guid, JournalName>();
        private static ConcurrentDictionary<Guid, ServiceInfo> _servicesList = new ConcurrentDictionary<Guid, ServiceInfo>();

        #region CoreComponentBase
        /// <summary>
        /// </summary>
        protected sealed override void OnStart()
        {
        }

        /// <summary>
        /// </summary>
        protected sealed override void OnStop()
        {
        }
        #endregion

        /// <summary>
        /// Фиксирует состояние сервиса без записи в журнал.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        public void RegisterServiceStateWithoutJournal(IMonitoredService service, ServiceStatus serviceStatus, string serviceStatusDetailed = null)
        {
            var newState = new ServiceInfo()
            {
                ID = service.ServiceID,
                Name = service.ServiceName,
                LastStatus = serviceStatus,
                LastStatusDetailed = serviceStatusDetailed,
                LastDateEvent = DateTime.Now
            };

            _servicesList.AddOrUpdate(service.ServiceID, newState, (k, i) => newState);
        }

        /// <summary>
        /// Фиксирует состояние сервиса.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        /// <param name="exception">Ошибки, если были зарегистрированы.</param>
        public void RegisterServiceState(IMonitoredService service, ServiceStatus serviceStatus, string serviceStatusDetailed = null, Exception exception = null)
        {
            RegisterServiceStateWithoutJournal(service, serviceStatus, serviceStatusDetailed);

            var eventType = Journaling.EventType.Info;
            if (serviceStatus == ServiceStatus.RunningIdeal) eventType = Journaling.EventType.Info;
            else if (serviceStatus == ServiceStatus.RunningWithErrors) eventType = Journaling.EventType.Error;
            else if (serviceStatus == ServiceStatus.CannotRunBecouseOfErrors) eventType = Journaling.EventType.CriticalError;
            else if (serviceStatus == ServiceStatus.Shutdown) eventType = Journaling.EventType.Info;

            RegisterServiceEvent(service, eventType, serviceStatus.ToStringFriendly(), serviceStatusDetailed, exception);
        }

        /// <summary>
        /// Записывает в журнал сервиса событие, связанное с сервисом.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="eventType">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="exception">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        public void RegisterServiceEvent(IMonitoredService service, Journaling.EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            var serviceJournal = GetJournalName(service);
            if (serviceJournal != null) AppCore.Get<Journaling.JournalingManager>().RegisterEvent(serviceJournal.IdJournal, eventType, eventInfo, eventInfoDetailed, null, exception);
        }

        private JournalName GetJournalName(IMonitoredService service)
        {
            return _servicesJournalsList.GetOrAddWithExpiration(service.ServiceID, (key) =>
            {
                var result = AppCore.Get<Journaling.JournalingManager>().RegisterJournal(1, service.ServiceName, "ServiceMonitor_" + key.ToString());
                if (!result.IsSuccess) this.RegisterEvent(Journaling.EventType.Error, "Не удалось зарегистрировать журнал мониторинга", $"Журнал для типа '{service.GetType().FullName}'");
                return result.Result;
            }, TimeSpan.FromMinutes(15));
        }

        /// <summary>
        /// Возвращает журнал для указанного сервиса.
        /// </summary>
        public IEnumerable<Journal> GetServiceJournal(IMonitoredService service)
        {
            var serviceJournal = GetJournalName(service);
            return GetServiceJournal(service.ServiceID);
        }

        /// <summary>
        /// Возвращает журнал для указанного идентификатора сервиса.
        /// </summary>
        public IEnumerable<Journal> GetServiceJournal(Guid serviceID)
        {
            if (_servicesJournalsList.TryGetValue(serviceID, out JournalName serviceJournal))
            {
                using (var db = this.CreateUnitOfWork())
                {
                    return db.Repo1.
                        Where(x => x.IdJournal == serviceJournal.IdJournal).
                        OrderByDescending(x => x.DateEvent).
                        ToList();
                }
            }
            else
            {
                return Enumerable.Empty<Journal>();
            }
        }

        /// <summary>
        /// Возвращает список сервисов.
        /// </summary>
        public IDictionary<Guid, ServiceInfo> GetServicesList()
        {
            return _servicesList;
        }

        /// <summary>
        /// Возвращает сервис с указанным идентификатором.
        /// </summary>
        /// <returns>Объект с данными сервиса или null, если сервис не найден.</returns>
        public ServiceInfo GetService(Guid serviceID)
        {
            return _servicesList.TryGetValue(serviceID, out ServiceInfo serviceInfo) ? serviceInfo : null;
        }
    }
}

namespace System
{
    using Journaling = OnUtils.Application.Journaling;
    using ServiceMonitor = OnUtils.Application.ServiceMonitor;

    /// <summary>
    /// </summary>
    public static class MonitorExtension
    {
        /// <summary>
        /// Фиксирует состояние сервиса на момент вызова метода.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="serviceStatus">Состояние сервиса.</param>
        /// <param name="serviceStatusDetailed">Детализированное состояние сервиса.</param>
        /// <param name="exception">Ошибки, если были зарегистрированы.</param>
        public static void RegisterServiceState(this ServiceMonitor.IMonitoredService service, ServiceMonitor.ServiceStatus serviceStatus, string serviceStatusDetailed = null, Exception exception = null)
        {
            service.GetAppCore().Get<ServiceMonitor.Monitor>()?.RegisterServiceState(service, serviceStatus, serviceStatusDetailed, exception);
        }

        /// <summary>
        /// Записывает в журнал сервиса событие, связанное с сервисом.
        /// </summary>
        /// <param name="service">Сервис, для которого производится регистрация состояния.</param>
        /// <param name="eventType">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="eventInfo">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="eventInfoDetailed">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        /// <param name="exception">См. <see cref="Journaling.JournalingManager.RegisterEvent"/>.</param>
        public static void RegisterServiceEvent(this ServiceMonitor.IMonitoredService service, Journaling.EventType eventType, string eventInfo, string eventInfoDetailed = null, Exception exception = null)
        {
            service.GetAppCore().Get<ServiceMonitor.Monitor>()?.RegisterServiceEvent(service, eventType, eventInfo, eventInfoDetailed, exception);
        }

    }
}