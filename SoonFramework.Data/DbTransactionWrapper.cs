using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 本地数据库事务包装器
    /// </summary>
    public class DbTransactionWrapper : LocalTransaction
    {
        /// <summary>
        /// 本地数据库事务包装器构造器
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="transaction">数据库事务</param>
        public DbTransactionWrapper(DbConnection connection, DbTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }
    }
}
