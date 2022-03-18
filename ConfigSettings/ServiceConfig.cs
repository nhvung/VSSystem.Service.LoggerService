using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSSystem.Service.LoggerService
{
    public class ServiceConfig
    {
        #region pools

        static string _pools_add_logs_folder;
        public static string pools_add_logs_folder { get { return _pools_add_logs_folder; } set { _pools_add_logs_folder = value; } }

        #endregion

        #region process_logs_worker

        static bool _process_logs_worker_enable = true;
        public static bool process_logs_worker_enable { get { return _process_logs_worker_enable; } set { _process_logs_worker_enable = value; } }

        static int _process_logs_worker_interval = 30;
        public static int process_logs_worker_interval { get { return _process_logs_worker_interval; } set { _process_logs_worker_interval = value; } }

        static int _process_logs_worker_number_of_threads = 1;
        public static int process_logs_worker_number_of_threads { get { return _process_logs_worker_number_of_threads; } set { _process_logs_worker_number_of_threads = value; } }

        #endregion

    }
}
