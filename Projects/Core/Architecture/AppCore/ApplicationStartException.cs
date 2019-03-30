using System;

namespace OnUtils.Architecture.AppCore
{
    /// <summary>
    /// Исключение, которое содержит исключения, возникающие во время запуска ядра приложения.
    /// </summary>
    public class ApplicationStartException : Exception
    {
        /// <summary>
        /// Создает новый экземпляр исключения с указанным шагом запуска ядра и исключением, возникшим в ядре.
        /// </summary>
        public ApplicationStartException(ApplicationStartStep step, Type contextType, Exception exception) : base(null, exception)
        {
            Step = step;
            ContextType = contextType;
        }

        /// <summary>
        /// Указывает на этап запуска ядра, во время выполнения которого возникло исключение.
        /// </summary>
        public ApplicationStartStep Step { get; }

        /// <summary>
        /// Указывает на тип, в методах которого возникло исключение.
        /// </summary>
        public Type ContextType { get; }
    }
}
