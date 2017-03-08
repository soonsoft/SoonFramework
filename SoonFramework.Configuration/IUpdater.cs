using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    /// <summary>
    /// 设置信息自动更新器
    /// </summary>
    public interface IUpdater : IDisposable
    {
        /// <summary>
        /// 系统设置更新委托
        /// </summary>
        Action<string[]> SettingsUpdateAction { get; set; }

        /// <summary>
        /// 开始
        /// </summary>
        void Start();
    }
}
