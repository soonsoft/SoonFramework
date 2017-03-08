using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 本地事务
    /// </summary>
    public abstract class LocalTransaction : IDisposable
    {
        //事务是否操作完成
        protected bool isComplete = false;
        //事务资源是否已经释放
        private bool isDisposed = false;

        /// <summary>
        /// 事务资源释放时事件，可以在本地事务释放中释放其它的资源如数据库连接
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// 和当前事务绑定的数据库连接
        /// </summary>
        public DbConnection Connection { get; protected set; }
        /// <summary>
        /// 当前开启的本地数据库事务
        /// </summary>
        public DbTransaction Transaction { get; protected set; }

        /// <summary>
        /// 该事务操作是否已经完成
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return isComplete;
            }
        }

        /// <summary>
        /// 完成事务操作
        /// </summary>
        public void Complete()
        {
            isComplete = true;
        }

        /// <summary>
        /// 处理释放资源事件
        /// </summary>
        void OnDisposing()
        {
            if(Disposing != null)
            {
                Disposing(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if(isDisposed)
            {
                return;
            }
            try
            {
                if (IsComplete)
                {
                    Transaction.Commit();
                }
                else
                {
                    Transaction.Rollback();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    OnDisposing();
                }
                catch { }
                try
                { 
                    Transaction.Dispose();
                }
                catch { }
                isDisposed = true;
            }
        }
    }
}
