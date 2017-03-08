using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoonFramework.Core;

namespace SoonFramework.Configuration
{
    public static class ConfigurationExtensions
    {
        private static class MethodCache<TValue>
        {
            public static Func<IConfiguration, string, TValue> GetValue;
        }

        static ConfigurationExtensions()
        {
            typeof(IConfiguration).GetMethod("GetObject").CreateDelegate(out MethodCache<object>.GetValue);
            typeof(IConfiguration).GetMethod("GetString").CreateDelegate(out MethodCache<string>.GetValue);
            typeof(IConfiguration).GetMethod("GetBoolean").CreateDelegate(out MethodCache<bool>.GetValue);
            typeof(IConfiguration).GetMethod("GetChar").CreateDelegate(out MethodCache<char>.GetValue);
            typeof(IConfiguration).GetMethod("GetSByte").CreateDelegate(out MethodCache<sbyte>.GetValue);
            typeof(IConfiguration).GetMethod("GetByte").CreateDelegate(out MethodCache<byte>.GetValue);
            typeof(IConfiguration).GetMethod("GetInt16").CreateDelegate(out MethodCache<short>.GetValue);
            typeof(IConfiguration).GetMethod("GetUInt16").CreateDelegate(out MethodCache<ushort>.GetValue);
            typeof(IConfiguration).GetMethod("GetInt32").CreateDelegate(out MethodCache<int>.GetValue);
            typeof(IConfiguration).GetMethod("GetUInt32").CreateDelegate(out MethodCache<uint>.GetValue);
            typeof(IConfiguration).GetMethod("GetInt64").CreateDelegate(out MethodCache<long>.GetValue);
            typeof(IConfiguration).GetMethod("GetUInt64").CreateDelegate(out MethodCache<ulong>.GetValue);
            typeof(IConfiguration).GetMethod("GetSingle").CreateDelegate(out MethodCache<float>.GetValue);
            typeof(IConfiguration).GetMethod("GetDouble").CreateDelegate(out MethodCache<double>.GetValue);
            typeof(IConfiguration).GetMethod("GetDecimal").CreateDelegate(out MethodCache<decimal>.GetValue);
            typeof(IConfiguration).GetMethod("GetDateTime").CreateDelegate(out MethodCache<DateTime>.GetValue);
        }

        public static TValue GetValue<TValue>(this IConfiguration configuration, string key)
        {
            return MethodCache<TValue>.GetValue(configuration, key);
        }
    }
}
