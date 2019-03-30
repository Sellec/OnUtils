using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.Validation
{
    /// <summary>
    /// Представляет исключение, вызываемое из <see cref="IDataContext.SaveChanges()"/>, когда сущности не проходят проверку.
    /// </summary>
    public class EntityValidationException : Exception
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса EntityValidationException, используя значения по умолчанию.
        /// </summary>
        public EntityValidationException() : this(null, null, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса EntityValidationException с указанным сообщением об ошибке.
        /// </summary>
        public EntityValidationException(string message) : this(message, null, null)
        {
        }

        /// <summary>
        /// Выполняет инициализацию нового экземпляра класса EntityValidationException с указанным сообщением об ошибке и результатами проверки.
        /// </summary>
        public EntityValidationException(string message, IEnumerable<EntityValidationResult> entityValidationResults) : this(message, entityValidationResults, null)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр EntityValidationException с указанными сообщением об ошибке и внутренним исключением.
        /// </summary>
        public EntityValidationException(string message, Exception innerException) : this(message, null, innerException)
        {
        }

        /// <summary>
        /// Инициализирует новый экземпляр EntityValidationException с указанными сообщением об ошибке, результатами проверки и внутренним исключением.
        /// </summary>
        public EntityValidationException(string message, IEnumerable<EntityValidationResult> entityValidationResults, Exception innerException) : base(message, innerException)
        {
            this.EntityValidationErrors = entityValidationResults;
        }

        /// <summary>
        /// Получает ошибки проверки, связанные с сущностью.
        /// </summary>
        public IEnumerable<EntityValidationResult> EntityValidationErrors { get; }

        /// <summary>
        /// Возвращает комплексное сообщение об ошибке.
        /// </summary>
        /// <param name="glueBefore">Подставляется перед каждой ошибкой.</param>
        /// <param name="glueAfter">Подставляется после каждой ошибки.</param>
        /// <returns></returns>
        public string CreateComplexMessage(string glueBefore = " - ", string glueAfter = ";\r\n")
        {
            var error = "";
            var parts = new List<string>();
            foreach (var _error in this.EntityValidationErrors)
            {
                if (!_error.IsValid)
                {
                    foreach (var __error in _error.ValidationErrors)
                    {
                        var errorMessage = glueBefore + __error.ErrorMessage;
                        if (!string.IsNullOrEmpty(glueAfter))
                        {
                            if (!errorMessage.Last().In('.', ',', ';', '!', '?')) errorMessage += glueAfter;
                        }

                        parts.Add(errorMessage);
                    }
                }
            }

            error = string.Join("", parts);


            return error;
        }



    }
}
