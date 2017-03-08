using SoonFramework.Configuration.DatabaseProviders;
using SoonFramework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    public abstract class ApplicationSettingsDataProvider
    {
        public static ApplicationSettingsDataProvider GetProvider()
        {
            DatabaseAccess dba = DatabaseAccessFactory.CreateDatabase();
            return GetProvider(dba);
        }

        public static ApplicationSettingsDataProvider GetProvider(string connectionStringName)
        {
            DatabaseAccess dba = DatabaseAccessFactory.CreateDatabase(connectionStringName);
            return GetProvider(dba);
        }

        static ApplicationSettingsDataProvider GetProvider(DatabaseAccess dba)
        {
            ApplicationSettingsDataProvider dataProvider = null;
            if (DatabaseAccessFactory.IsSQLServer(dba))
            {
                dataProvider = new SqlSettingsDataProvider();
            }
            if(dataProvider != null)
            {
                dataProvider.DatabaseAccess = dba;
            }
            return dataProvider;
        }

        public DatabaseAccess DatabaseAccess { get; private set; }

        public abstract IList<ApplicationSettingItem> GetAll();

        public abstract bool UpdateItem(ApplicationSettingItem settingItem);

        public abstract bool UpdateItems(IEnumerable<ApplicationSettingItem> settingItems);

        public abstract bool UpdateValue(string settingKey, string settingValue);

        public abstract IDictionary<string, long> GetSettingChangeValues();
    }
}
