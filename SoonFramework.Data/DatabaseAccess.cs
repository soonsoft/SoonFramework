using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Globalization;
using System.Transactions;
using System.Reflection;
using SoonFramework.Core;
using System.Data.Entity.Core.Common;

namespace SoonFramework.Data
{
    /// <summary>
    /// 数据库操作类
    /// 可以单独使用也可以作为EntityFramework的扩展使用
    /// </summary>
    public abstract partial class DatabaseAccess
    {
        private const string ParamName = "p";
        private DbConnection _connection;
        private DbProviderFactory _dbProviderFactory;

        //连接是否已经由内部API方法打开
        private bool _openedConnection;
        //打开连接计数器
        private int _connectionRequestCount;
        //需要排除的属性标记
        private readonly static Type IgnoreType = typeof(IgnoreAttribute);

        #region Constructor

        private DatabaseAccess()
        {
            InitialCommandTextTransformer();
        }

        /// <summary>
        /// 根据DbConnection连接对象构建DatabaseAccess
        /// for EntityFramework
        /// </summary>
        /// <param name="connection"></param>
        public DatabaseAccess(DbConnection connection, DbProviderFactory providerFactory = null)
            : this()
        {
            this._connection = connection;
            this._dbProviderFactory = providerFactory;
        }
        
        /// <summary>
        /// 根据DbProviderFactory对象构建DatabaseAccess
        /// </summary>
        /// <param name="providerFactory"></param>
        public DatabaseAccess(DbProviderFactory providerFactory)
            : this()
        {
            this._connection = providerFactory.CreateConnection();
            this._dbProviderFactory = providerFactory;
        }

        #endregion

        #region Propeties

        public abstract string ParameterPrefix { get; }

