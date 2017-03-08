using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 缓存策略，用户缓存DbParameter和查询字符串
    /// </summary>
    internal class CachingMechanism
    {
        private Hashtable cache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        /// 复制DataParameter对象
        /// </summary>
        /// <param name="originalParameters"></param>
        /// <returns></returns>
        public static IDataParameter[] CloneParameters(IDataParameter[] originalParameters)
        {
            IDataParameter[] clonedParameters = new IDataParameter[originalParameters.Length];

            for (int i = 0, j = originalParameters.Length; i < j; i++)
            {
                clonedParameters[i] = (IDataParameter)((ICloneable)originalParameters[i]).Clone();
            }

            return clonedParameters;
        }

        /// <summary>
        /// 清除所有缓存
        /// </summary>
        public void Clear()
        {
            this.cache.Clear();
        }

        /// <summary>
        /// 对象添加缓存
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <param name="set">对象</param>
        public void AddSetToCache<T>(string connectionString, IDbCommand command, T set)
            where T : class
        {
            string storedProcedure = command.CommandText;
            string key = CreateHashKey(connectionString, storedProcedure);
            this.cache[key] = set;
        }

        /// <summary>
        /// 从缓存中获取对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <returns>对象</returns>
        public T GetCachedSet<T>(string connectionString, IDbCommand command)
            where T : class
        {
            string commandText = command.CommandText;
            string key = CreateHashKey(connectionString, commandText);
            return (T)(this.cache[key]);
        }

        /// <summary>
        /// 将参数列表添加到缓存
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数列表</param>
        public void AddParameterSetToCache(string connectionString, IDbCommand command, IDataParameter[] parameters)
        {
            string commandText = command.CommandText;
            string key = CreateHashKey(connectionString, commandText);
            this.cache[key] = parameters;
        }

        /// <summary>
        /// 从缓存中获取参数列表
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <returns></returns>
        public IDataParameter[] GetCachedParameterSet(string connectionString, IDbCommand command)
        {
            string storedProcedure = command.CommandText;
            string key = CreateHashKey(connectionString, storedProcedure);
            IDataParameter[] cachedParameters = (IDataParameter[])(this.cache[key]);
            return CloneParameters(cachedParameters);
        }

        /// <summary>
        /// 是否已经有缓存
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <returns>bool</returns>
        public bool IsSetCached(string connectionString, IDbCommand command)
        {
            string hashKey = CreateHashKey(
                connectionString,
                command.CommandText);
            return this.cache[hashKey] != null;
        }

        /// <summary>
        /// 创建一个缓存键
        /// </summary>
        /// <param name="connectionString">数据库连接字符串</param>
        /// <param name="command">DbCommand</param>
        /// <returns>Key</returns>
        private static string CreateHashKey(string connectionString, string commandText)
        {
            return String.Concat(connectionString, ":", commandText);
        }
    }
}
