using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SoonFramework.Service.WindowsService
{
    [RunInstaller(true)]
    public partial class WindowsServiceInstaller : Installer
    {
        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        public WindowsServiceInstaller()
        {
            InitializeComponent();

            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.LocalSystem;
            processInstaller.Password = null;
            processInstaller.Username = null;

            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = ConfigurationManager.AppSettings["ServiceName"];

            var displayName = ConfigurationManager.AppSettings["ServiceDisplayName"];
            if (!string.IsNullOrEmpty(displayName))
                serviceInstaller.DisplayName = displayName;
            var serviceDescription = ConfigurationManager.AppSettings["ServiceDescription"];
            if (!string.IsNullOrEmpty(serviceDescription))
                serviceInstaller.Description = serviceDescription;

            var servicesDependedOn = new List<string> { "tcpip" };
            var servicesDependedOnConfig = ConfigurationManager.AppSettings["ServicesDependedOn"];

            if (!string.IsNullOrEmpty(servicesDependedOnConfig))
                servicesDependedOn.AddRange(servicesDependedOnConfig.Split(new char[] { ',', ';' }));

            serviceInstaller.ServicesDependedOn = servicesDependedOn.ToArray();

            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }
    }
}
