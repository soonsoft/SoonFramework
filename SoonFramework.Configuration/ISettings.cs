using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Configuration
{
    /// <summary>
    /// 程序设置接口
    /// </summary>
    public interface ISettings : IConfiguration
    {
        void Initial();

        void Load();
    }
}
