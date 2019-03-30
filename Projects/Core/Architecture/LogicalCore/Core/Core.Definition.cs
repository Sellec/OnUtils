using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TraceStudio.Utils.Architecture.LogicalCore
{
    /// <summary>
    /// </summary>
    public abstract partial class CoreBase
    {
        private Type _constraintType = null;

        internal CoreBase(Type constraintType)
        {
            var t = TraceStudio.Utils.Types.TypeHelpers.ExtractGenericType(this.GetType(), typeof(Core<>));
            if (t == null) throw new TypeAccessException("CoreBase нельзя использовать нигде, кроме как в Core.");

            _constraintType = constraintType;
        }
    }

    /// <summary>
    /// Представляет ядро логики.
    /// Ядро логики обеспечивает работу компонентов, входящих в его состав (два вида серверных компонентов - менеджеры и операции).
    /// По сути и менеджеры и операции - это и есть бизнес-логика, однако основная часть именно "бизнеса" сосредоточена в операциях. 
    /// Менеджеры - это больше инфраструктурные компоненты, такие, как диспетчеры вызовов, менеджеры настроек, фабрики компонентов и пр. 
    /// Можно условно обозначить менеджеры уровнем 2.1, а операции  уровнем 2.2, т.е. уровень операций в иерархии слоёв приложения выше, чем уровень менеджеров. 
    /// Таким образом, менеджеры не должны ничего знать об операциях, а операции могут обращаться к менеджерам.
    /// </summary>
    public abstract partial class Core<TOperationContract> : CoreBase
    {
        private ModularCore _modularCore = null;
        private Core.Modular.Base.CoreComponentState _coreState = Core.Modular.Base.CoreComponentState.None;
        private bool _isUseFactoryPool = false;

        /// <summary>
        /// Создает новый объект ядра. 
        /// Параметр <paramref name="isUseFactoryPool"/> указывает, следует ли использовать пул фабрик для создания объектов кроме механизма задания реализаций. См. <see cref="Core.Modular.AppCore{TAppCore, TSingletonInterface, TTransientInterface}.AppCore(bool)"/>.
        /// </summary>
        public Core(bool isUseFactoryPool) : base(typeof(TOperationContract))
        {

        }

        /// <summary>
        /// Запускает ядро логики.
        /// </summary>
        public void Start()
        {
            if (_coreState == Core.Modular.Base.CoreComponentState.Started) throw new InvalidOperationException("Ядро логики уже запущено.");
            if (_coreState == Core.Modular.Base.CoreComponentState.Stopped) throw new InvalidOperationException("Ядро логики остановлено. Повторный запуск остановленных ядер запрещен.");

            try
            {
                var core = new ModularCore();
                core.Start();

                _modularCore = core;
                _coreState = TraceStudio.Utils.Architecture.Core.Modular.Base.CoreComponentState.Started;

                RequestClientDispatcher._runningInstances.AddIfNotExists(this);
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// </summary>
        public void Stop()
        {
            try
            {
                if (_coreState != TraceStudio.Utils.Architecture.Core.Modular.Base.CoreComponentState.Started) return;

                _modularCore.Stop();

                if (RequestClientDispatcher._runningInstances.Contains(this))
                    RequestClientDispatcher._runningInstances.Remove(this);
            }
            catch
            {
                throw;
            }
        }


    }

}
