using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class StringExtensions
    {
        #region 正则

        /// <summary>
        /// 是否正则匹配
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static bool IsMatch(this string s, string pattern)
        {
            if (s == null) return false;
            else return Regex.IsMatch(s, pattern);
        }

        /// <summary>
        /// 正则查询
        /// </summary>
        /// <param name="s">字符串</param>
        /// <param name="pattern">正则表达式</param>
        /// <returns></returns>
        public static string Match(this string s, string pattern)
        {
            if (s == null) return String.Empty;
            return Regex.Match(s, pattern).Value;
        }

        #endregion


    }
}
