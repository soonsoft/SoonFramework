using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    /// <summary>
    /// 程序配置接口，程序配置信息是只读的
    /// </summary>
    public interface IConfiguration
    {

        #region Get Methods

        object GetObject(string key);

        string GetString(string key);

        bool GetBoolean(string key);

        char GetChar(string key);

        sbyte GetSByte(string key);

        byte GetByte(string key);

        short GetInt16(string key);

        ushort GetUInt16(string key);

        int GetInt32(string key);

        uint GetUInt32(string key);

        long GetInt64(string key);

        ulong GetUInt64(string key);

        float GetSingle(string key);

        double GetDouble(string key);

        decimal GetDecimal(string key);

        DateTime GetDateTime(string key);

        #endregion
    }
}
