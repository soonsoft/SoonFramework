using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using SoonFramework.Core;
using System.Globalization;
using System.Collections.Specialized;

namespace SoonFramework.Configuration
{
    public class ConfigurationSettings : ConfigurationBase
    {
        private readonly Dictionary<string, string> m_settingValues = null;

        protected virtual IDictionary<string, string> SettingValues
        {
            get
            {
                return m_settingValues;
            }
        }

        NameValueCollection AppSettings
        {
            get
            {
                return ConfigurationManager.AppSettings;
            }
        }

        public ConfigurationSettings()
        {
            m_settingValues = new Dictionary<string, string>(20);
        }

        public override string GetString(string key)
        {
            Guard.ArgumentNotNullOrEmpty(key, "key");

            var settingValues = SettingValues;
            if (settingValues.ContainsKey(key))
            {
                return settingValues[key];
            }
            return null;
        }

        public DateTime GetDateTime(string key, string format)
        {
            return DateTime.ParseExact(GetString(key), format, CultureInfo.InvariantCulture);
        }

        public override void Load()
        {
            var appSettings = AppSettings;
            string[] keys = appSettings.AllKeys;
            for (int i = 0; i < keys.Length; i++)
            {
                m_settingValues.Add(keys[i], appSettings[keys[i]]);
            }
        }
    }
}
