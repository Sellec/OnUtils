using Microsoft.EntityFrameworkCore;
using System;

namespace OnUtils.Data.EntityFramework.Internal
{
    using UnitOfWork;

    class ModelAccessorInternal : IModelAccessor
    {
        internal Action<DbContextOptionsBuilder> ConfiguringDelegate { get; set; }
        internal Action<ModelBuilder> ModelCreatingDelegate { get; set; }

        public string ConnectionString { get; set; }
    }
}
