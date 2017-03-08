using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Data
{
    /// <summary>
    /// 数据库查询语句格式化异常
    /// </summary>
    [Serializable]
    public class CommandTextFormatException : ApplicationException
    {
        public CommandTextFormatException()
        {
        }

        public CommandTextFormatException(string message)
            : base(message)
        {
        }

        public CommandTextFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CommandTextFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
