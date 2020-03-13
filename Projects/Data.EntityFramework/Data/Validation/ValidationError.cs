using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.Validation
{
    /// <summary>
    /// Ошибка проверки. Ошибка проверки может быть на уровне сущности или на уровне свойства.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Создает экземпляр класса ValidationError с именем свойства <paramref name="propertyName"/> (может быть пустым) и конкретной ошибкой <paramref name="errorMessage"/>.
        /// </summary>
        public ValidationError(string propertyName, string errorMessage)
        {
            this.PropertyName = propertyName;
            this.ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Возвращает сообщение об ошибке проверки.
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Возвращает имя недопустимого свойства.
        /// Если ошибка относится к сущности, то вернет null.
        /// </summary>
        public string PropertyName { get; }
    }
}
