using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 参数缓存
    /// </summary>
    internal class ParameterCache
    {
        /// <summary>
        /// 从缓存中复制一份参数列表
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>缓存列表</returns>
        public static IDataParameter[] CreateParameterCopy(DbCommand command)
        {
            IDataParameterCollection parameters = command.Parameters;
            IDataParameter[] parameterArray = new IDataParameter[parameters.Count];
            parameters.CopyTo(parameterArray, 0);

            return CachingMechanism.CloneParameters(parameterArray);
        }
    }
}