        /// <summary>
        /// 获取当前使用的数据库连接
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                return this._connection;
            }
        }

        /// <summary>
        /// 获取数据库Provider创建工厂
        /// </summary>
        public DbProviderFactory ProviderFactory
        {
            get
            {
                if (this._dbProviderFactory == null)
                {
                    this._dbProviderFactory = DbProviderServices.GetProviderFactory(Connection);
                }
                return this._dbProviderFactory;
            }
        }

        /// <summary>
        /// 获取当前使用的本地事务操作器
        /// </summary>
        internal DbTransactionScope LocalTransactionScope { get; set; }

        /// <summary>
        /// 获取当前使用的SQL语句转换器，对SQL语句中的参数进行替换
        /// </summary>
        internal virtual CommandTextTransformer CommandTextTransformer { get; private set; }

        #endregion

        #region DbConnection Manager

        /// <summary>
        /// 检查数据库连接的开启状态，如果数据库连接没有正常开启则打开，如果已经打开则不做任何处理
        /// </summary>
        internal virtual void EnsureConnection()
        {
            if (Connection.State == ConnectionState.Broken)
            {
                Connection.Close();
            }

            if (Connection.State == ConnectionState.Closed)
            {
                Connection.Open();
                _openedConnection = true;
            }

            if (_openedConnection)
            {
                _connectionRequestCount++;
            }
        }

        /// <summary>
        /// 是否连接，如果内部没有开启连接则调用此方法只会更新计数器并不会真正关闭连接
        /// </summary>
        /// <param name="realClose">默认为true</param>
        internal virtual void ReleaseConnection(bool realClose = true)
        {
            if (_openedConnection)
            {
                if (_connectionRequestCount > 0)
                {
                    _connectionRequestCount--;
                }

                // When no operation is using the connection and the context had opened the connection
                // the connection can be closed
                if (_connectionRequestCount == 0)
                {
                    if (realClose) 
                        Connection.Close();
                    _openedConnection = false;
                }
            }
        }

        /// <summary>
        /// 获取一个已经打开的数据库连接对象，
        /// 当需要和EntityFramework混合使用时避免数据库连接多次打开关闭
        /// 标准写法：
        /// using(var conn = dba.GetOpenConnection()) 
        /// {
        ///     ....
        /// }
        /// </summary>
        /// <returns>已经打开的数据库连接</returns>
        public virtual DbConnectionWrapper GetOpenConnection()
        {
            if (!_openedConnection)
            {
                var connectionWrapper = new DbConnectionWrapper(Connection);
                connectionWrapper.Open();
                return connectionWrapper;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取一个已经打开的数据库连接对象
        /// </summary>
        /// <param name="createDbConnectionWrapperFunc">通过指定该参数可以创建各种不同的DbConnectionWrapper对象</param>
        /// <returns></returns>
        public virtual DbConnectionWrapper GetOpenConnection(Func<DbConnection, DbConnectionWrapper> createDbConnectionWrapperFunc)
        {
            if (!_openedConnection)
            {
                var connectionWrapper = createDbConnectionWrapperFunc(Connection);
                connectionWrapper.Open();
                return connectionWrapper;
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Create DbCommand

        /// <summary>
        /// 创建一个DbCommand对象
        /// CommandText为Empty
        /// CommandType为Text
        /// </summary>
        /// <returns>DbCommand</returns>
        public virtual DbCommand CreateCommand()
        {
            return Connection.CreateCommand();
        }

        /// <summary>
        /// 创建一个带有超时时间的DbCommand
        /// </summary>
        /// <param name="timeout">timeout 0表示永远不超时，负数会抛出异常</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand(int timeout)
        {
            DbCommand command = CreateCommand();
            command.CommandTimeout = timeout;
            return command;
        }

        /// <summary>
        /// 创建DbCommand对象
        /// </summary>
        /// <param name="commandText">需要执行SQL</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand(string commandText)
        {
            return CreateCommand(commandText, CommandType.Text);
        }

        /// <summary>
        /// 创建DbCommand对象
        /// </summary>
        /// <param name="commandText">需要执行SQL</param>
        /// <param name="timeout">timeout 0表示永远不超时，负数会抛出异常</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand(string commandText, int timeout)
        {
            return CreateCommand(commandText, CommandType.Text, timeout);
        }

        /// <summary>
        /// 创建DbCommand对象
        /// </summary>
        /// <param name="commandText">需要执行SQL</param>
        /// <param name="commandType">CommandType, Text：SQL语句；StoredProcedure：存储过程</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand(string commandText, CommandType commandType)
        {
            DbCommand command = CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            return command;
        }

        /// <summary>
        /// 创建DbCommand对象
        /// </summary>
        /// <param name="commandText">需要执行SQL</param>
        /// <param name="commandType">CommandType, Text：SQL语句；StoredProcedure：存储过程</param>
        /// <param name="timeout">timeout 0表示永远不超时，负数会抛出异常</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateCommand(string commandText, CommandType commandType, int timeout)
        {
            DbCommand command = CreateCommand();
            command.CommandText = commandText;
            command.CommandType = commandType;
            command.CommandTimeout = timeout;
            return command;
        } 

        /// <summary>
        /// 创建一个用于执行存储过程的DbCommand对象
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateProcedureCommand(string procedureName)
        {
            return CreateCommand(procedureName, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 创建一个用于执行存储过程的DbCommand对象
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="timeout">timeout 0表示永远不超时，负数会抛出异常</param>
        /// <returns>DbCommand</returns>
        public DbCommand CreateProcedureCommand(string procedureName, int timeout)
        {
            return CreateCommand(procedureName, CommandType.StoredProcedure, timeout);
        }

        #endregion

        #region Create DbParameter

        /// <summary>
        /// 将匿名对象作为参数添加到Parameters集合中
        /// 当需要将普通class作为参数添加时不作为添加项的属性可以标记IgnoreAttribute
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数对象</param>
        /// <returns>PropertyDescriptorCollection</returns>
        PropertyDescriptorCollection AnonymousAddInParameter(DbCommand command, object parameters)
        {
            PropertyDescriptorCollection properties = null;
            if (parameters != null)
            {
                properties = TypeDescriptor.GetProperties(parameters);
                int i = 0;
                PropertyDescriptor[] propertyDescriptorArray = new PropertyDescriptor[properties.Count];
                string[] parameterNames = new string[propertyDescriptorArray.Length];
                bool flag;
                foreach (PropertyDescriptor property in properties)
                {
                    flag = false;
                    //排除特性
                    foreach(Attribute attr in property.Attributes)
                    {
                        if (attr.GetType() == IgnoreType)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag) continue;

                    propertyDescriptorArray[i] = property;
                    parameterNames[i] = BuildParameterName(property.Name);
                    AddInParameter(command, property.Name, property.GetValue(parameters));
                    i++;
                }
                CommandTextTransformer.TransformingByNames(this, command, parameterNames);
                properties = new PropertyDescriptorCollection(propertyDescriptorArray, true);
            }
            return properties;
        }

        /// <summary>
        /// 将数组或集合添加为Parameters
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数集合</param>
        void AddInParameters(DbCommand command, IEnumerable<object> parameters)
        {
            CommandTextTransformer.TransformingByIndexes(this, command, parameters);
        }

        /// <summary>
        /// 添加输入参数（用于手动为SQL语句中的索引参数设置值）
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="type">参数类型，对应DbType枚举</param>
        /// <param name="value">参数值</param>
        public void AddInParameter(DbCommand command, DbType type, object value)
        {
            AddInParameter(command, null, type, value);
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        public void AddInParameter(DbCommand command, string name, object value)
        {
            AddParameter(command, name, null, null, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, value);
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <param name="size">参数大小</param>
        public void AddInParameter(DbCommand command, string name, object value, int size)
        {
            AddParameter(command, name, null, size, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, value);
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型，对应DbType枚举</param>
        /// <param name="value">参数值</param>
        public void AddInParameter(DbCommand command, string name, DbType type, object value)
        {
            AddParameter(command, name, type, null, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, value);
        }

        /// <summary>
        /// 添加输入参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型，对应DbType枚举</param>
        /// <param name="size">参数大小</param>
        /// <param name="value">参数值</param>
        public void AddInParameter(DbCommand command, string name, DbType type, int size, object value)
        {
            AddParameter(command, name, type, size, ParameterDirection.Input, false, 0, 0, null, DataRowVersion.Default, value);
        }

        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型，对应DbType枚举</param>
        /// <returns>DbParameter</returns>
        public DbParameter AddOutParameter(DbCommand command, string name, DbType type)
        {
            return AddParameter(command, name, type, null, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
        }

        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="type">参数类型，对应DbType枚举</param>
        /// <param name="size">参数大小</param>
        /// <returns>DbParameter</returns>
        public DbParameter AddOutParameter(DbCommand command, string name, DbType type, int size)
        {
            return AddParameter(command, name, type, size, ParameterDirection.Output, true, 0, 0, String.Empty, DataRowVersion.Default, DBNull.Value);
        }

        /// <summary>
        /// 添加输出参数
        /// </summary>
        /// <param name="command">目标DbCommand对象</param>
        /// <param name="name">参数名</param>
        /// <param name="dbType">参数类型，对应DbType枚举</param>
        /// <param name="size">参数大小</param>
        /// <param name="direction">输入参或是输出参，对应ParameterDirection枚举</param>
        /// <param name="nullable">是否可以为null</param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn">源列名</param>
        /// <param name="sourceVersion">源版本号</param>
        /// <param name="value">参数值</param>
        /// <returns></returns>
        public virtual DbParameter AddParameter(DbCommand command,
                                                string name,
                                                DbType? dbType,
                                                int? size,
                                                ParameterDirection direction,
                                                bool nullable,
                                                byte precision,
                                                byte scale,
                                                string sourceColumn,
                                                DataRowVersion sourceVersion,
                                                object value)
        {
            Guard.ArgumentNotNull(command, "command");

            DbParameter parameter = CreateParameter(command);
            ConfigureParameter(parameter, name, dbType, size, direction, nullable, precision, scale, sourceColumn, sourceVersion, value);
            command.Parameters.Add(parameter);
            return parameter;
        }

        /// <summary>
        /// 通过DbCommand创建SQL参数对象
        /// </summary>
        /// <param name="command">DbCommand实例</param>
        /// <returns>DbParameter实例</returns>
        protected virtual DbParameter CreateParameter(DbCommand command)
        {
            Guard.ArgumentNotNull(command, "command");
            return command.CreateParameter();
        }

        /// <summary>
        /// 配置SQL参数
        /// </summary>
        /// <param name="param">DbParameter参数实例</param>
        /// <param name="name">参数名称</param>
        /// <param name="dbType">对应数据库类型</param>
        /// <param name="size">长度</param>
        /// <param name="direction">参数类型，对应input或者是out参数</param>
        /// <param name="nullable">是否可以为空</param>
        /// <param name="precision"></param>
        /// <param name="scale"></param>
        /// <param name="sourceColumn">源列名</param>
        /// <param name="sourceVersion">源版本号</param>
        /// <param name="value">参数值</param>
        protected virtual void ConfigureParameter(DbParameter param,
                                                  string name,
                                                  DbType? dbType,
                                                  int? size,
                                                  ParameterDirection direction,
                                                  bool nullable,
                                                  byte precision,
                                                  byte scale,
                                                  string sourceColumn,
                                                  DataRowVersion sourceVersion,
                                                  object value)
        {
            param.ParameterName = BuildParameterName(name);
            if (dbType != null)
                param.DbType = dbType.Value;
            if(size != null)
                param.Size = size.Value;
            param.Value = value ?? DBNull.Value;
            param.Direction = direction;
            param.IsNullable = nullable;
            param.SourceColumn = sourceColumn;
            param.SourceVersion = sourceVersion;
        }
        
        /// <summary>
        /// 构建符合数据要求的参数名称
        /// </summary>
        /// <param name="name">sql参数名称</param>
        /// <returns></returns>
        public virtual string BuildParameterName(string name)
        {
            if(String.IsNullOrEmpty(name))
            {
                return name;
            }
            if (ParameterPrefix[0] == name[0])
            {
                return name;
            }
            return String.Concat(ParameterPrefix, name);
        }

        /// <summary>
        /// 对DbCommand的CommandText属性值进行SQL语句的命名参数格式转换
        /// {Name} => @Name
        /// </summary>
        /// <param name="command"></param>
        public virtual void NamingParameters(DbCommand command)
        {
            CommandTextTransformer.TransformingByNames(this, command);
        }

        /// <summary>
        /// 对DbCommand的CommandText属性值进行SQL语句的索引参数格式转换
        /// {0} => @p0
        /// </summary>
        /// <param name="command"></param>
        public virtual void IndexingParameters(DbCommand command)
        {
            CommandTextTransformer.TransformingByIndexes(this, command);
        }

        /// <summary>
        /// 创建SQL语句参数格式转换器
        /// </summary>
        protected virtual void InitialCommandTextTransformer()
        {
            CommandTextTransformer = new CommandTextTransformer();
        }

        #endregion

        #region BeginTransaction

        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>事务管理器</returns>
        public DbTransactionScope BeginTransaction()
        {
            if(Transaction.Current == null)
            {
                if(LocalTransactionScope == null)
                {
                    LocalTransactionScope = new DbTransactionScope(this);
                    LocalTransactionScope.BeginTransaction();
                }
                return LocalTransactionScope;
            }
            else
            {
                throw new DbTransactionException();
            }
        }

        /// <summary>
        /// 开启指定事务等级的事务
        /// <para>IsolationLevel枚举用于控制当前的事务等级</para>
        /// <para>Chaos：无法覆盖隔离级别更高的事务中的挂起的更改。</para>
        /// <para>ReadCommitted：在正在读取数据时保持共享锁，以避免脏读，但是在事务结束之前可以更改数据，从而导致不可重复的读取或幻像数据。</para>
        /// <para>ReadUncommitted：可以进行脏读，意思是说，不发布共享锁，也不接受独占锁。</para>
        /// <para>RepeatableRead：在查询中使用的所有数据上放置锁，以防止其他用户更新这些数据。 防止不可重复的读取，但是仍可以有幻像行。 </para>
        /// <para>Serializable：在 DataSet 上放置范围锁，以防止在事务完成之前由其他用户更新行或向数据集中插入行。</para>
        /// <para>Snapshot：通过在一个应用程序正在修改数据时存储另一个应用程序可以读取的相同数据版本来减少阻止。 表示您无法从一个事务中看到在其他事务中进行的更改，即便重新查询也是如此。</para>
        /// <para>Unspecified：正在使用与指定隔离级别不同的隔离级别，但是无法确定该级别。当使用 OdbcTransaction 时，如果不设置 IsolationLevel 或将 IsolationLevel 设置为 Unspecified，则事务将根据由所使用的驱动程序所决定的隔离级别来执行。</para>
        /// </summary>
        /// <param name="level">事务等级</param>
        /// <returns>DbTransactionScope</returns>
        public DbTransactionScope BeginTransaction(System.Data.IsolationLevel level)
        {
            if (Transaction.Current == null)
            {
                if (LocalTransactionScope == null)
                {
                    LocalTransactionScope = new DbTransactionScope(this);
                    LocalTransactionScope.BeginTransaction(level);
                }
                return LocalTransactionScope;
            }
            else
            {
                throw new DbTransactionException();
            }
        }

        /// <summary>
        /// 判断当前上下文中是否开启本地事务，如果开启则为DbCommand对象设置事务
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns></returns>
        bool CheckLocalTransaction(DbCommand command)
        {
            if(LocalTransactionScope != null)
            {
                command.Transaction = LocalTransactionScope.LocalTransaction.Transaction;
                return true;
            }
            return false;
        }

        #endregion

        #region ExecuteNonQuery

        #region ExecuteNonQuery

        /// <summary>
        /// 执行SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(DbCommand command)
        {
            Guard.ArgumentNotNull(command, "command");

            try
            {
                EnsureConnection();
                //检查事务
                CheckLocalTransaction(command);
                return command.ExecuteNonQuery();
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
        /// 执行SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return ExecuteNonQuery(command);
        }

        /// <summary>
        /// 执行SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数对象</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteNonQuery(command);
        }

        /// <summary>
        /// 执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteNonQuery(command);
            }
        }

        /// <summary>
        /// 执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数数组</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteNonQuery(command, parameters);
            }
        }

        /// <summary>
        /// 执行SQL 语句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数对象</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQuery(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");
            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteNonQuery(command, parameters);
            }
        }

        #endregion /// ExecuteNonQuery

        #region ExecuteNonQueryMultiple

        /// <summary>
        /// 一次执行多条SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        protected int DoExecuteNonQueryMultiple(DbCommand command, IEnumerable<object> parameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            PropertyDescriptorCollection properties = null;
            int rowsAffected = 0;
            int i = 0;
            foreach (object obj in parameters)
            {
                if (properties == null)
                {
                    properties = AnonymousAddInParameter(command, obj);
                }
                else
                {
                    i = 0;
                    foreach (DbParameter param in command.Parameters)
                    {
                        param.Value = properties[i].GetValue(obj) ?? DBNull.Value;
                        i++;
                    }
                }
                rowsAffected += command.ExecuteNonQuery();
            }
            return rowsAffected;
        }

        /// <summary>
        /// 一次执行多条SQL 语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>影响的行数</returns>
        protected int DoExecuteNonQueryMultiple(DbCommand command, IEnumerable<object[]> parameters)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (parameters == null)
            {
                throw new ArgumentNullException("parameters");
            }

            int rowsAffected = 0;
            int paramLen = 0;
            if (command.Parameters.Count > 0)
                command.Parameters.Clear();
            int i = 0;
            foreach (object[] value in parameters)
            {
                if (value == null || value.Length == 0)
                    continue;
                if (paramLen == 0)
                {
                    for (i = 0; i < value.Length; i++)
                    {
                        AddInParameter(command, ParamName + i, value[i]);
                    }
                    paramLen = value.Length;
                }
                else
                {
                    i = 0;
                    foreach (DbParameter param in command.Parameters)
                    {
                        param.Value = value[i] ?? DBNull.Value;
                        i++;
                    }
                }
                rowsAffected += command.ExecuteNonQuery();
            }
            return rowsAffected;
        }

        /// <summary>
        /// 一次执行多条SQL 语句
        /// 如果在调用前没有开启事务则会自动打开事务
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQueryMultiple(DbCommand command, IEnumerable<object> parameters)
        {
            int rowsAffected = 0;
            if (!CheckLocalTransaction(command))
            {
                using (DbTransactionScope scope = BeginTransaction())
                {
                    scope.SetTransaction(command);
                    rowsAffected = DoExecuteNonQueryMultiple(command, parameters);
                    scope.Complete();
                }
            }
            else
            {
                rowsAffected = DoExecuteNonQueryMultiple(command, parameters);
            }
            return rowsAffected;
        }

        /// <summary>
        /// 一次执行多条SQL 语句
        /// 如果在调用前没有开启事务则会自动打开事务
        /// IEnumerable&lt;object[]&gt;的参数可以通过QueryParameterUtility.TransferMultipleParameters转换
        /// 如：
        /// Guid[] guidArr = new Guid[10] { ... };
        /// IList&lt;object[]&gt; parameters = QueryParameterUtility.TransferMultipleParameters&lt;Guid&gt;(guidArr);
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQueryMultiple(DbCommand command, IEnumerable<object[]> parameters)
        {
            int rowsAffected = 0;
            if (!CheckLocalTransaction(command))
            {
                using (DbTransactionScope scope = BeginTransaction())
                {
                    scope.SetTransaction(command);
                    rowsAffected = DoExecuteNonQueryMultiple(command, parameters);
                    scope.Complete();
                }
            }
            else
            {
                rowsAffected = DoExecuteNonQueryMultiple(command, parameters);
            }
            return rowsAffected;
        }

        /// <summary>
        /// 一次执行多条SQL 语句
        /// 如果在调用前没有开启事务则会自动打开事务
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQueryMultiple(string commandText, IEnumerable<object> parameters)
        {
            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }
            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteNonQueryMultiple(command, parameters);
            }
        }

        /// <summary>
        /// 一次执行多条SQL 语句
        /// 如果在调用前没有开启事务则会自动打开事务
        /// IEnumerable<object[]>的参数可以通过QueryParameterUtility.TransferMultipleParameters转换
        /// 如：
        /// Guid[] guidArr = new Guid[10] { ... };
        /// IList<object[]> parameters = QueryParameterUtility.TransferMultipleParameters<Guid>(guidArr);
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数列表</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteNonQueryMultiple(string commandText, IEnumerable<object[]> parameters)
        {
            if (String.IsNullOrEmpty(commandText))
            {
                throw new ArgumentNullException("commandText");
            }
            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteNonQueryMultiple(command, parameters);
            }
        }

        #endregion /// ExecuteNonQueryMultiple

        #endregion

        #region ExecuteReader

        #region ExecuteReader

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(DbCommand command)
        {
            Guard.ArgumentNotNull(command, "command");

            bool lastOpenedConnection = _openedConnection;
            bool isOpen = false;
            CommandBehavior behavior = CommandBehavior.Default;
            try
            {
                EnsureConnection();
                //判断是否在方法中开启了连接，如果由方法内部开启则需要在关闭DataReader时关闭连接
                isOpen = lastOpenedConnection != _openedConnection;
                if (isOpen)
                {
                    behavior = CommandBehavior.CloseConnection;
                }
                CheckLocalTransaction(command);
                DbDataReader reader = command.ExecuteReader(behavior);
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
        /// 执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return ExecuteReader(command);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteReader(command);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader(command);
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader(command, parameters);
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteReader(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader(command, parameters);
            }
        }

        #endregion ///ExecuteReader

        #region ExecuteReader<TResult>

        /// <summary>
        /// 加载查询数据集
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="reader">DbDataReader实例</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        protected virtual TResult LoadResult<TResult>(DbDataReader reader, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            TResult result = new TResult();
            if(reader != null)
            {
                using(reader)
                {
                    while (reader.Read())
                    {
                        readDataHandler(result, reader);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(DbCommand command, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            try
            {
                EnsureConnection();
                CheckLocalTransaction(command);
                DbDataReader reader = command.ExecuteReader();
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
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(DbCommand command, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            AddInParameters(command, parameters);
            return ExecuteReader<TResult>(command, readDataHandler);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该数据库必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(DbCommand command, object parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteReader<TResult>(command, readDataHandler);
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(string commandText, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader<TResult>(command, readDataHandler);
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(string commandText, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader<TResult>(command, parameters, readDataHandler);
            }
        }

        /// <summary>
        /// 执行查询
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该类型必须有空构造</typeparam>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReader<TResult>(string commandText, object parameters, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReader<TResult>(command, parameters, readDataHandler);
            }
        }

        #endregion ///ExecuteReader<TResult>

        #region ExecuteReaderPaging

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            //prepare DbCommand
            DbCommand rowCountCommand = RowCountCommand(command, pageIndex, pageSize);
            if (pageIndex == 1)
            {
                FirstPageCommand(command, pageIndex, pageSize);
            }
            else
            {
                PageCommand(command, pageIndex, pageSize);
            }

            if(preparePagingCommand != null)
            {
                preparePagingCommand(command, this);
            }

            bool lastOpenedConnection = _openedConnection;
            bool isOpen = false;
            CommandBehavior behavior = CommandBehavior.Default;

            try
            {
                EnsureConnection();
                isOpen = lastOpenedConnection != _openedConnection;
                if (isOpen)
                {
                    behavior = CommandBehavior.CloseConnection;
                }

                CheckLocalTransaction(rowCountCommand);
                DbDataReader reader = null;
                rowCount = -1;
                using (reader = rowCountCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rowCount = reader.GetInt32(0);
                    }
                }

                CheckLocalTransaction(command);
                reader = command.ExecuteReader(behavior);
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
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                command: command,
                preparePagingCommand: null, 
                pageIndex: pageIndex, 
                pageSize: pageSize, 
                rowCount: out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            AddInParameters(command, parameters);
            return ExecuteReaderPaging(command, preparePagingCommand, pageIndex, pageSize, out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            IEnumerable<object> parameters, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteReaderPaging(command, pageIndex, pageSize, out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            DbCommand command, 
            object parameters,
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging(command, preparePagingCommand, pageIndex, pageSize, out rowCount);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                commandText: commandText,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging(command, parameters, preparePagingCommand, pageIndex, pageSize, out rowCount);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            IEnumerable<object> parameters, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="preparePagingCommand">分页语句后续处理</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging(command, parameters, preparePagingCommand, pageIndex, pageSize, out rowCount);
            }
        }

        /// <summary>
        /// 执行分页查询
        /// 注意，SQL语句中一定要包含ORDER BY子句
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="rowCount">总记录数</param>
        /// <returns>DbDataReader 读取器</returns>
        public virtual DbDataReader ExecuteReaderPaging(
            string commandText,
            object parameters,
            int pageIndex, 
            int pageSize, 
            out int rowCount)
        {
            return ExecuteReaderPaging(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        #endregion ///ExecuteReaderPaging

        #region ExecuteReaderPaging<TResult>

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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            //Prepare DbCommand
            DbCommand rowCountCommand = RowCountCommand(command, pageIndex, pageSize);
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

            try
            {
                EnsureConnection();
                CheckLocalTransaction(rowCountCommand);
                rowCount = -1;
                using (DbDataReader reader = rowCountCommand.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        rowCount = reader.GetInt32(0);
                    }
                }
                return ExecuteReader<TResult>(command, readDataHandler);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging<TResult>(
                command: command,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            AddInParameters(command, parameters);
            return ExecuteReaderPaging<TResult>(
                command: command,
                preparePagingCommand: preparePagingCommand,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize, 
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            IEnumerable<object> parameters, 
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging<TResult>(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteReaderPaging<TResult>(
                command: command,
                preparePagingCommand: preparePagingCommand,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize, 
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            DbCommand command,
            object parameters,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging(
                command: command,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            Action<DbCommand, DatabaseAccess> preparePagingCommand,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging<TResult>(
                    command: command,
                    preparePagingCommand: preparePagingCommand,
                    readDataHandler: readDataHandler,
                    pageIndex: pageIndex,
                    pageSize: pageSize, 
                    rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging<TResult>(
                commandText: commandText,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            IEnumerable<object> parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging<TResult>(
                    command: command,
                    parameters: parameters,
                    preparePagingCommand: preparePagingCommand, 
                    readDataHandler: readDataHandler,
                    pageIndex: pageIndex,
                    pageSize: pageSize, 
                    rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            IEnumerable<object> parameters,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging<TResult>(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            object parameters,
            Action<DbCommand, DatabaseAccess> preparePagingCommand, 
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteReaderPaging<TResult>(
                    command: command, 
                    parameters: parameters,
                    preparePagingCommand: preparePagingCommand,
                    readDataHandler: readDataHandler, 
                    pageIndex: pageIndex,
                    pageSize: pageSize, 
                    rowCount: out rowCount);
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
        /// <param name="rowCount">总记录数</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteReaderPaging<TResult>(
            string commandText,
            object parameters,
            Action<TResult, DbDataReader> readDataHandler, 
            int pageIndex, 
            int pageSize, 
            out int rowCount)
            where TResult : new()
        {
            return ExecuteReaderPaging<TResult>(
                commandText: commandText,
                parameters: parameters,
                preparePagingCommand: null,
                readDataHandler: readDataHandler,
                pageIndex: pageIndex,
                pageSize: pageSize,
                rowCount: out rowCount);
        }

        #endregion ///ExecuteReaderPaging<TResult>

        #endregion

        #region ExecuteScalar

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(DbCommand command)
        {
            try
            {
                EnsureConnection();
                CheckLocalTransaction(command);
                return command.ExecuteScalar();
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
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return ExecuteScalar(command);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteScalar(command);
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(string commandText)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteScalar(command);
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(string commandText, IEnumerable<object> parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteScalar(command, parameters);
            }
        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中的第一行第一列。所有其它的行和列将被忽略
        /// </summary>
        /// <param name="commandText">SQL 语句</param>
        /// <param name="parameters">参数</param>
        /// <returns>查询结果</returns>
        public virtual object ExecuteScalar(string commandText, object parameters)
        {
            Guard.ArgumentNotNullOrEmpty(commandText, "commandText");

            using (DbCommand command = CreateCommand(commandText))
            {
                return ExecuteScalar(command, parameters);
            }
        }

        #endregion

        #region DataSet

        /// <summary>
        /// Sets the RowUpdated event for the data adapter.
        /// </summary>
        /// <param name="adapter">The <see cref="DbDataAdapter"/> to set the event.</param>
        protected virtual void SetUpRowUpdatedEvent(DbDataAdapter adapter) { }

        /// <summary>
        /// 获取DataAdapter对象
        /// </summary>
        /// <param name="updateBehavior"></param>
        /// <returns>数据适配器对象</returns>
        protected DbDataAdapter GetDataAdapter(UpdateBehavior updateBehavior)
        {
            DbDataAdapter adapter = ProviderFactory.CreateDataAdapter();
            if (updateBehavior == UpdateBehavior.Continue)
            {
                SetUpRowUpdatedEvent(adapter);
            }
            return adapter;
        }

        #region LoadDataSet

        /// <summary>
        /// 填充DataSet
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="dataSet">DataSet</param>
        /// <param name="tableNames">需要填充的表名</param>
        void DoLoadDataSet(DbCommand command,
                           DataSet dataSet,
                           params string[] tableNames)
        {
            Guard.ArgumentNotNullOrEmpty(tableNames, "tableNames");

            for (int i = 0; i < tableNames.Length; i++)
            {
                if (string.IsNullOrEmpty(tableNames[i])) throw new ArgumentException(string.Concat("tableNames[", i, "] is Null or Empty"));
            }

            try
            {
                EnsureConnection();
                CheckLocalTransaction(command);
                using (DbDataAdapter adapter = GetDataAdapter(UpdateBehavior.Standard))
                {
                    ((IDbDataAdapter)adapter).SelectCommand = command;

                    string systemCreatedTableNameRoot = "Table";
                    for (int i = 0; i < tableNames.Length; i++)
                    {
                        string systemCreatedTableName = (i == 0)
                                                            ? systemCreatedTableNameRoot
                                                            : systemCreatedTableNameRoot + i;

                        adapter.TableMappings.Add(systemCreatedTableName, tableNames[i]);
                    }

                    adapter.Fill(dataSet);
                }
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
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DateSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command)
        {
            return ExecuteDataSet(command, "Table");
        }

        /// <summary>
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="tableNames">表名</param>
        /// <returns>DataSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command, params string[] tableNames)
        {
            DataSet dataSet = new DataSet();
            dataSet.Locale = CultureInfo.InvariantCulture;
            DoLoadDataSet(command, dataSet, tableNames);
            return dataSet;
        }

        /// <summary>
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command, IEnumerable<object> parameters)
        {
            AddInParameters(command, parameters);
            return ExecuteDataSet(command);
        }

        /// <summary>
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <returns>DataSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command, object parameters)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteDataSet(command);
        }

        /// <summary>
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="tableNames">表名</param>
        /// <returns>DataSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command, IEnumerable<object> parameters, params string[] tableNames)
        {
            AddInParameters(command, parameters);
            return ExecuteDataSet(command, tableNames);
        }

        /// <summary>
        /// 执行查询并返回DataSet数据集
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="parameters">参数</param>
        /// <param name="tableNames">表名</param>
        /// <returns>DataSet数据集</returns>
        public virtual DataSet ExecuteDataSet(DbCommand command, object parameters, params string[] tableNames)
        {
            AnonymousAddInParameter(command, parameters);
            return ExecuteDataSet(command, tableNames);
        }

        #endregion /// LoadDataSet

        #region UpdateDataSet

        /// <summary>
        /// 更新DataSet数据集
        /// </summary>
        /// <param name="behavior">更新方式 Standard: 出错就停止; Continue: 出错继续更新; Transactional: 出错回滚</param>
        /// <param name="dataSet">更新的数据集</param>
        /// <param name="tableName">表名</param>
        /// <param name="insertCommand">执行Insert操作</param>
        /// <param name="updateCommand">执行Update操作</param>
        /// <param name="deleteCommand">执行Delete操作</param>
        /// <param name="updateBatchSize">是否使用批处理</param>
        /// <returns></returns>
        int DoUpdateDataSet(UpdateBehavior behavior,
                            DataSet dataSet,
                            string tableName,
                            DbCommand insertCommand,
                            DbCommand updateCommand,
                            DbCommand deleteCommand,
                            int? updateBatchSize)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentException("tableName is Null or Empty");
            if (dataSet == null) throw new ArgumentNullException("dataSet");

            if (insertCommand == null && updateCommand == null && deleteCommand == null)
            {
                throw new ArgumentException("all commands is null");
            }

            try
            {
                //数据库连接管理，如果连接是在外部打开时DataAdapter对象是不会自动关闭数据库连接的
                EnsureConnection();
                using (DbDataAdapter adapter = GetDataAdapter(behavior))
                {
                    IDbDataAdapter explicitAdapter = adapter;
                    if (insertCommand != null)
                    {
                        CheckLocalTransaction(insertCommand);
                        explicitAdapter.InsertCommand = insertCommand;
                    }
                    if (updateCommand != null)
                    {
                        CheckLocalTransaction(updateCommand);
                        explicitAdapter.UpdateCommand = updateCommand;
                    }
                    if (deleteCommand != null)
                    {
                        CheckLocalTransaction(deleteCommand);
                        explicitAdapter.DeleteCommand = deleteCommand;
                    }

                    if (updateBatchSize != null)
                    {
                        adapter.UpdateBatchSize = (int)updateBatchSize;
                        if (insertCommand != null)
                            adapter.InsertCommand.UpdatedRowSource = UpdateRowSource.None;
                        if (updateCommand != null)
                            adapter.UpdateCommand.UpdatedRowSource = UpdateRowSource.None;
                        if (deleteCommand != null)
                            adapter.DeleteCommand.UpdatedRowSource = UpdateRowSource.None;
                    }

                    int rows = adapter.Update(dataSet.Tables[tableName]);
                    return rows;
                }
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
        /// 更新DataSet数据集
        /// </summary>
        /// <param name="dataSet">更新的数据集</param>
        /// <param name="tableName">表名</param>
        /// <param name="insertCommand">执行Insert操作</param>
        /// <param name="updateCommand">执行Update操作</param>
        /// <param name="deleteCommand">执行Delete操作</param>
        /// <param name="updateBehavior">更新方式 Standard: 出错就停止; Continue: 出错继续更新; Transactional: 出错回滚</param>
        /// <param name="updateBatchSize">是否使用批处理</param>
        /// <returns></returns>
        public int UpdateDataSet(DataSet dataSet,
                                 string tableName,
                                 DbCommand insertCommand,
                                 DbCommand updateCommand,
                                 DbCommand deleteCommand,
                                 UpdateBehavior updateBehavior,
                                 int? updateBatchSize)
        {
            int rowsAffected = 0;
            using (Connection)
            {
                if (updateBehavior == UpdateBehavior.Transactional && LocalTransactionScope == null)
                {
                    using(var transaction = BeginTransaction())
                    {
                        rowsAffected = DoUpdateDataSet(updateBehavior, dataSet, tableName,
                                           insertCommand, updateCommand, deleteCommand, updateBatchSize);
                        transaction.Complete();
                    }
                }
                else
                {
                    rowsAffected = DoUpdateDataSet(updateBehavior, dataSet, tableName,
                                           insertCommand, updateCommand, deleteCommand, updateBatchSize);
                }
                return rowsAffected;
            }
        }

        #endregion /// UpdateDataSet

        #endregion

        #region ExecuteProcedure

        static void CheckProcedureCommand(DbCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }
            if (command.CommandType != CommandType.StoredProcedure)
            {
                throw new ArgumentException("command.CommandType is not CommandType.StoredProcedure");
            }
        }

        #region NonQuery

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>影响的行数</returns>
        public virtual int ExecuteProcedureNonQuery(DbCommand command)
        {
            CheckProcedureCommand(command);

            return ExecuteNonQuery(command);
        }

        /// <summary>
        /// 执行存储过程
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns>影响的行数</returns>
        public virtual int ExceuteProcedureNonQuery(string procedureName)
        {
            Guard.ArgumentNotNullOrEmpty(procedureName, "procedureName");

            DbCommand command = CreateCommand(procedureName, CommandType.StoredProcedure);

            return ExecuteNonQuery(command);
        }

        //public abstract int ExceuteProcedureNonQuery(string procedureName, out object outValue);

        //public abstract int ExceuteProcedureNonQuery(string procedureName, IEnumerable<object> parameters);

        //public abstract int ExceuteProcedureNonQuery(string procedureName, IEnumerable<object> parameters, out object outValue);

        //public abstract int ExceuteProcedureNonQuery(string procedureName, object parameters);

        //public abstract int ExceuteProcedureNonQuery(string procedureName, object parameters, out object outValue);

        #endregion

        #region DataReader

        /// <summary>
        /// 执行存储过程，并返回DbDataReader读取器
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteProcedureReader(DbCommand command)
        {
            CheckProcedureCommand(command);

            return ExecuteReader(command);
        }

        /// <summary>
        /// 执行存储过程，并返回DbDataReader读取器
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <returns>DbDataReader读取器</returns>
        public virtual DbDataReader ExecuteProcedureReader(string procedureName)
        {
            if (String.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentNullException("procedureName");
            }
            DbCommand command = CreateCommand(procedureName, CommandType.StoredProcedure);

            return ExecuteReader(command);
        }

        //public abstract DbDataReader ExceuteProcedureReader(string procedureName, out object outValue);

        //public abstract DbDataReader ExceuteProcedureReader(string procedureName, IEnumerable<object> parameters);

        //public abstract DbDataReader ExceuteProcedureReader(string procedureName, IEnumerable<object> parameters, out object outValue);

        //public abstract DbDataReader ExceuteProcedureReader(string procedureName, object parameters);

        //public abstract DbDataReader ExceuteProcedureReader(string procedureName, object parameters, out object outValue);

        #endregion

        #region DataReader<TResult>

        /// <summary>
        /// 执行存储过程，返回数据集
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该数据库必须有空构造</typeparam>
        /// <param name="command">DbCommand</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteProcedureReader<TResult>(DbCommand command, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            CheckProcedureCommand(command);

            return ExecuteReader<TResult>(command, readDataHandler);
        }

        /// <summary>
        /// 执行存储过程，返回数据集
        /// </summary>
        /// <typeparam name="TResult">返回数据集的类型，该数据库必须有空构造</typeparam>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="readDataHandler">DbDataReader和数据集映射的委托</param>
        /// <returns>数据集</returns>
        public virtual TResult ExecuteProcedureReader<TResult>(string procedureName, Action<TResult, DbDataReader> readDataHandler)
            where TResult : new()
        {
            if (String.IsNullOrEmpty(procedureName))
            {
                throw new ArgumentNullException("procedureName");
            }
            DbCommand command = CreateCommand(procedureName, CommandType.StoredProcedure);

            return ExecuteReader<TResult>(command, readDataHandler);
        }

        //public abstract TResult ExceuteProcedureReader<TResult>(string procedureName, Action<TResult, DbDataReader> readDataHandler, out object outValue) 
        //    where TResult : new();

        //public abstract TResult ExceuteProcedureReader<TResult>(string procedureName, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler) 
        //    where TResult : new();

        //public abstract TResult ExceuteProcedureReader<TResult>(string procedureName, IEnumerable<object> parameters, Action<TResult, DbDataReader> readDataHandler, out object outValue) 
        //    where TResult : new();

        //public abstract TResult ExceuteProcedureReader<TResult>(string procedureName, object parameters, Action<TResult, DbDataReader> readDataHandler) 
        //    where TResult : new();

        //public abstract TResult ExceuteProcedureReader<TResult>(string procedureName, object parameters, Action<TResult, DbDataReader> readDataHandler, out object outValue) 
        //    where TResult : new();

        #endregion

        #endregion

        #region Paging

        /// <summary>
        /// 计算页数
        /// </summary>
        /// <param name="rowCount">记录总数</param>
        /// <param name="pageSize">每页显示的记录数</param>
        /// <returns>总页数</returns>
        public static int PageCount(int rowCount, int pageSize)
        {
            return (rowCount + pageSize - 1) / pageSize;
        }

        /// <summary>
        /// 计算分页的起始行号 从1开始
        /// </summary>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        /// <returns>起始行号</returns>
        protected static int RowStart(int pageIndex, int pageSize)
        {
            int rowEnd = pageIndex * pageSize;
            return (rowEnd + 1) - pageSize;
        }

        /// <summary>
        /// 计算分页的结束行号
        /// </summary>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        /// <returns>结束行号</returns>
        protected static int RowEnd(int pageIndex, int pageSize)
        {
            return pageIndex * pageSize;
        }

        protected const string AS_KEY = " AS ";
        protected const string EQUAL_KEY = "=";
        protected const string POINT_KEY = ".";
        protected const string SELECT = "SELECT";
        protected const string FROM = "FROM";
        protected const string ORDER_BY = "ORDER BY";
        protected const string WHERE = "WHERE";

        protected static void CheckOrderClause(string commandText)
        {
            if(commandText.LastIndexOf(ORDER_BY, StringComparison.CurrentCultureIgnoreCase) < 0)
            {
                throw new CommandTextFormatException("分页SQL语句语法错误，分页查询语句必须包含ORDER BY子句");
            }
        }

        /// <summary>
        /// 分析查询语句中的列
        /// </summary>
        /// <param name="selectColumns"></param>
        /// <returns></returns>
        public static string GetSelectColumns(string selectColumns)
        {
            selectColumns = selectColumns.Trim();
            string all = "*";
            if (String.IsNullOrEmpty(selectColumns) || selectColumns == all)
            {
                return all;
            }

            string[] columnItems = selectColumns.Split(',');
            StringBuilder columns = new StringBuilder();
            int index = -1;
            string temp = null;
            for (int i = 0; i < columnItems.Length; i++)
            {
                temp = columnItems[i];
                if ((index = temp.LastIndexOf(AS_KEY, StringComparison.CurrentCultureIgnoreCase)) > -1)
                {
                    temp = temp.Substring(index + AS_KEY.Length);
                }
                else if ((index = temp.IndexOf(EQUAL_KEY)) > -1)
                {
                    temp = temp.Substring(index + 1);
                }
                else if ((index = temp.IndexOf(POINT_KEY)) > -1)
                {
                    temp = temp.Substring(index + 1);
                }
                temp = temp.Trim();
                if (temp == all)
                    return all;
                columns.Append(temp);
                if (i < columnItems.Length - 1)
                    columns.Append(",");
            }
            return columns.ToString();
        }

        /// <summary>
        /// 将查询语句包装成查询总记录数的语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <returns>包装后的语句</returns>
        protected virtual DbCommand RowCountCommand(DbCommand command)
        {
            string commandText = command.CommandText;
            int index = commandText.IndexOf(ORDER_BY, StringComparison.CurrentCultureIgnoreCase);
            if (index >= 0)
            {
                commandText = commandText.Substring(0, index);
            }
            DbCommand rowCountCommand = CreateCommand(
                String.Concat("SELECT COUNT(1) FROM (", commandText, ") COUNT_TEMP_TABLE"));
            rowCountCommand.Parameters.AddRange(
                ParameterCache.CreateParameterCopy(command));
            return rowCountCommand;
        }

        /// <summary>
        /// 将查询语句包装成查询总记录数的语句
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页（暂时无效）</param>
        /// <param name="pageSize">每页显示的记录数（暂时无效）</param>
        /// <returns>包装后的语句</returns>
        protected virtual DbCommand RowCountCommand(DbCommand command, int pageIndex, int pageSize)
        {
            return RowCountCommand(command);
        }

        /// <summary>
        /// 包装查询第一页的DbCommand
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        protected abstract void FirstPageCommand(DbCommand command, int pageIndex, int pageSize);

        /// <summary>
        /// 包装分页查询的DbCommand
        /// </summary>
        /// <param name="command">DbCommand</param>
        /// <param name="pageIndex">第几页</param>
        /// <param name="pageSize">每页显示的记录数</param>
        protected abstract void PageCommand(DbCommand command, int pageIndex, int pageSize);

        #endregion
    }
}
