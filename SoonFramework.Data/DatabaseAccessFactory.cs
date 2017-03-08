using SoonFramework.Core;
using SoonFramework.Data.MySQL;
using SoonFramework.Data.OleDb;
using SoonFramework.Data.Oracle;
using SoonFramework.Data.PostgreSQL;
using SoonFramework.Data.SQLite;
using SoonFramework.Data.SQLServer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// DatabaseAccess创建工厂
    /// </summary>
    public class DatabaseAccessFactory
    {
        /// <summary>
        /// 默认为SQL Server
        /// </summary>
        const string DefaultProvider = "System.Data.SqlClient";
        readonly static Dictionary<string, DatabaseAccessCreator> CreatorDictionary = null;

        static DatabaseAccessFactory()
        {
            CreatorDictionary = new Dictionary<string, DatabaseAccessCreator>(10);
            InitialCreatorDictionary();
        }

        static void InitialCreatorDictionary()
        {
            //SQL Server
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "SQLServer",
                DbConnectionProviderClassFullName = "System.Data.SqlClient.SqlConnection",
                DatabaseAccessConstructor = (c, p) => new SqlDatabaseAccess(c, p)
            });
            //Oracle
            /*
             * Oracle实现库比较多
             * Oracle Managed Driver是全托管的Oracle客户端，推荐使用，不用依赖oci
             * Oracle ODP.NET Oracle官方出的客户端，依赖oci，Oracle.DataAccess.Client.OracleConnection
             * Oracle OracleClient Microsoft出的客户端，依赖oci，.Net 4.0以后不再支持，System.Data.OracleClient.OracleConnection
             */
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "Oracle",
                DbConnectionProviderClassFullName = "Oracle.ManagedDataAccess.Client.OracleConnection",
                DatabaseAccessConstructor = (c, p) => new OracleDatabaseAccess(c, p)
            });
            //OleDB 可用于查询Excel或者连接Access数据库
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "OleDb",
                DbConnectionProviderClassFullName = "System.Data.OleDb.OleDbConnection",
                DatabaseAccessConstructor = (c, p) => new OleDatabaseAccess(c, p)
            });
            //MySQL
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "MySQL",
                DbConnectionProviderClassFullName = "MySql.Data.MySqlClient.MySqlConnection",
                DatabaseAccessConstructor = (c, p) => new MySqlDatabaseAccess(c, p)
            });
            //PostgreSQL
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "PostgreSQL",
                DbConnectionProviderClassFullName = "Npgsql.NpgsqlConnection",
                DatabaseAccessConstructor = (c, p) => new PostgreSQLDatabaseAccess(c, p)
            });
            //SQLite
            SetDatabaseAccessCreator(new DatabaseAccessCreator
            {
                Name = "SQLite",
                DbConnectionProviderClassFullName = "System.Data.SQLite.SQLiteConnection",
                DatabaseAccessConstructor = (c, p) => new SQLiteDatabaseAccess(c, p)
            });
        }

        /// <summary>
        /// 创建DatabaseAccess
        /// </summary>
        /// <returns>DatabaseAccess实例</returns>
        public static DatabaseAccess CreateDatabase()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[0];
            if (settings == null)
            {
                throw new Exception("can't found ConnectionString");
            }
            return CreateDatabase(settings);
        }

        /// <summary>
        /// 根据指定的数据库连接名称创建DatabaseAccess
        /// </summary>
        /// <param name="connectionStringName">数据库连接名称</param>
        /// <returns>DatabaseAccess实例</returns>
        public static DatabaseAccess CreateDatabase(string connectionStringName)
        {
            if (connectionStringName == null)
            {
                throw new ArgumentNullException("connectionStringName");
            }
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[connectionStringName];
            if (settings == null)
            {
                throw new ArgumentException(String.Format("can't found connectionString section by \"{0}\"", connectionStringName));
            }
            return CreateDatabase(settings);
        }

        /// <summary>
        /// 根据数据库连接对象创建DatabaseAccess
        /// </summary>
        /// <param name="connection">数据库连接对象</param>
        /// <returns>DatabaseAccess实例</returns>
        internal static DatabaseAccess CreateDatabase(DbConnection connection)
        {
            return CreateDatabaseAccess(connection);
        }

        /// <summary>
        /// 根据数据库连接配置创建DatabaseAccess
        /// </summary>
        /// <param name="settings">数据库连接配置</param>
        /// <returns>DatabaseAccess实例</returns>
        static DatabaseAccess CreateDatabase(ConnectionStringSettings settings)
        {
            string providerName = settings.ProviderName;
            if (String.IsNullOrEmpty(providerName))
            {
                providerName = DefaultProvider;
            }
            DbProviderFactory providerFactory = DbProviderFactories.GetFactory(providerName);
            DbConnection connection = providerFactory.CreateConnection();
            connection.ConnectionString = settings.ConnectionString;
            return CreateDatabaseAccess(connection, providerFactory);
        }

        /// <summary>
        /// 设置DatabaseAccess构造器
        /// </summary>
        /// <param name="creator"></param>
        public static void SetDatabaseAccessCreator(DatabaseAccessCreator creator)
        {
            Guard.ArgumentNotNull(creator, "creator");
            Guard.ArgumentNotNullOrEmpty(creator.DbConnectionProviderClassFullName, "creator.DbConnectionProviderClassFullName");

            CreatorDictionary[creator.DbConnectionProviderClassFullName] = creator;
        }

        /// <summary>
        /// 根据数据库连接和数据库工厂创建DatabaseAccess
        /// </summary>
        /// <param name="connection">数据库连接</param>
        /// <param name="providerFactory">数据库工厂</param>
        /// <returns>DatabaseAccess实例</returns>
        public static DatabaseAccess CreateDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
        {
            Guard.ArgumentNotNull(connection, "connection");

            string connectionClassName = connection.GetType().FullName;
            if(!CreatorDictionary.ContainsKey(connectionClassName))
            {
                throw new NotSupportedException(
                    String.Format("框架暂时不支持创建数据库连接为{0}的DatabaseAccess实现类", connectionClassName));
            }
            return CreatorDictionary[connectionClassName].DatabaseAccessConstructor(connection, providerFactory);
        }

        #region 判断数据库类型

        public static bool IsDatabaseAccess<TDatabaseAccess>(DatabaseAccess databaseAccess)
        {
            if(databaseAccess == null)
            {
                return false;
            }
            return databaseAccess is TDatabaseAccess;
        }

        public static bool IsSQLServer(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<SqlDatabaseAccess>(databaseAccess);
        }

        public static bool IsOracle(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<OracleDatabaseAccess>(databaseAccess);
        }

        public static bool IsMySQL(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<MySqlDatabaseAccess>(databaseAccess);
        }

        public static bool IsPostgreSQL(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<PostgreSQLDatabaseAccess>(databaseAccess);
        }

        public static bool IsSQLite(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<SQLiteDatabaseAccess>(databaseAccess);
        }

        public static bool IsOleDb(DatabaseAccess databaseAccess)
        {
            return IsDatabaseAccess<OleDatabaseAccess>(databaseAccess);
        }

        #endregion
    }
}
