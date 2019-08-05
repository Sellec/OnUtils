using System;
using System.Data.Entity;

namespace OnUtils.Data.EntityFramework.Internal
{
    using UnitOfWork;

    class ModelAccessorInternal : IModelAccessor
    {
        internal Action<DbModelBuilder> ModelBuilderDelegate { get; set; }

        public string ConnectionString { get; set; }
    }
}
