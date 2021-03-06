﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data.MySQL
{
    /// <summary>
    /// MySQL数据库对应的DatabaseAccess处理类
    /// 用于提供一些MySQL特有的处理方式
    /// </summary>
    public class MySqlDatabaseAccess : DatabaseAccess
    {
        /// <summary>
        /// MySqlDatabaseAccess构造函数
        /// </summary>
        /// <param name="connection">MySqlConnection</param>
        /// <param name="providerFactory">MySqlProviderFactory</param>
        public MySqlDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        /// <summary>
        /// MySqlDatabaseAccess构造函数
        /// </summary>
        /// <param name="providerFactory">MySqlProviderFactory</param>
        public MySqlDatabaseAccess(DbProviderFactory providerFactory = null)
            : base(providerFactory)
        {
        }

        private const string ParameterNameFormat = "?";

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
            throw new NotSupportedException("MySQL not Supported");
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
            string commandText = command.CommandText;
            CheckOrderClause(commandText);

            int rowStart = RowStart(pageIndex, pageSize) - 1;
            StringBuilder pagingCommandText = new StringBuilder();
            pagingCommandText
                .Append("SELECT PAGE_RESULT_SET.* FROM (")
                .Append(commandText)
                .Append(") PAGE_RESULT_SET")
                .Append(" LIMIT ").Append(rowStart)
                .Append(",").Append(pageSize);

            command.CommandText = pagingCommandText.ToString();
        }

        #endregion
    }
}
