using Microsoft.EntityFrameworkCore;
using System;

namespace OnUtils.Data.EntityFramework.Internal
{
    using UnitOfWork;

    class ModelAccessorInternal : IModelAccessor
    {
        internal Action<ModelBuilder> ModelBuilderDelegate { get; set; }

        public string ConnectionString { get; set; }
    }
}
