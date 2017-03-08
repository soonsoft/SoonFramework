using SoonFramework.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    public abstract partial class DatabaseAccess
    {
        #region DbConnection Manager Async

        /// <summary>
        /// 检查数据库连接的开启状态，如果数据库连接没有正常开启则打开，如果已经打开则不做任何处理
        /// </summary>
        internal virtual async Task EnsureConnectionAsync()
        {
            if (Connection.State == ConnectionState.Broken)
            {
                Connection.Close();
            }

            if (Connection.State == ConnectionState.Closed)
            {
                _openedConnection = true;
                try
                {
                    await Connection.OpenAsync();
                }
                catch
                {
                    _openedConnection = false;
                    throw;
                }
            }

            if (_openedConnection)
            {
                _connectionRequestCount++;
            }
        }

        /// <summary>
        /// 获取一个已经打开的数据库连接对象，
        /// 当需要和EntityFramework混合使用时避免数据库连接多次打开关闭
        /// <para>标准写法：</para>
        /// <code>
        /// using(var conn = await dba.GetOpenConnectionAsync()) 
        /// {
        ///     ....
        /// }
        /// </code>
        /// </summary>
        /// <returns>已经打开的数据库连接</returns>
        public virtual async Task<DbConnectionWrapper> GetOpenConnectionAsync()
        {
            if (!_openedConnection)
            {
                var connectionWrapper = new DbConnectionWrapper(Connection);
                await connectionWrapper.OpenAsync();
                return connectionWrapper;
            }
            else
            {
                return null;
            }
        }

        #endregion ///DbConnection Manager Async

        #region ExecuteNonQueryAsync

        /// <summary>
        /// 异步执行SQL 语句
        /// 用法 int val = await ExecuteNonQueryAsync(command);
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(DbCommand command)
        {
            bool hasLocalTransaction = false;
            try
            {
                Task ensureConnectionTask = EnsureConnectionAsync();
                hasLocalTransaction = CheckLocalTransaction(command);

                await ensureConnectionTask;
                int affectRows = await command.ExecuteNonQueryAsync();
                return affectRows;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (!hasLocalTransaction)
                    ReleaseConnection();
            }
        }

        /// <summary>
        /// 异步执行SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return await ExecuteNonQueryAsync(command);
        }

        /// <summary>
        /// 异步执行SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteNonQueryAsync(command);
        }

        /// <summary>
        /// 异步执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteNonQueryAsync(command);
            }
        }

        /// <summary>
        /// 异步执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteNonQueryAsync(command, parameters);
            }
        }

        /// <summary>
        /// 异步执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        public virtual async Task<int> ExecuteNonQueryAsync(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteNonQueryAsync(command, parameters);
            }
        }

        #endregion ///ExecuteNonQueryAsync

        #region ExecuteScalarAsync

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(DbCommand command)
        {
            try
            {
                Task ensureConnectionTask = EnsureConnectionAsync();
                CheckLocalTransaction(command);

                await ensureConnectionTask;
                return await command.ExecuteScalarAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                ReleaseConnection();
            }
        }

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return await ExecuteScalarAsync(command);
        }

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteScalarAsync(command);
        }

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteScalarAsync(command);
            }
        }

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteScalarAsync(command, parameters);
            }
        }

        /// <summary>
        /// 异步执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual async Task<object> ExecuteScalarAsync(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteScalarAsync(command, parameters);
            }
        }

        #endregion ///ExecuteScalarAsync

        #region ExecuteReaderAsync

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(DbCommand command)
        {
            Guard.ArgumentNotNull(command, "command");

            bool lastOpenedConnection = _openedConnection;
            bool isOpen = false;
            CommandBehavior behavior = CommandBehavior.Default;
            try
            {
                await EnsureConnectionAsync();
                //判断是否在方法中开启了连接，如果由方法内部开启则需要在关闭DataReader时关闭连接
                isOpen = lastOpenedConnection != _openedConnection;
                if (isOpen)
                {
                    behavior = CommandBehavior.CloseConnection;
                }
                CheckLocalTransaction(command);

                DbDataReader reader = await command.ExecuteReaderAsync(behavior);
                ReleaseConnection(false);
                return reader;
            }
            catch
            {
                if (isOpen)
                    ReleaseConnection();
                else
                    ReleaseConnection(false);
                throw;
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return await ExecuteReaderAsync(command);
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteReaderAsync(command);
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync(command);
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync(command, parameters);
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual async Task<DbDataReader> ExecuteReaderAsync(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync(command, parameters);
            }
        }

        #endregion ///ExecuteReaderAsync

        #region ExecuteReaderAsync<TResult>

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该数据库必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(DbCommand command, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            try
            {
                await EnsureConnectionAsync();
                CheckLocalTransaction(command);
                DbDataReader reader = await command.ExecuteReaderAsync();
                return LoadResult<TResult>(reader, readDataHandler);
            }
            catch
            {
                throw;
            }
            finally
            {
                ReleaseConnection();
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(DbCommand command, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            AddInParameters(command, parameters);
            return await ExecuteReaderAsync<TResult>(command, readDataHandler);
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该数据库必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(DbCommand command, object parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteReaderAsync<TResult>(command, readDataHandler);
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(string commandText, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync<TResult>(command, readDataHandler);
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(string commandText, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync<TResult>(command, parameters, readDataHandler);
            }
        }

        /// <summary>
        /// 异步执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual async Task<TResult> ExecuteReaderAsync<TResult>(string commandText, object parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderAsync<TResult>(command, parameters, readDataHandler);
            }
        }

        #endregion /// ExecuteReaderAsync<TResult>

        #region ExecuteReaderPagingAsync

        async Task<int> ReaderRowCountAsync(DbCommand command, int pageIndex, int pageSize)
        {
            //prepare DbCommand
            DbCommand rowCountCommand = RowCountCommand(command, pageIndex, pageSize);
            CheckLocalTransaction(rowCountCommand);

            int rowCount = -1;
            using (DbDataReader reader = await rowCountCommand.ExecuteReaderAsync())
            {
                if (reader.Read())
                {
                    rowCount = reader.GetInt32(0);
                }
            }
            return rowCount;
        }

        async Task<DbDataReader> ExecutePagingDataReader(
            DbCommand command, 
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize,
            CommandBehavior behavior)
        {
            if (pageIndex == 1)
            {
                FirstPageCommand(command, pageIndex, pageSize);
            }
            else
            {
                PageCommand(command, pageIndex, pageSize);
            }

            if (preparePagingCommand != null)
            {
                preparePagingCommand(command, this);
            }

            CheckLocalTransaction(command);
            DbDataReader reader = await command.ExecuteReaderAsync(behavior);
            return reader;
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            bool lastOpenedConnection = _openedConnection;
            bool isOpen = false;
            CommandBehavior behavior = CommandBehavior.Default;

            try
            {
                await EnsureConnectionAsync();
                isOpen = lastOpenedConnection != _openedConnection;
                if (isOpen)
                {
                    behavior = CommandBehavior.CloseConnection;
                }

                var rowCountTask = ReaderRowCountAsync(command, pageIndex, pageSize);
                var pagingReaderTask = ExecutePagingDataReader(command, preparePagingCommand, pageIndex, pageSize, behavior);

                int rowCount = await rowCountTask;
                DbDataReader reader = await pagingReaderTask;
                
                ReleaseConnection(false);
                PagingResult<DbDataReader> result = new PagingResult<DbDataReader>
                {
                    Result = reader,
                    RowCount = rowCount,
                    PageCount = PageCount(rowCount, pageSize)
                };
                return result;
            }
            catch
            {
                if (isOpen)
                    ReleaseConnection();
                else
                    ReleaseConnection(false);
                throw;
            }
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                command: command,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            AddInParameters(command, parameters);
            return await ExecuteReaderPagingAsync(command, preparePagingCommand, pageIndex, pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            IEnumerable<object> parameters,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteReaderPagingAsync(command, pageIndex, pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            DbCommand command,
            object parameters,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync(command, preparePagingCommand, pageIndex, pageSize);
            }
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                commandText: commandText,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync(command, parameters, preparePagingCommand, pageIndex, pageSize);
            }
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            IEnumerable<object> parameters,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            int pageIndex,
            int pageSize)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync(command, parameters, preparePagingCommand, pageIndex, pageSize);
            }
        }

        /// <summary>
        /// 异步执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult</returns>
        public virtual async Task<PagingResult<DbDataReader>> ExecuteReaderPagingAsync(
            string commandText,
            object parameters,
            int pageIndex,
            int pageSize)
        {
            return await ExecuteReaderPagingAsync(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        #endregion ///ExecuteReaderPagingAsync

        #region ExecuteReaderPagingAsync<TResult>

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>PagingResult数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            try
            {
                await EnsureConnectionAsync();

                var rowCountTask = ReaderRowCountAsync(command, pageIndex, pageSize);
                var pagingReaderTask = ExecutePagingDataReader(command, preparePagingCommand, pageIndex, pageSize, CommandBehavior.Default);

                int rowCount = await rowCountTask;
                DbDataReader reader = await pagingReaderTask;
                TResult resultData = LoadResult<TResult>(reader, readDataHandler);

                PagingResult<TResult> result = new PagingResult<TResult>
                {
                    Result = resultData,
                    RowCount = rowCount,
                    PageCount = PageCount(rowCount, pageSize)
                };
                return result;
            }
            catch
            {
                throw;
            }
            finally
            {
                ReleaseConnection();
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync<TResult>(
                command: command,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            AddInParameters(command, parameters);
            return await ExecuteReaderPagingAsync<TResult>(
                command: command, 
                preparePagingCommand: preparePagingCommand, 
                readDataHandler: readDataHandler, 
                pageIndex: pageIndex, 
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            IEnumerable<object> parameters,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync<TResult>(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            AnonymousAddInParameter(command, parameters);
            return await ExecuteReaderPagingAsync<TResult>(
                command: command, 
                preparePagingCommand: preparePagingCommand, 
                readDataHandler: readDataHandler, 
                pageIndex: pageIndex, 
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            DbCommand command,
            object parameters,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync<TResult>(
                    command: command, 
                    preparePagingCommand: preparePagingCommand, 
                    readDataHandler: readDataHandler, 
                    pageIndex:pageIndex, 
                    pageSize: pageSize);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync<TResult>(
                commandText: commandText,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync<TResult>(
                    command: command, 
                    parameters: parameters, 
                    preparePagingCommand: preparePagingCommand, 
                    readDataHandler: readDataHandler, 
                    pageIndex: pageIndex, 
                    pageSize: pageSize);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            IEnumerable<object> parameters,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync<TResult>(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return await ExecuteReaderPagingAsync<TResult>(
                    command: command, 
                    parameters: parameters, 
                    preparePagingCommand: preparePagingCommand, 
                    readDataHandler: readDataHandler, 
                    pageIndex: pageIndex, 
                    pageSize: pageSize);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns>数据集</returns>
        public virtual async Task<PagingResult<TResult>> ExecuteReaderPagingAsync<TResult>(
            string commandText,
            object parameters,
            Action<TResult, DbDataReader> readDataHandler,
            int pageIndex,
            int pageSize)
            where TResult : new()
        {
            return await ExecuteReaderPagingAsync<TResult>(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize);
        }

        #endregion ///ExecuteReaderPagingAsync<TResult>
    }
}
