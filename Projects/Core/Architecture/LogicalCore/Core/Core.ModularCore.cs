namespace TraceStudio.Utils.Architecture.LogicalCore
{
    public partial class Core<TOperationContract>
    {
        /// <summary>
        /// Интерфейс-заглушка для 
        /// </summary>
        public interface IModularComponentSingleton
        {
        }

        /// <summary>
        /// Реализация модульного ядра из TraceStudio.Utils для использования операций и менеджеров в виде модулей.
        /// </summary>
        public class ModularCore : Core.Modular.AppCore<ModularCore, IModularComponentSingleton, TOperationContract>
        {
            /// <summary>
            /// Создает новый объект ядра. 
            /// Параметр <paramref name="isUseFactoryPool"/> указывает, следует ли использовать пул фабрик для создания объектов кроме механизма задания реализаций. См. <see cref="Core.Modular.AppCore{TAppCore, TSingletonInterface, TTransientInterface}.AppCore(bool)"/>.
            /// </summary>
            public ModularCore(bool isUseFactoryPool) : base(isUseFactoryPool)
            {
            }
        }

    }
}
