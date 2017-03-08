using System;
using System.Collections.Generic;
using System.Configuration.Install;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Service.WindowsService
{
    public static class SelfInstaller
    {
        public static bool InstallMe(string exePath)
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UninstallMe(string exePath)
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new string[] { "/u", exePath });
            }
            catch
            {
                return false;
            }
            return true;
        }
    }
}
