using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoonFramework.Data.SQLServer
{
    /// <summary>
    /// SQL Server 数据库对应的DatabaseAccess处理类
    /// 用于提供一些SQL Server特有的处理方式
    /// </summary>
    public class SqlDatabaseAccessOld : SqlDatabaseAccess
    {
        static readonly PagingCommandTextCache pagingTextCache = new PagingCommandTextCache();

        public SqlDatabaseAccessOld(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        public SqlDatabaseAccessOld(DbProviderFactory providerFactory = null)
            : base(providerFactory)
        {
        }

        #region Paging

        /// <summary>
        /// 构建分页查询其它页的DbCommand对象
        /// </summary>
        /// <param name="command">DBCommand对象</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页多少行记录</param>
        protected override void PageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            string commandText = pagingTextCache.GetPagingCommandText(command);

            if (commandText == null)
            {
                commandText = command.CommandText;
                int index = commandText.LastIndexOf(ORDER_BY, StringComparison.CurrentCultureIgnoreCase);
                string orderClause = String.Empty;

                int start = commandText.IndexOf(SELECT, StringComparison.CurrentCultureIgnoreCase) + SELECT.Length;
                int end = commandText.IndexOf(FROM, StringComparison.CurrentCultureIgnoreCase) - start - 1;
                string selectColumns = commandText.Substring(start, end).Trim();

                if (index > -1)
                {
                    orderClause = commandText.Substring(index);
                    commandText = commandText.Substring(0, index);
                }
                else
                {
                    orderClause = "ORDER BY " + TryFormatOrderColumn(selectColumns);
                }

                Regex regex = new Regex(@"\s\bFROM\b\s", RegexOptions.IgnoreCase);
                StringBuilder rownumString = new StringBuilder(",ROW_NUMBER() OVER(").Append(orderClause)
                    .Append(") AS [Paging_RowNumber] FROM ");
                StringBuilder pagingString = new StringBuilder("SELECT ")
                    .Append("PageResultSet.*")
                    .Append(" FROM (").Append(regex.Replace(commandText, rownumString.ToString(), 1))
                    .Append(") PageResultSet WHERE [Paging_RowNumber] >= ");
                commandText = pagingString.ToString();

                pagingTextCache.SetPagingCommandText(command, commandText);
            }

            //添加分页数据
            int rowStart = RowStart(pageIndex, pageSize);
            int rowEnd = RowEnd(pageIndex, pageSize);
            commandText = String.Concat(commandText, rowStart, " AND [Paging_RowNumber] <= ", rowEnd);

            command.CommandText = commandText;
        }

        #endregion

        #region Helpers

        private string TryFormatOrderColumn(string selectColumns)
        {
            string all = "*";
            string[] columnItems = selectColumns.Split(',');
            StringBuilder columns = new StringBuilder();
            int index = -1;
            string temp = null;
            for (int i = 0; i < columnItems.Length; i++)
            {
                temp = columnItems[i];
                if ((index = temp.IndexOf(AS_KEY, StringComparison.CurrentCultureIgnoreCase)) > -1)
                {
                    temp = temp.Substring(0, index);
                }
                else if ((index = temp.IndexOf(EQUAL_KEY)) > -1)
                {
                    temp = temp.Substring(0, index);
                }
                temp = temp.Trim();
                if (temp.IndexOf(all) > -1)
                {
                    throw new CommandTextFormatException("当前语句无法自动解析排序列，请手动编写ORDER BY字句");
                }
                columns.Append(temp);
                if (i < columnItems.Length - 1)
                    columns.Append(",");
            }
            return columns.ToString();
        }

        #endregion
    }
}
