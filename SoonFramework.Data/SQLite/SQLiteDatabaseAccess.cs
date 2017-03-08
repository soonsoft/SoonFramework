using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data.SQLite
{
    /// <summary>
    /// SQLite数据库对应的DatabaseAccess处理类
    /// 用于提供一些SQLite特有的处理方式
    /// </summary>
    public class SQLiteDatabaseAccess : DatabaseAccess
    {
        static readonly PagingCommandTextCache pagingTextCache = new PagingCommandTextCache();
        /// <summary>
        /// PostgreSQLDatabaseAccess构造函数
        /// </summary>
        /// <param name="connection">PostgreSQLConnection</param>
        /// <param name="providerFactory">PostgreSQLProviderFactory</param>
        public SQLiteDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        /// <summary>
        /// PostgreSQLDatabaseAccess构造函数
        /// </summary>
        /// <param name="providerFactory">PostgreSQLProviderFactory</param>
        public SQLiteDatabaseAccess(DbProviderFactory providerFactory = null)
            : base(providerFactory)
        {
        }

        private const string ParameterNameFormat = ":";

        public override string ParameterPrefix
        {
            get
            {
                return ParameterNameFormat;
            }
        }

        #region DataSet

        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            throw new NotSupportedException("SQLite not Supported");
        }

        #endregion

        #region Paging

        /// <summary>
        /// 包装查询第一页的DbCommand
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        protected override void FirstPageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            PageCommand(command, pageIndex, pageSize);
        }

        /// <summary>
        /// 包装分页查询的DbCommand
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        protected override void PageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            string commandText = pagingTextCache.GetPagingCommandText(command);
            if (commandText == null)
            {
                commandText = command.CommandText;
                StringBuilder pagingString = new StringBuilder("SELECT PAGE_RESULT_SET.* FROM (");
                pagingString.Append(commandText)
                    .Append(") PAGE_RESULT_SET");
                commandText = pagingString.ToString();

                pagingTextCache.SetPagingCommandText(command, commandText);
            }

            //分页信息
            int rowStart = (pageIndex - 1) * pageSize;
            commandText = String.Concat(commandText, " LIMIT ", pageSize, " OFFSET ", rowStart);

            command.CommandText = commandText;
        }

        #endregion
    }
}
