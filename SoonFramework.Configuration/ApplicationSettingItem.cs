using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    public class ApplicationSettingItem
    {
        public Guid SettingID { get; set; }

        public string SettingKey { get; set; }

        public string SettingText { get; set; }

        public string SettingValue { get; set; }

        public string GroupKey { get; set; }

        public string GroupText { get; set; }

        public bool Enabled { get; set; }
    }
}
