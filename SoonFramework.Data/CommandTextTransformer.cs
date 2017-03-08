using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SoonFramework.Core;

namespace SoonFramework.Data
{
    internal class CommandTextTransformer
    {
        /// <summary>
        /// 对SQL语句中的索引参数占位符进行修正，并对DbCommand对象添加参数
        /// CommandText可以针对数据库特性编写，也可以使用较为通用的方式
        /// <code>
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = @p0";
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = {0}";
        /// </code>
        /// </summary>
        /// <param name="dba">DatabaseAccess对象</param>
        /// <param name="command">DbCommand对象</param>
        /// <param name="parameters">参数</param>
        public virtual void TransformingByIndexes(DatabaseAccess dba, DbCommand command, IEnumerable<object> parameters)
        {
            if(parameters != null || parameters.IsNotEmpty())
            {
                if (parameters.All(p => p is DbParameter))
                {
                    // 防止用户手动创建DbParameter对象
                    foreach (object value in parameters)
                    {
                        command.Parameters.Add((DbParameter)value);
                    }
                }
                else if (!parameters.Any(p => p is DbParameter))
                {
                    // 提供数据库原生的参数化语句或是占位符方式
                    // SQL Server: 
                    //      SELECT * FROM TableName WHERE Column = @ColumnValue;
                    //      SELECT * FROM TableName WHERE Column = {0};
                    int length = parameters.Count();
                    string[] parameterNames = new string[length];
                    string[] parameterSql = new string[length];
                    int i = 0;

                    foreach (object value in parameters)
                    {
                        parameterNames[i] = string.Format(CultureInfo.InvariantCulture, "p{0}", i);
                        dba.AddInParameter(command, parameterNames[i], value);
                        parameterSql[i] = dba.BuildParameterName(parameterNames[i]);

                        i++;
                    }
                    command.CommandText = string.Format(CultureInfo.InvariantCulture, command.CommandText, parameterSql);
                }
                else
                {
                    throw new InvalidOperationException("SQL参数要么全部都是DbParameter对象，要么全部都是值，不能混合。");
                }
            }
        }

        /// <summary>
        /// 对SQL语句中的索引参数占位符进行修正，并对DbCommand对象添加参数
        /// <code>
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = @p0";
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = {0}";
        /// </code>
        /// </summary>
        /// <param name="dba"></param>
        /// <param name="command"></param>
        public virtual void TransformingByIndexes(DatabaseAccess dba, DbCommand command)
        {
            if (command != null && command.Parameters.Count > 0)
            {
                string[] parameterNames = new string[command.Parameters.Count];
                for (int i = 0; i < parameterNames.Length; i++)
                {
                    parameterNames[i] = dba.BuildParameterName(String.Format(CultureInfo.InvariantCulture, "p{0}", i));
                    command.Parameters[i].ParameterName = parameterNames[i];
                }
                command.CommandText = string.Format(
                    CultureInfo.InvariantCulture, 
                    command.CommandText,
                    parameterNames);
            }
        }

        /// <summary>
        /// 对SQL语句中的命名参数占位符进行修正，并对DbCommand对象添加参数
        /// <code>
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = @ColumnName";
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = {ColumnName}";
        /// </code>
        /// </summary>
        /// <param name="dba">DatabaseAccess对象</param>
        /// <param name="command">DbCommand对象</param>
        /// <param name="parameterNames">格式化后的参数名 如@ParameterName</param>
        public virtual void TransformingByNames(DatabaseAccess dba, DbCommand command, IEnumerable<string> parameterNames)
        {
            if (parameterNames != null || parameterNames.IsNotEmpty())
            {
                command.CommandText = CommandTextFormatByNames(
                    command.CommandText,
                    parameterNames,
                    dba.BuildParameterName);
            }
        }

        /// <summary>
        /// 对SQL语句中的命名参数占位符进行修正，并对DbCommand对象添加参数
        /// </summary>
        /// <code>
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = @ColumnName";
        /// command.CommandText = "SELECT * FROM TableName WHERE Column = {ColumnName}";
        /// </code>
        /// <param name="dba">DatabaseAccess对象</param>
        /// <param name="command">DbCommand对象</param>
        public virtual void TransformingByNames(DatabaseAccess dba, DbCommand command)
        {
            if(command != null && command.Parameters.Count > 0)
            {
                string[] parameterNames = new string[command.Parameters.Count];
                for(int i = 0; i < parameterNames.Length; i++)
                {
                    parameterNames[i] = command.Parameters[i].ParameterName;
                }
                command.CommandText = CommandTextFormatByNames(
                    command.CommandText,
                    parameterNames,
                    dba.BuildParameterName);
            }
        }

        #region StringFormatExtensions

        static string Slice(string str, int startIndex)
        {
            if (String.IsNullOrEmpty(str))
            {
                return String.Empty;
            }
            int length = str.Length;
            while (startIndex < 0)
                startIndex = length + startIndex;
            if (startIndex >= length)
                return String.Empty;
            return str.Substring(startIndex);
        }

        static string Slice(string str, int startIndex, int endIndex)
        {
            if (String.IsNullOrEmpty(str))
            {
                return String.Empty;
            }

            int length = str.Length;
            while (startIndex < 0)
                startIndex = length + startIndex;
            if (startIndex >= length)
                return String.Empty;

            while (endIndex < 0)
                endIndex = length + endIndex;
            if (endIndex >= length)
                endIndex = length - 1;
            if (endIndex < startIndex)
                return string.Empty;
            return str.Substring(startIndex, endIndex - startIndex);
        }


        const char START_CHAR = '{';
        const char END_CHAR = '}';
        /// <summary>
        /// 对SQL语句进行命名参数格式替换 {ParameterName} => @ParameterName
        /// </summary>
        /// <param name="format">需要替换的SQL语句</param>
        /// <param name="names">参数名称，注意这里是已经格式化后符合数据库标准的参数</param>
        /// <param name="buildParameterName">数据库参数名称构造委托</param>
        /// <returns>格式化以后的SQL</returns>
        protected virtual string CommandTextFormatByNames(string format, IEnumerable<string> names, Func<string, string> buildParameterName)
        {
            if (String.IsNullOrEmpty(format) || names == null || names.IsEmpty())
            {
                return format;
            }
            StringBuilder builder = new StringBuilder();
            int i = 0;
            while (true)
            {
                int open = format.IndexOf(START_CHAR, i);
                int close = format.IndexOf(END_CHAR, i);
                if ((open < 0) && (close < 0))
                {
                    builder.Append(Slice(format, i));
                    break;
                }
                if ((close > 0) && ((close < open) || (open < 0)))
                {
                    if (format[close + 1] != END_CHAR)
                    {
                        throw new InvalidOperationException("如果想加入\"{\"或\"}\"字符需成双出现，如\"{{\"或\"}}\"");
                    }
                    builder.Append(Slice(format, i, close + 1));
                    i = close + 2;
                    continue;
                }
                builder.Append(Slice(format, i, open));
                i = open + 1;
                if (format[i] == START_CHAR)
                {
                    builder.Append(START_CHAR);
                    i++;
                    continue;
                }
                if (close < 0)
                    throw new InvalidOperationException("格式化字符串缺少闭合标记");
                string brace = format.Substring(i, close - i).Trim();
                string parameterName = buildParameterName(brace);
                string name = names.Single(o => o != null
                    ? o.Equals(parameterName, StringComparison.InvariantCultureIgnoreCase)
                    : false) as string;
                builder.Append(name);

                i = close + 1;
            }
            return builder.ToString();
        }

        #endregion
    }
}
