using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 分页语句缓存
    /// </summary>
    internal class PagingCommandTextCache
    {
        //缓存器
        private CachingMechanism cache = new CachingMechanism();

        /// <summary>
        /// 设置分页语句缓存
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pagingCommandText">分页查询语句</param>
        public void SetPagingCommandText(DbCommand command, string pagingCommandText)
        {
            this.cache.AddSetToCache<string>(command.Connection.ConnectionString, command, pagingCommandText);
        }

        /// <summary>
        /// 缓存清除
        /// </summary>
        protected internal void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        /// 是否已经缓存
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>bool</returns>
        public bool AlreadyCached(DbCommand command)
        {
            return this.cache.IsSetCached(command.Connection.ConnectionString, command);
        }

        /// <summary>
        /// 获取分页语句缓存
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns></returns>
        public string GetPagingCommandText(DbCommand command)
        {
            return this.cache.GetCachedSet<string>(command.Connection.ConnectionString, command);
        }
    }
}
