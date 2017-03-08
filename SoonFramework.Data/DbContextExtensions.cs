using SoonFramework.Data.Oracle;
using SoonFramework.Data.SQLServer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 用于EntityFramework的扩展方法
    /// </summary>
    public static class DbContextExtensions
    {
        /// <summary>
        /// 用于判断当前的数据库连接是否为预期的数据库连接
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="connType">预期数据库连接类型</param>
        /// <returns>bool</returns>
        private static bool IsDatabase(DbConnection connection, string connType)
        {
            return connection.ToString() == connType;
        }

        /// <summary>
        /// 通过DbContext创建DatabaseAccess
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <returns>DatabaseAccess实例</returns>
        public static DatabaseAccess DatabaseAccess(this DbContext context)
        {
            DbConnection connection = context.Database.Connection;
            return DatabaseAccessFactory.CreateDatabase(connection);
        }

        /// <summary>
        /// 获取Linq查询时分页的起始行号，用于Skip()方法的参数
        /// </summary>
        /// <param name="context">DbContext</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页的页码</param>
        /// <returns>起始行号</returns>
        public static int RowStartLinq(this DbContext context, int pageIndex, int pageSize)
        {
            return (pageIndex - 1) * pageSize;
        }
    }
}
