using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Data.Validation
{
    /// <summary>
    /// Представляет результаты проверки для одной сущности.
    /// </summary>
    public class EntityValidationResult
    {
        /// <summary>
        /// Создает экземпляр класса EntityValidationResult.
        /// </summary>
        public EntityValidationResult(IRepositoryEntry entry, IEnumerable<ValidationError> validationErrors)
        {
            this.Entry = entry;
            this.ValidationErrors = new List<ValidationError>(validationErrors);
        }

        /// <summary>
        /// Возвращает экземпляр класса <see cref="IRepositoryEntry"/> , к которому применяются результаты.
        /// </summary>
        public IRepositoryEntry Entry { get; }

        /// <summary>
        /// Возвращает значение, указывающее, допустима ли сущность.
        /// </summary>
        public bool IsValid
        {
            get { return ValidationErrors == null || ValidationErrors.Count == 0; }
        }

        /// <summary>
        /// Возвращает ошибки проверки. Не может иметь значение NULL.
        /// </summary>
        public ICollection<ValidationError> ValidationErrors { get; }
    }
}
