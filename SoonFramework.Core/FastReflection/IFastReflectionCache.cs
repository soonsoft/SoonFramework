using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SoonFramework.Core.FastReflection
{
    public interface IFastReflectionCache<TKey, TValue>
    {
        TValue Get(TKey key);
    }
}
