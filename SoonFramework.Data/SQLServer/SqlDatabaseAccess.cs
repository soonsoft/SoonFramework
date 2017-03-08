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
    public class SqlDatabaseAccess : DatabaseAccess
    {
        public SqlDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        public SqlDatabaseAccess(DbProviderFactory providerFactory = null)
            : base(providerFactory)
        {
        }

        private const string ParameterNameFormat = "@";

        public override string ParameterPrefix
        {
            get
            {
                return ParameterNameFormat;
            }
        }

        #region DataSet

        /// <devdoc>
        /// Listens for the RowUpdate event on a dataadapter to support UpdateBehavior.Continue
        /// </devdoc>
        private void OnSqlRowUpdated(object sender, SqlRowUpdatedEventArgs rowThatCouldNotBeWritten)
        {
            if (rowThatCouldNotBeWritten.RecordsAffected == 0)
            {
                if (rowThatCouldNotBeWritten.Errors != null)
                {
                    rowThatCouldNotBeWritten.Row.RowError = rowThatCouldNotBeWritten.Errors.Message;
                    rowThatCouldNotBeWritten.Status = UpdateStatus.SkipCurrentRow;
                }
            }
        }

        /// <summary>
        /// 为DbDataAdapter设置DateSet行更新后的处理事件
        /// </summary>
        /// <param name="adapter"></param>
        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            ((SqlDataAdapter)adapter).RowUpdated += OnSqlRowUpdated;
        }

        #endregion

        #region Paging

        /// <summary>
        /// 获取分页查询第一页的DbCommand对象
        /// </summary>
        /// <param name="command">DBCommand对象</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页多少行记录</param>
        protected override void FirstPageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            string commandText = command.CommandText;

            int rowEnd = RowEnd(pageIndex, pageSize);
            string topClause = "SELECT TOP(" + rowEnd + ") ";

            Regex regex = new Regex(@"\bSELECT\b\s", RegexOptions.IgnoreCase);
            command.CommandText = regex.Replace(commandText, topClause, 1);
        }

        /// <summary>
        /// 构建分页查询其它页的DbCommand对象
        /// </summary>
        /// <param name="command">DBCommand对象</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页多少行记录</param>
        protected override void PageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            string commandText = command.CommandText;
            CheckOrderClause(commandText);

            int offset = RowStart(pageIndex, pageSize) - 1;
            StringBuilder pagingCommandText = new StringBuilder();
            pagingCommandText
                .Append(commandText.Trim())
                .Append(" OFFSET ").Append(offset)
                .Append(" ROW FETCH NEXT ").Append(pageSize)
                .Append(" ROWS ONLY");
            command.CommandText = pagingCommandText.ToString();
        }

        #endregion
    }
}
