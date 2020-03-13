using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.Errors
{
    /// <summary>
    /// Исключение, выбрасываемое <see cref="IDataContext.SaveChanges()"/>, если применение изменений провалилось.
    /// </summary>
    public class UpdateException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса UpdateException, используя значения по умолчанию.
        /// </summary>
        public UpdateException() : this(null, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса UpdateException с указанным сообщением об ошибке.
        /// </summary>
        public UpdateException(string message) : this(message, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр UpdateException с указанными сообщением об ошибке и внутренним исключением.
        /// </summary>
        public UpdateException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр UpdateException с указанными сообщением об ошибке, внутренним исключением и списком сущностей, для которых возникли ошибки сохранения.
        /// </summary>
        public UpdateException(string message, Exception innerException, IEnumerable<IRepositoryEntry> entries) : base(message, innerException)
        {
            this.Entries = entries;
        }

        /// <summary>
        /// Возвращает объекты <see cref="IRepositoryEntry"/>, представляющие сущности, которые не могли быть сохранены в базе данных.
        /// </summary>
        public IEnumerable<IRepositoryEntry> Entries
        {
            get;
            private set;
        }
    }
}
