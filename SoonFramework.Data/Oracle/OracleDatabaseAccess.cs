using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoonFramework.Data.Oracle
{
    /// <summary>
    /// Oracle数据库对应的DatabaseAccess处理类
    /// 用于提供一些Oracle特有的处理方式
    /// </summary>
    public class OracleDatabaseAccess : DatabaseAccess
    {
        static readonly PagingCommandTextCache pagingTextCache = new PagingCommandTextCache();

        /// <summary>
        /// OracleDatabaseAccess构造函数
        /// </summary>
        /// <param name="connection">OracleConnection</param>
        /// <param name="providerFactory">OracleProviderFactory</param>
        public OracleDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        /// <summary>
        /// OracleDatabaseAccess构造函数
        /// </summary>
        /// <param name="providerFactory">OracleProviderFactory</param>
        public OracleDatabaseAccess(DbProviderFactory providerFactory = null)
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

        /// <summary>
        /// Sets the RowUpdated event for the data adapter.
        /// </summary>
        /// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
        /// <remarks>The <see cref="DbDataAdapter"/> must be an <see cref="OracleDataAdapter"/>.</remarks>
        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            throw new NotSupportedException("Oracle not Supported");
            //((OracleDataAdapter)adapter).RowUpdated += OnOracleRowUpdated;
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
            string commandText = command.CommandText;

            int rowEnd = RowEnd(pageIndex, pageSize);
            StringBuilder pagingString = new StringBuilder("SELECT * FROM (")
                .Append(commandText)
                .Append(") PAGE_RESULT_SET WHERE ROWNUM <= ")
                .Append(rowEnd);
            command.CommandText = pagingString.ToString();
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

                int start = commandText.IndexOf(SELECT, StringComparison.CurrentCultureIgnoreCase) + SELECT.Length;
                int end = commandText.IndexOf(FROM, StringComparison.CurrentCultureIgnoreCase) - start - 1;
                string selectColumns = commandText.Substring(start, end).Trim();

                Regex regex = new Regex(@"\s\bFROM\b\s", RegexOptions.IgnoreCase);
                StringBuilder pagingString = new StringBuilder("SELECT ")
                    .Append("PAGE_RESULT_SET.*")
                    .Append(" FROM (")
                    .Append("SELECT PAGE_TEMP.*,ROWNUM AS PAGING_ROW_NUMBER FROM (")
                    .Append(commandText)
                    .Append(") PAGE_TEMP) PAGE_RESULT_SET")
                    .Append(" WHERE PAGING_ROW_NUMBER >= ");
                commandText = pagingString.ToString();

                pagingTextCache.SetPagingCommandText(command, commandText);
            }

            //添加分页信息
            int rowStart = RowStart(pageIndex, pageSize);
            int rowEnd = RowEnd(pageIndex, pageSize);
            commandText = String.Concat(commandText, rowStart, " AND PAGING_ROW_NUMBER <= ", rowEnd);

            command.CommandText = commandText;
        }

        #endregion
    }
}
