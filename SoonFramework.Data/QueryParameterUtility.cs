using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 查询参数工具
    /// </summary>
    public class QueryParameterUtility
    {
        #region Datetime Parameter

        /// <summary>
        /// 设置用于查询的开始时间和结束时间
        /// </summary>
        /// <param name="start">开始时间，会被修正为00:00:00</param>
        /// <param name="end">结束时间，会被修正为23:59:59</param>
        public static void DateTimeStartAndEnd(ref DateTime start, ref DateTime end)
        {
            //00:00:00
            start = DateTimeStart(start);
            //23:59:59
            end = DateTimeEnd(end);
        }

        /// <summary>
        /// 设置用于查询的开始时间
        /// </summary>
        /// <param name="start">开始时间，会被修正为00:00:00</param>
        /// <returns>开始时间</returns>
        public static DateTime DateTimeStart(DateTime start)
        {
            //00:00:00
            return new DateTime(start.Year, start.Month, start.Day, 0, 0, 0, 0);
        }

        /// <summary>
        /// 设置用于查询的结束时间
        /// </summary>
        /// <param name="end">结束时间，会被修正为23:59:59</param>
        /// <returns>结束时间</returns>
        public static DateTime DateTimeEnd(DateTime end)
        {
            //23:59:59
            return new DateTime(end.Year, end.Month, end.Day, 23, 59, 59, 999);
        }

        #endregion

        #region parameter object to object[]

        /// <summary>
        /// 将一维参数转换为二维参数，用于多条语句执行
        /// Guid[] guidArr = new Guid[10] { ... };
        /// IList<object[]> parameters = QueryParameterUtility.TransferMultipleParameters<Guid>(guidArr);
        /// </summary>
        /// <typeparam name="T">一维参数的数据类型</typeparam>
        /// <param name="parameters">一维参数列表</param>
        /// <returns>二维参数</returns>
        public static IList<object[]> TransferMultipleParameters<T>(IEnumerable<T> parameters)
        {
            if (parameters == null)
                return null;
            IList<object[]> transParams = new List<object[]>(parameters.Count());
            foreach (T item in parameters)
            {
                transParams.Add(new object[] { item });
            }
            return transParams;
        }

        /// <summary>
        /// 将一维参数转换为二维参数，用于多条语句执行。
        /// 在转换过程中还可以添加其他参数，往往用于创建一对多关系数据
        /// </summary>
        /// <typeparam name="T">一维参数的数据类型</typeparam>
        /// <param name="parameters">一维参数列表</param>
        /// <param name="otherParameter">其它参数</param>
        /// <returns>二维参数</returns>
        public static IList<object[]> TransferMultipleParameters<T>(IEnumerable<T> parameters, 
            object otherParameter)
        {
            return TransferMultipleParameters<T>(parameters, 
                new object[] { otherParameter });
        }

        /// <summary>
        /// 将一维参数转换为二维参数，用于多条语句执行。
        /// 在转换过程中还可以添加其他参数，往往用于创建一对多关系数据
        /// </summary>
        /// <typeparam name="T">一维参数的数据类型</typeparam>
        /// <param name="parameters">一维参数列表</param>
        /// <param name="otherParameter1">其它参数1</param>
        /// <param name="otherParameter2">其它参数2</param>
        /// <returns>二维参数</returns>
        public static IList<object[]> TransferMultipleParameters<T>(IEnumerable<T> parameters, 
            object otherParameter1, object otherParameter2)
        {
            return TransferMultipleParameters<T>(parameters, 
                new object[] { otherParameter1, otherParameter2 });
        }

        /// <summary>
        /// 将一维参数转换为二维参数，用于多条语句执行。
        /// 在转换过程中还可以添加其他参数，往往用于创建一对多关系数据
        /// </summary>
        /// <typeparam name="T">一维参数的数据类型</typeparam>
        /// <param name="parameters">一维参数列表</param>
        /// <param name="otherParameter1">其它参数1</param>
        /// <param name="otherParameter2">其它参数2</param>
        /// <param name="otherParameter3">其它参数3</param>
        /// <returns>二维参数</returns>
        public static IList<object[]> TransferMultipleParameters<T>(IEnumerable<T> parameters, 
            object otherParameter1, object otherParameter2, object otherParameter3)
        {
            return TransferMultipleParameters<T>(parameters, 
                new object[] { otherParameter1, otherParameter2, otherParameter3 });
        }

        /// <summary>
        /// 将一维参数转换为二维参数，用于多条语句执行。
        /// 在转换过程中还可以添加其他参数，往往用于创建一对多关系数据
        /// </summary>
        /// <typeparam name="T">一维参数的数据类型</typeparam>
        /// <param name="parameters">一维参数列表</param>
        /// <param name="otherParameters">多个其它参数</param>
        /// <returns>二维参数</returns>
        public static IList<object[]> TransferMultipleParameters<T>(IEnumerable<T> parameters, object[] otherParameters)
        {
            if (parameters == null)
                return null;
            int length = 1;
            if (otherParameters != null)
            {
                length += otherParameters.Length;
            }
            IList<object[]> transParams = new List<object[]>(parameters.Count());
            object[] paramArr = null;
            int i = 0;
            foreach (T item in parameters)
            {
                paramArr = new object[length];
                paramArr[0] = item;
                for (i = 1; i < length; i++)
                {
                    paramArr[i] = otherParameters[i - 1];
                }
                transParams.Add(paramArr);
            }
            return transParams;
        }

        #endregion
    }
}
