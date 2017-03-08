using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// DbDataReader 扩展方法
    /// </summary>
    public static class DbDataReaderExtensions
    {
        private const string TrueValue = "1";
        private const string TrueString = "true";

        /// <summary>
        /// 检查值是否为Null
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>bool</returns>
        static bool CheckNull(object value)
        {
            return value == null || value is DBNull;
        }

        /// <summary>
        /// 获取一个Guid值，没有则返回Null
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns>Nullable&lt;Guid&gt;</returns>
        public static Guid? GetGuidOrNull(this DbDataReader reader, string columnName)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return null;
            if(value is Guid)
            {
                return (Guid)value;
            }
            else
            {
                Guid guidValue;
                if(Guid.TryParse(value.ToString(), out guidValue))
                {
                    return guidValue;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取一个Guid值，没有则返回Null
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <returns></returns>
        public static Guid? GetGuidOrNull(this DbDataReader reader, int index)
        {
            object value = reader[index];
            if (CheckNull(value))
                return null;
            if (value is Guid)
            {
                return (Guid)value;
            }
            else
            {
                Guid guidValue;
                if (Guid.TryParse(value.ToString(), out guidValue))
                {
                    return guidValue;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 获取一个DateTime值，没有则返回Null
        /// 可以指定默认返回值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static DateTime? GetDateTimeOrDefault(this DbDataReader reader, string columnName, DateTime? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return Convert.ToDateTime(value);
        }

        /// <summary>
        /// 获取一个DateTime值，没有则返回Null
        /// 可以指定默认返回值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns></returns>
        public static DateTime? GetDateTimeOrDefault(this DbDataReader reader, int index, DateTime? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return Convert.ToDateTime(value);
        }

        /// <summary>
        /// 获取String，没有则返回Null
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <returns></returns>
        public static string GetStringOrNull(this DbDataReader reader, string columnName)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return null;
            return value.ToString();
        }

        /// <summary>
        /// 获取String，没有则返回Null
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <returns></returns>
        public static string GetStringOrNull(this DbDataReader reader, int index)
        {
            object value = reader[index];
            if (CheckNull(value))
                return null;
            return value.ToString();
        }

        /// <summary>
        /// 获取Float，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static float? GetFloatOrDefault(this DbDataReader reader, string columnName, float? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return float.Parse(value.ToString());
        }

        /// <summary>
        /// 获取Float，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static float? GetFloatOrDefault(this DbDataReader reader, int index, float? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return float.Parse(value.ToString());
        }

        /// <summary>
        /// 获取Double，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static double? GetDoubleOrDefault(this DbDataReader reader, string columnName, double? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return double.Parse(value.ToString());
        }

        /// <summary>
        /// 获取Double，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static double? GetDoubleOrDefault(this DbDataReader reader, int index, double? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return double.Parse(value.ToString());
        }

        /// <summary>
        /// 获取decimal，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static decimal? GetDecimalOrDefault(this DbDataReader reader, string columnName, decimal? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return decimal.Parse(value.ToString());
        }

        /// <summary>
        /// 获取decimal，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static decimal? GetDecimalOrDefault(this DbDataReader reader, int index, decimal? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return decimal.Parse(value.ToString());
        }

        /// <summary>
        /// 获取short，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static short? GetShortOrDefault(this DbDataReader reader, string columnName, short? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return short.Parse(value.ToString());
        }

        /// <summary>
        /// 获取short，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static short? GetShortOrDefault(this DbDataReader reader, int index, short? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return short.Parse(value.ToString());
        }

        /// <summary>
        /// 获取int，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static int? GetIntOrDefault(this DbDataReader reader, string columnName, int? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return int.Parse(value.ToString());
        }

        /// <summary>
        /// 获取int，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static int? GetIntOrDefault(this DbDataReader reader, int index, int? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return int.Parse(value.ToString());
        }

        /// <summary>
        /// 获取long，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static long? GetLongOrDefault(this DbDataReader reader, string columnName, long? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            return long.Parse(value.ToString());
        }

        /// <summary>
        /// 获取long，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static long? GetLongOrDefault(this DbDataReader reader, int index, long? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            return long.Parse(value.ToString());
        }

        /// <summary>
        /// 获取boolean，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="columnName">列名</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static bool? GetBooleanOrDefault(this DbDataReader reader, string columnName, bool? defaultValue = null)
        {
            object value = reader[columnName];
            if (CheckNull(value))
                return defaultValue;
            string value2 = value.ToString().ToLower();
            return (value2 == TrueString || value2 == TrueValue);
        }

        /// <summary>
        /// 获取boolean，没有可以返回指定的默认值
        /// </summary>
        /// <param name="reader">DbDataReader</param>
        /// <param name="index">列索引</param>
        /// <param name="defaultValue">默认值，默认为Null</param>
        /// <returns></returns>
        public static bool? GetBooleanOrDefault(this DbDataReader reader, int index, bool? defaultValue = null)
        {
            object value = reader[index];
            if (CheckNull(value))
                return defaultValue;
            string value2 = value.ToString().ToLower();
            return (value2 == TrueString || value2 == TrueValue);
        }
    }
}
