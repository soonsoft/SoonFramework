using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Core
{
    public static class Guard
    {
        const string ExceptionStringEmpty = "argument {0} is empty";

        /// <summary>
        /// 如何参数值等于Null则抛出异常 <see cref="ArgumentNullException"/>
        /// </summary>
        /// <exception cref="ArgumentNullException">当参数值为Null</exception>
        /// <param name="argumentValue">检查的参数值</param>
        /// <param name="argumentName">检查的参数名称</param>
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null) throw new ArgumentNullException(argumentName);
        }

        /// <summary>
        /// 检查参数不是Null或者空
        /// </summary>
        /// <exception cref="ArgumentNullException">参数等于Null</exception>
        /// <exception cref="ArgumentException">参数等于空</exception>
        /// <param name="argumentValue">检查的参数值</param>
        /// <param name="argumentName">检查的参数名称</param>
        public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
        {
            if (argumentValue == null) throw new ArgumentNullException(argumentName);
            if (argumentValue.Length == 0) throw new ArgumentException(String.Format(ExceptionStringEmpty, argumentName));
        }

        /// <summary>
        /// 检查参数不是Null或者空
        /// </summary>
        /// <exception cref="ArgumentNullException">参数等于Null</exception>
        /// <exception cref="ArgumentException">参数等于空</exception>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="argumentValue">检查的参数值</param>
        /// <param name="argumentName">检查的参数名称</param>
        public static void ArgumentNotNullOrEmpty<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            if (argumentValue == null) throw new ArgumentNullException(argumentName);
            if (argumentValue.IsEmpty()) throw new ArgumentException(String.Format(ExceptionStringEmpty, argumentName));
        }
    }
}
