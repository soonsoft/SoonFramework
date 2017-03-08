using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 数据库事务异常
    /// </summary>
    [Serializable]
    public class DbTransactionException :  ApplicationException
    {
        private const string ExceptionMessage = "已创建环境事务(System.Transactions.TransactionScope)";
        public DbTransactionException()
            : base(ExceptionMessage)
        {
        }

        public DbTransactionException(string message)
            : base(message)
        {
        }

        public DbTransactionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public DbTransactionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
