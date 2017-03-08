using SoonFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    public abstract class ConfigurationBase : ISettings
    {

        #region Get Methods

        public virtual bool GetBoolean(string key)
        {
            return bool.Parse(GetString(key));
        }

        public virtual byte GetByte(string key)
        {
            return byte.Parse(GetString(key));
        }

        public virtual char GetChar(string key)
        {
            return char.Parse(GetString(key));
        }

        public virtual DateTime GetDateTime(string key)
        {
            return DateTime.Parse(GetString(key));
        }

        public virtual decimal GetDecimal(string key)
        {
            return decimal.Parse(GetString(key));
        }

        public virtual double GetDouble(string key)
        {
            return double.Parse(GetString(key));
        }

        public virtual short GetInt16(string key)
        {
            return short.Parse(GetString(key));
        }

        public virtual int GetInt32(string key)
        {
            return int.Parse(GetString(key));
        }

        public virtual long GetInt64(string key)
        {
            return long.Parse(GetString(key));
        }

        public virtual object GetObject(string key)
        {
            return GetString(key);
        }

        public virtual sbyte GetSByte(string key)
        {
            return sbyte.Parse(GetString(key));
        }

        public virtual float GetSingle(string key)
        {
            return float.Parse(GetString(key));
        }

        public abstract string GetString(string key);

        public virtual ushort GetUInt16(string key)
        {
            return ushort.Parse(GetString(key));
        }

        public virtual uint GetUInt32(string key)
        {
            return uint.Parse(GetString(key));
        }

        public virtual ulong GetUInt64(string key)
        {
            return ulong.Parse(GetString(key));
        }

        #endregion

        public string this[string key]
        {
            get
            {
                return GetString(key);
            }
        }

        public virtual void Initial()
        {
        }

        public virtual void Load()
        {
        }
    }
}
