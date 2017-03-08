using SoonFramework.Core;
using SoonFramework.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    public class ApplicationSettings : ConfigurationBase
    {
        private const string DEFAULT_GROUP = "DEFAULT_GROUP";
        private Dictionary<string, ApplicationSettingItem> m_settingValues = null;
        private Dictionary<string, ApplicationSettingGroup> m_settingGroups = null;
        private string m_settingConnectionName = null;
        private ApplicationSettingsDataProvider m_dataProvider = null;

        protected virtual IDictionary<string, ApplicationSettingItem> SettingValues
        {
            get
            {
                return m_settingValues;
            }
        }

        public ApplicationSettingGroup[] Groups
        {
            get
            {
                var sourceGroups = m_settingGroups.Values;
                ApplicationSettingGroup[] groups = new ApplicationSettingGroup[sourceGroups.Count];
                int index = 0;
                foreach(var group in sourceGroups)
                {
                    groups[index] = group;
                    index++;
                }
                return groups;
            }
        }

        public ApplicationSettingsDataProvider DataProvider
        {
            get
            {
                if (m_dataProvider == null)
                {
                    m_dataProvider = CreateDatabaseAccess();
                }
                return m_dataProvider;
            }
        }

        public IUpdater SettingsUpdater { get; set; }

        public ApplicationSettings()
        {
        }

        public ApplicationSettings(string connectionName)
            : this()
        {
            m_settingConnectionName = connectionName;
        }

        public override void Initial()
        {
            if(SettingsUpdater == null)
            {
                SettingsUpdater = new DefaultSettingsUpdater(CreateDatabaseAccess());
            }
            SettingsUpdater.SettingsUpdateAction = changedKeys =>
            {
                LoadSettingValues();
            };
            SettingsUpdater.Start();
        }

        public override void Load()
        {
            LoadSettingValues();
        }

        private void LoadSettingValues()
        {
            var items = DataProvider.GetAll();

            Dictionary<string, ApplicationSettingItem> settingItems = new Dictionary<string, ApplicationSettingItem>(50);
            Dictionary<string, ApplicationSettingGroup> settingGroups = new Dictionary<string, ApplicationSettingGroup>(10);

            ApplicationSettingGroup group = null;
            foreach (ApplicationSettingItem item in items)
            {
                if(String.IsNullOrEmpty(item.GroupKey))
                {
                    item.GroupKey = DEFAULT_GROUP;
                }
                if (settingGroups.ContainsKey(item.GroupKey))
                {
                    group = settingGroups[item.GroupKey];
                }
                else
                {
                    group = new ApplicationSettingGroup
                    {
                        GroupKey = item.GroupKey,
                        GroupText = item.GroupText
                    };
                    settingGroups[group.GroupKey] = group;
                }
                group.AddItem(item);
                settingItems[item.SettingKey] = item;
            }

            Interlocked.Exchange(ref m_settingValues, settingItems);
            Interlocked.Exchange(ref m_settingGroups, settingGroups);
        }

        private ApplicationSettingsDataProvider CreateDatabaseAccess()
        {
            if (String.IsNullOrEmpty(m_settingConnectionName))
            {
                return ApplicationSettingsDataProvider.GetProvider();
            }
            else
            {
                return ApplicationSettingsDataProvider.GetProvider(m_settingConnectionName);
            }
        }

        public override string GetString(string key)
        {
            Guard.ArgumentNotNullOrEmpty(key, "key");

            var settingValues = SettingValues;
            if (settingValues.ContainsKey(key))
            {
                return settingValues[key].SettingValue;
            }
            return null;
        }
    }
}
