using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Entity;

namespace OnUtils.Data.EntityFramework.Internal
{
    using UnitOfWork;

    class ModelAccessorInternal : IModelAccessor
    {
        internal Action<DbModelBuilder> ModelBuilderDelegate { get; set; }
    }
}
