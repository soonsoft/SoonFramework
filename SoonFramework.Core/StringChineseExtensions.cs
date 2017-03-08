using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class StringChineseExtensions
    {
        /// <summary>
        /// 转全角文本 （SBC case）
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>全角字符串</returns>
        public static string ToSBC(this string input)
        {
            if(String.IsNullOrEmpty(input))
            {
                return input;
            }

            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 32)
                {
                    c[i] = (char)12288;
                    continue;
                }
                if (c[i] < 127)
                    c[i] = (char)(c[i] + 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 转半角文本 （DBC case）
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>半角字符串</returns>
        public static string ToDBC(this string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new string(c);
        }

        /// <summary>
        /// 获取汉字拼音首字母
        /// </summary>
        /// <param name="s">输入</param>
        /// <returns>汉字拼音首字母，其它字符原样返回</returns>
        public static string GetChineseSpell(this string str)
        {
            if(String.IsNullOrEmpty(str))
            {
                return str;
            }

            char[] source = str.ToCharArray();
            char[] value = new char[source.Length];
            Encoding defaultEncoding = Encoding.Default;
            char c;
            byte[] bytes;
            for (int i = 0; i < source.Length; i++)
            {
                c = source[i];
                bytes = defaultEncoding.GetBytes(source, i, 1);
                if (bytes.Length == 2)
                {
                    value[i] = GetGetSpellCharAt(bytes);
                }
                else
                {
                    value[i] = c;
                }
            }

            return new String(value);
        }

        /// <summary>
        /// 获取汉字拼音首字母
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static char GetGetSpellCharAt(byte[] bytes)
        {
            int value1 = (short)bytes[0];
            int value2 = (short)bytes[1];
            int value = value1 * 256 + value2;
            
            if(value >= 45217 && value <= 45252)
            {
                return 'A';
            }
            else if(value >= 45253 && value <= 45760)
            {
                return 'B';
            }
            else if(value >= 45761 && value <= 46317)
            {
                return 'C';
            }
            else if(value >= 46318 && value <= 46825)
            {
                return 'D';
            }
            else if(value >= 46826 && value <= 47009)
            {
                return 'E';
            }
            else if(value >= 47010 && value <= 47296)
            {
                return 'F';
            }
            else if(value >= 47297 && value <= 47613)
            {
                return 'G';
            }
            else if(value >= 47614 && value <= 48118)
            {
                return 'H';
            }
            else if(value >= 48119 && value <= 49061)
            {
                return 'J';
            }
            else if (value >= 49062 && value <= 49323)
            {
                return 'K';
            }
            else if (value >= 49324 && value <= 49895)
            {
                return 'L';
            }
            else if (value >= 49896 && value <= 50370)
            {
                return 'M';
            }
            else if (value >= 50371 && value <= 50613)
            {
                return 'N';
            }
            else if (value >= 50614 && value <= 50621)
            {
                return 'O';
            }
            else if (value >= 50622 && value <= 50905)
            {
                return 'P';
            }
            else if (value >= 50906 && value <= 51386)
            {
                return 'Q';
            }
            else if (value >= 51387 && value <= 51445)
            {
                return 'R';
            }
            else if (value >= 51446 && value <= 52217)
            {
                return 'S';
            }
            else if (value >= 52218 && value <= 52697)
            {
                return 'T';
            }
            else if (value >= 52698 && value <= 52979)
            {
                return 'W';
            }
            else if (value >= 52980 && value <= 53640)
            {
                return 'X';
            }
            else if (value >= 53689 && value <= 54480)
            {
                return 'Y';
            }
            else if (value >= 54481 && value <= 55289)
            {
                return 'Z';
            }
            else
            {
                return '?';
            }
        }

        /// <summary>
        /// 转换成简体中文
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>简体中文字符串</returns>
        public static string ToSimplifiedChinese(this string input)
        {
            if(String.IsNullOrEmpty(input))
            {
                return input;
            }

            return null;
        }

        /// <summary>
        /// 转换成繁体中文
        /// </summary>
        /// <param name="input">任意字符串</param>
        /// <returns>繁体中文字符串</returns>
        public static string ToTraditionalChinese(this string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return input;
            }

            return null;
        }
    }
}
