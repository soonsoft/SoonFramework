using SoonFramework.Core;
using SoonFramework.Data;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration.DatabaseProviders
{
    public class SqlSettingsDataProvider : ApplicationSettingsDataProvider
    {

        private const string LOAD_SQL = "SELECT SettingID, SettingKey, SettingText, SettingValue, GroupKey, GroupText, [Enabled] FROM sys_Settings";
        private const string UPDATE_SQL = "UPDATE [sys_Settings] SET SettingKey = {SettingKey}, SettingText = {SettingText}, SettingValue = {SettingValue}, GroupKey = {GroupKey}, GroupText = {GroupText}, [Enabled] = {Enabled} WHERE SettingID = {SettingID}";
        private const string UPDATE_VALUE_SQL = "UPDATE [sys_Settings] SET SettingValue = {0} WHERE SettingKey = {1}";
        private const string QUERY_SETTING_CHANGE = "SELECT * FROM [sys_Settings_ChangeDependency]";

        public override IList<ApplicationSettingItem> GetAll()
        {
            DatabaseAccess dba = DatabaseAccess;
            DbCommand cmd = dba.CreateCommand(LOAD_SQL);
            List<ApplicationSettingItem> settingItems = dba.ExecuteReader<List<ApplicationSettingItem>>(
                cmd, (d, r) => 
                {
                    d.Add(new ApplicationSettingItem
                    {
                        SettingID = (Guid)r["SettingID"],
                        SettingKey = (string)r["SettingKey"],
                        SettingText = r.GetStringOrNull("SettingText"),
                        SettingValue = r.GetStringOrNull("SettingValue"),
                        GroupKey = r.GetStringOrNull("GroupKey"),
                        GroupText = r.GetStringOrNull("GroupText"),
                        Enabled = r.GetBooleanOrDefault("Enabled", false).Value
                    });
                });
            return settingItems;
        }

        public override bool UpdateItem(ApplicationSettingItem settingItem)
        {
            Guard.ArgumentNotNull(settingItem, "item");

            DatabaseAccess dba = DatabaseAccess;
            int affectRows = dba.ExecuteNonQuery(UPDATE_SQL, settingItem);

            return affectRows > 0;
        }

        public override bool UpdateItems(IEnumerable<ApplicationSettingItem> settingItems)
        {
            Guard.ArgumentNotNull(settingItems, "settingItems");
            if(!settingItems.Any())
            {
                return false;
            }

            DatabaseAccess dba = DatabaseAccess;
            int affectRows = dba.ExecuteNonQueryMultiple(UPDATE_SQL, settingItems);

            return affectRows > 0;
        }

        public override bool UpdateValue(string settingKey, string settingValue)
        {
            Guard.ArgumentNotNullOrEmpty(settingKey, "settingKey");
            Guard.ArgumentNotNullOrEmpty(settingValue, "settingValue");

            DatabaseAccess dba = DatabaseAccess;
            int affectRows = dba.ExecuteNonQuery(
                UPDATE_VALUE_SQL, 
                new object[] { settingValue, settingKey });

            return affectRows > 0;
        }

        public override IDictionary<string, long> GetSettingChangeValues()
        {
            DatabaseAccess dba = DatabaseAccess;
            Dictionary<string, long> values = dba.ExecuteReader<Dictionary<string, long>>(
                QUERY_SETTING_CHANGE, (d, r) => 
                {
                    d.Add(r.GetStringOrNull("SettingKey"), r.GetLongOrDefault("ChangeValue", 0).Value);
                });
            return values;
        }
    }
}
