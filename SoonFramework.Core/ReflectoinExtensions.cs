using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class ReflectoinExtensions
    {
        public static TDelegate CreateDelegate<TDelegate>(this MethodInfo method)
        {
            if(method == null)
            {
                throw new ArgumentNullException("method");
            }

            return (TDelegate)(object)Delegate.CreateDelegate(typeof(TDelegate), method);
        }

        public static void CreateDelegate<TDelegate>(this MethodInfo method, out TDelegate result)
        {
            result = CreateDelegate<TDelegate>(method);
        }
    }
}
