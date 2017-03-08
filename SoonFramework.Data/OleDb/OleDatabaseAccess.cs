using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoonFramework.Data.OleDb
{
    /// <summary>
    /// Oracle数据库对应的DatabaseAccess处理类
    /// 用于提供一些Oracle特有的处理方式
    /// </summary>
    public class OleDatabaseAccess : DatabaseAccess
    {
        static readonly PagingCommandTextCache pagingTextCache = new PagingCommandTextCache();

        public OleDatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : base(connection, providerFactory)
        {
        }

        public OleDatabaseAccess(DbProviderFactory providerFactory = null)
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

        /// <summary>
        /// Sets the RowUpdated event for the data adapter.
        /// </summary>
        /// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
        /// <remarks>The <see cref="DbDataAdapter"/> must be an <see cref="OracleDataAdapter"/>.</remarks>
        protected override void SetUpRowUpdatedEvent(DbDataAdapter adapter)
        {
            throw new NotSupportedException("OleDb not Supported");
        }

        #endregion

        #region Paging

        protected override void FirstPageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            throw new NotSupportedException();
        }

        protected override void PageCommand(DbCommand command, int pageIndex, int pageSize)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
