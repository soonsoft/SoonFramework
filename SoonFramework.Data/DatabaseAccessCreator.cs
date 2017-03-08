using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// DatabaseAccess工厂类构造器
    /// </summary>
    public class DatabaseAccessCreator
    {
        /// <summary>
        /// 支持的数据库名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 数据库客户端实现的DbConnection完整类名
        /// </summary>
        public string DbConnectionProviderClassFullName { get; set; }

        /// <summary>
        /// 对应数据库的DatabaseAccess类型的构造方法实现
        /// </summary>
        public Func<DbConnection, DbProviderFactory, DatabaseAccess> DatabaseAccessConstructor { get; set; }
    }
}
