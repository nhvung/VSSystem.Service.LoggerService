using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VSSystem.Configuration;
using VSSystem.Logger;
using VSSystem.Security;
using VSSystem.Service.LoggerService.Service.Workers;
using VSSystem.ServiceProcess;

namespace VSSystem.Service.LoggerService.Service
{
    public class VSService : AService
    {
        public VSService(string name, int server_ID, string rootComponentName, string privateKey, ALogger logger)
                : base(name, server_ID, rootComponentName, privateKey, new string[]
                {
                    "pools",
                    "process_logs_worker"
                }, logger)
        {
        }
        async protected override Task _InitConfiguration()
        {
            await base._InitConfiguration();

            try
            {
                CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
                Thread.CurrentThread.CurrentCulture = culture;

                var dbIni = _ini;

                Func<string, string> decryptFunc = ite => Cryptography.DecryptFromHexString<string>(ite, _privateKey);
                Data.Monitoring.Variables.InitFromIniFile(dbIni.ReadValue<string>, decryptFunc);

                _ini.ReadAllStaticConfigs<ServiceConfig>(_defaultSections);

                if (string.IsNullOrWhiteSpace(ServiceConfig.pools_add_logs_folder))
                {
                    ServiceConfig.pools_add_logs_folder = WorkingFolder.FullName + "/Pools/Logs";
                }
            }
            catch { }
        }
        protected override void _InitializeWorkers()
        {
            AddWorker(new ProcessLogsWorker(ServiceConfig.process_logs_worker_enable, _name, ServiceConfig.process_logs_worker_interval, ServiceConfig.process_logs_worker_number_of_threads, _logger));
        }
    }
}
