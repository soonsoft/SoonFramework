using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace SoonFramework.Data
{
    /// <summary>
    /// 事务管理器默认实现
    /// </summary>
    public sealed class TransactionManager : ITransactionManager
    {
        private bool isClosed = false;
        private bool isCommit = false;
        private static object asyncLocker = new Object();

        private DbConnection connection;
        /// <summary>
        /// 获取事务相关的数据库连接对象
        /// </summary>
        public DbConnection Connection 
        {
            get
            {
                return connection;
            }
        }

        private DbTransaction transaction;
        /// <summary>
        /// 获取DbTransaction对象
        /// </summary>
        public DbTransaction Transaction 
        {
            get
            {
                return transaction;
            }
        }

        public TransactionManager(DbConnection connection)
        {
            this.connection = connection;
        }

        ~TransactionManager()
        {
            Close(false);
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        internal void BeginTransaction()
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                transaction = connection.BeginTransaction();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 开始事务
        /// </summary>
        /// <param name="isolationLevel">以指定的隔离级别启动数据库事务</param>
        internal void BeginTransaction(System.Data.IsolationLevel isolationLevel)
        {
            try
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                transaction = connection.BeginTransaction(isolationLevel);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void AddCommand(DbCommand command)
        {
            command.Transaction = Transaction;
        }
        
        /// <summary>
        /// 提交事务
        /// </summary>
        public void Commit()
        {
            try
            {
                transaction.Commit();
                isCommit = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                Close(true);
            }
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public void Rollback()
        {
            try
            {
                transaction.Rollback();
            }
            catch
            {
                throw;
            }
            finally
            {
                Close(true);
            }
        }

        void Close(bool closing)
        {
            if (!isClosed)
            {
                //if(closing)
                    //处理托管资源
                try
                {
                    if (connection != null && connection.State != ConnectionState.Closed)
                        connection.Close();
                    connection = null;
                }
                catch { }
                try
                {
                    if (transaction != null)
                        transaction.Dispose();
                    transaction = null;
                }
                catch { }
            }
            isClosed = true;
        }

        public void Dispose()
        {
            if (!isClosed)
            {
                lock (asyncLocker)
                {
                    if (!isCommit)
                        Rollback();
                }
            }
            GC.SuppressFinalize(this);
        }
    }
}
