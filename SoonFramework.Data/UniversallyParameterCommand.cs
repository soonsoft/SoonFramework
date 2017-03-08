using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 支持通用参数化SQL的DbCommand包装器
    /// </summary>
    public class UniversallyParameterCommand : DbCommand
    {
        DbCommand m_command;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        public UniversallyParameterCommand(DbCommand command)
        {
            m_command = command;
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CommandText
        {
            get
            {
                return m_command.CommandText;
            }

            set
            {
                m_command.CommandText = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override int CommandTimeout
        {
            get
            {
                return m_command.CommandTimeout;
            }

            set
            {
                m_command.CommandTimeout = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override CommandType CommandType
        {
            get
            {
                return m_command.CommandType;
            }
            set
            {
                m_command.CommandType = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool DesignTimeVisible
        {
            get
            {
                return m_command.DesignTimeVisible;
            }

            set
            {
                m_command.DesignTimeVisible = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return m_command.UpdatedRowSource;
            }

            set
            {
                m_command.UpdatedRowSource = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override DbConnection DbConnection
        {
            get
            {
                return m_command.Connection;
            }

            set
            {
                m_command.Connection = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override DbParameterCollection DbParameterCollection
        {
            get
            {
                return m_command.Parameters;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override DbTransaction DbTransaction
        {
            get
            {
                return m_command.Transaction;
            }

            set
            {
                m_command.Transaction = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Cancel()
        {
            m_command.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int ExecuteNonQuery()
        {
            return m_command.ExecuteNonQuery();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override object ExecuteScalar()
        {
            return m_command.ExecuteScalar();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Prepare()
        {
            m_command.Prepare();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override DbParameter CreateDbParameter()
        {
            return m_command.CreateParameter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="behavior"></param>
        /// <returns></returns>
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return m_command.ExecuteReader(behavior);
        }
    }
}
