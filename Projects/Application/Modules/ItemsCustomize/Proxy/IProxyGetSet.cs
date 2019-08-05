﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnUtils.Application.Modules.ItemsCustomize.Proxy
{
    interface IProxyGetSet
    {
        TOutType ProxyGetValue<TOutType>(int IdField);

        void ProxySetValue<TOutType>(int IdField, TOutType value);
    }
}