﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SoonFramework.Core.FastReflection
{
    public class MethodInvokerCache : FastReflectionCache<MethodInfo, IMethodInvoker>
    {
        protected override IMethodInvoker Create(MethodInfo key)
        {
            return FastReflectionFactories.MethodInvokerFactory.Create(key);
        }
    }
}
