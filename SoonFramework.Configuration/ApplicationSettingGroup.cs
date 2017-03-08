using SoonFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    public class ApplicationSettingGroup
    {
        public ApplicationSettingGroup()
        {
            Items = new List<ApplicationSettingItem>(10);
        }

        public string GroupKey { get; set; }

        public string GroupText { get; set; }

        public List<ApplicationSettingItem> Items { get; private set; }

        public string this[string key]
        {
            get
            {
                Guard.ArgumentNotNullOrEmpty(key, "key");

                foreach(var item in Items)
                {
                    if(item.SettingKey == key)
                    {
                        return item.SettingValue;
                    }
                }
                return null;
            }
        }

        internal void AddItem(ApplicationSettingItem item)
        {
            Guard.ArgumentNotNull(item, "item");

            if (item.GroupKey == GroupKey)
            {
                Items.Add(item);
            }
        }
    }
}
