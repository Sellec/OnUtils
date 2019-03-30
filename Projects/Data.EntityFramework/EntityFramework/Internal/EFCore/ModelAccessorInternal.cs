using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace OnUtils.Data.EntityFramework.Internal
{
    using UnitOfWork;

    class ModelAccessorInternal : IModelAccessor
    {
        internal Action<ModelBuilder> ModelBuilderDelegate { get; set; }
    }
}
