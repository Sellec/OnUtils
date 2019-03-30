using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;

namespace OnUtils.Data.EntityFramework
{
    using UnitOfWork;

    /// <summary>
    /// </summary>
    public static class ModelAccessorEntityFrameworkExtensions
    {
        /// <summary>
        /// Предоставляет доступ к процессу создания модели в EntityFramework через делегат <paramref name="modelBuilderDelegate"/>.
        /// </summary>
        public static void UseEntityFramework(this IModelAccessor modelAccessor, Action<DbModelBuilder> modelBuilderDelegate)
        {
            if (modelAccessor is Internal.ModelAccessorInternal modelAccessorInternal)
            {
                modelAccessorInternal.ModelBuilderDelegate = modelBuilderDelegate;
            }
            else
            {
                throw new InvalidOperationException("This can be used only for EF context.");
            }
        }
    }
}
