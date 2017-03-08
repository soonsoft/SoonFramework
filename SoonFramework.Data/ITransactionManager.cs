using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace SoonFramework.Data
{
    /// <summary>
    /// 数据库事务管理器
    /// </summary>
    public interface ITransactionManager : IDisposable
    {
        /// <summary>
        /// 当前数据库连接
        /// </summary>
        DbConnection Connection { get; }

        /// <summary>
        /// 当前数据库事务
        /// </summary>
        DbTransaction Transaction { get; }

        /// <summary>
        /// 为DbCommand对象添加相应的事务
        /// </summary>
        /// <param name="command"></param>
        void AddCommand(DbCommand command);

        /// <summary>
        /// 提交当前事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚当前事务
        /// </summary>
        void Rollback();
    }
}
