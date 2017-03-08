using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    /// <summary>
    /// Linq集合扩展方法
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 判断集合是否为空
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">集合</param>
        /// <returns>集合为空返回true，集合不为空返回false</returns>
        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        /// <summary>
        /// 判断集合是否不为空
        /// </summary>
        /// <typeparam name="T">集合元素类型</typeparam>
        /// <param name="source">集合</param>
        /// <returns>集合为空返回true，集合不为空返回false</returns>
        public static bool IsNotEmpty<T>(this IEnumerable<T> source)
        {
            return source.Any();
        }
    }
}
