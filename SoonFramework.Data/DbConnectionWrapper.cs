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
    /// 数据库连接包装器
    /// </summary>
    public class DbConnectionWrapper : IDisposable
    {
        private DbConnection connection = null;
        private bool isDisposed = false;

        /// <summary>
        /// 带参构造
        /// </summary>
        /// <param name="conn">DbConnection数据库连接实例</param>
        public DbConnectionWrapper(DbConnection conn)
        {
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }
            this.connection = conn;
        }

        /// <summary>
        /// 当前数据库连接是否为开启状态
        /// </summary>
        public bool IsOpened { get; private set; }

        /// <summary>
        /// 包含的DbConnection对象的State状态
        /// </summary>
        public ConnectionState State
        {
            get
            {
                return connection.State;
            }
        }

        /// <summary>
        /// 返回包含的DbConnection实例
        /// </summary>
        internal DbConnection Connection
        {
            get
            {
                return connection;
            }
        }

        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public virtual void Open()
        {
            if (isDisposed)
            {
                throw new DataException("DbConnection is Disposed!");
            }
            if (connection.State == ConnectionState.Broken)
            {
                connection.Close();
            }
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            IsOpened = true;
        }

        /// <summary>
        /// 异步打开数据库连接
        /// </summary>
        public virtual async Task OpenAsync()
        {
            if (isDisposed)
            {
                throw new DataException("DbConnection is Disposed!");
            }
            if (connection.State == ConnectionState.Broken)
            {
                connection.Close();
            }
            if (connection.State != ConnectionState.Open)
            {
                await connection.OpenAsync();
            }
            IsOpened = true;
        }

        /// <summary>
        /// 关闭数据库连接
        /// </summary>
        public virtual void Close()
        {
            if (isDisposed)
            {
                throw new DataException("DbConnection is Disposed!");
            }
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
            IsOpened = false;
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (isDisposed) return;

            try
            {
                Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                isDisposed = true;
            }
        }
    }
}
