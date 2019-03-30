using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OnUtils.Data.EntityFramework
{
    using UnitOfWork;

    /// <summary>
    /// </summary>
    public static class ModelAccessorEntityFrameworkCoreExtensions
    {
        /// <summary>
        /// Предоставляет доступ к процессу создания модели в EntityFramework Core через делегат <paramref name="modelBuilderDelegate"/>.
        /// </summary>
        public static void UseEntityFrameworkCore(this IModelAccessor modelAccessor, Action<ModelBuilder> modelBuilderDelegate)
        {
            if (modelAccessor is Internal.ModelAccessorInternal modelAccessorInternal)
            {
                modelAccessorInternal.ModelBuilderDelegate = modelBuilderDelegate;
            }
            else
            {
                throw new InvalidOperationException("This can be used only for EF Core context.");
            }
        }
    }
}
