using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 用于事务的数据库连接包装器
    /// </summary>
    public class TransactionConnectionWrapper : DbConnectionWrapper
    {
        private DatabaseAccess context = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="context">DatabaseAccess实例</param>
        /// <param name="conn">数据库连接</param>
        public TransactionConnectionWrapper(DatabaseAccess context, DbConnection conn)
            : base(conn)
        {
            this.context = context;
        }

        /// <summary>
        /// 关闭数据库连接，如果开始事务时数据库连接已经被打开则不会关闭
        /// </summary>
        public override void Close()
        {
            if(context != null)
            {
                context.ReleaseConnection();
            }
        }
    }
}
