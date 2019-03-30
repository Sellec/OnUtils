using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// Представляет исключение, обработанное кодом движка, т.е. такое, которое можно использовать для отображения пользователю.
    /// </summary>
    public class HandledException : Exception
    {
        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса, используя указанное сообщение об ошибке.
        /// </summary>
        public HandledException(string message) : this(message, null)
        {
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса с указанным сообщением об ошибке и ссылкой на внутреннее исключение, которое стало причиной данного исключения.
        /// </summary>
        public HandledException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Аналогично созданию нового экземпляра класса с передачей сообщения "Неожиданная ошибка" и ссылкой на исключение <paramref name="ex"/>.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static HandledException FromUnhandledException(Exception ex)
        {
            return new HandledException("Неожиданная ошибка", ex);
        }

    }
}