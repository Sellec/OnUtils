using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.Errors
{
    /// <summary>
    /// Исключение, выбрасываемое <see cref="IDataContext"/>, если во время применения изменений обнаружилось, что в источнике данных были произведены изменения с момента последнего получения данных.
    /// </summary>
    public class UpdateConcurrencyException : UpdateException
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса UpdateConcurrencyException, используя значения по умолчанию.
        /// </summary>
        public UpdateConcurrencyException() : base()
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса UpdateConcurrencyException с указанным сообщением об ошибке.
        /// </summary>
        public UpdateConcurrencyException(string message) : base(message)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр UpdateConcurrencyException с указанными сообщением об ошибке и внутренним исключением.
        /// </summary>
        public UpdateConcurrencyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр UpdateConcurrencyException с указанными сообщением об ошибке, внутренним исключением и списком сущностей, для которых возникли ошибки сохранения.
        /// </summary>
        public UpdateConcurrencyException(string message, Exception innerException, IEnumerable<IRepositoryEntry> entries) : base(message, innerException, entries)
        {
        }

    }
}
