using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SoonFramework.Data
{
    /// <summary>
    /// 本地事务托管器
    /// </summary>
    public sealed class DbTransactionScope : IDisposable
    {
        /// <summary>
        /// 程序集内部使用的DatabaseAccess对象
        /// </summary>
        internal DatabaseAccess context;
        //数据库连接包装器
        private DbConnectionWrapper connectionWrapper;
        //本地事务对象
        private LocalTransaction localTransaction;

        /// <summary>
        /// 本地事务托管器构造，必须传入当前使用的DatabaseAccess对象
        /// </summary>
        /// <param name="dba"></param>
        public DbTransactionScope(DatabaseAccess dba)
        {
            context = dba;
        }

        /// <summary>
        /// 当前开启的本地事务
        /// </summary>
        internal LocalTransaction LocalTransaction
        {
            get
            {
                return localTransaction;
            }
        }

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns></returns>
        internal LocalTransaction BeginTransaction()
        {
            connectionWrapper = context.GetOpenConnection(CreateDbConnectionWrapper);
            if(connectionWrapper == null)
            {
                throw new DbTransactionException("打开事务失败，DatabaseAccess内部方法正在使用数据库连接，必须等到连接释放才能开启事务。");
            }
            DbTransaction transaction = connectionWrapper.Connection.BeginTransaction();
            DbTransactionWrapper wrapper = new DbTransactionWrapper(connectionWrapper.Connection, transaction);
            wrapper.Disposing += wrapper_Disposing;
            localTransaction = wrapper;
            return wrapper;
        }

        /// <summary>
        /// 开启指定事务等级的事务
        /// IsolationLevel枚举用于控制当前的事务等级
        /// Chaos：无法覆盖隔离级别更高的事务中的挂起的更改。
        /// ReadCommitted：在正在读取数据时保持共享锁，以避免脏读，但是在事务结束之前可以更改数据，从而导致不可重复的读取或幻像数据。
        /// ReadUncommitted：可以进行脏读，意思是说，不发布共享锁，也不接受独占锁。
        /// RepeatableRead：在查询中使用的所有数据上放置锁，以防止其他用户更新这些数据。 防止不可重复的读取，但是仍可以有幻像行。 
        /// Serializable：在 DataSet 上放置范围锁，以防止在事务完成之前由其他用户更新行或向数据集中插入行。
        /// Snapshot：通过在一个应用程序正在修改数据时存储另一个应用程序可以读取的相同数据版本来减少阻止。 表示您无法从一个事务中看到在其他事务中进行的更改，即便重新查询也是如此。
        /// Unspecified：正在使用与指定隔离级别不同的隔离级别，但是无法确定该级别。当使用 OdbcTransaction 时，如果不设置 IsolationLevel 或将 IsolationLevel 设置为 Unspecified，则事务将根据由所使用的驱动程序所决定的隔离级别来执行。
        /// </summary>
        /// <param name="level">事务等级</param>
        /// <returns></returns>
        internal LocalTransaction BeginTransaction(IsolationLevel level)
        {
            connectionWrapper = context.GetOpenConnection(CreateDbConnectionWrapper);
            if (connectionWrapper == null)
            {
                throw new DbTransactionException("打开事务失败，DatabaseAccess内部方法正在使用数据库连接，必须等到连接释放才能开启事务。");
            }
            DbTransaction transaction = connectionWrapper.Connection.BeginTransaction(level);
            DbTransactionWrapper wrapper = new DbTransactionWrapper(connectionWrapper.Connection, transaction);
            wrapper.Disposing += wrapper_Disposing;
            localTransaction = wrapper;
            return wrapper;
        }

        void wrapper_Disposing(object sender, EventArgs e)
        {
            connectionWrapper.Dispose();
        }

        TransactionConnectionWrapper CreateDbConnectionWrapper(DbConnection connection)
        {
            return new TransactionConnectionWrapper(context, connection);
        }

        /// <summary>
        /// 将当前事务绑定到DbCommand对象上
        /// </summary>
        /// <param name="command"></param>
        public void SetTransaction(DbCommand command)
        {
            if(LocalTransaction != null)
            {
                command.Transaction = LocalTransaction.Transaction;
            }
        }

        /// <summary>
        /// 完成事务操作
        /// </summary>
        public void Complete()
        {
            if(localTransaction != null)
            {
                localTransaction.Complete();
            }
        }

        /// <summary>
        /// 释放事务所占用的资源
        /// </summary>
        public void Dispose()
        {
            if(localTransaction != null)
            {
                localTransaction.Dispose();
            }
            localTransaction = null;
            context.LocalTransactionScope = null;
        }
    }
}
