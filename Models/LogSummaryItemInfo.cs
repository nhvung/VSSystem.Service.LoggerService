using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSSystem.Data.Monitoring.DTO;

namespace VSSystem.Service.LoggerService.Models
{
    public class LogSummaryItemInfo
    {

        int _Server_ID;
        public int Server_ID { get { return _Server_ID; } set { _Server_ID = value; } }

        string _HostName;
        public string HostName { get { return _HostName; } set { _HostName = value; } }

        int _Component_ID;
        public int Component_ID { get { return _Component_ID; } set { _Component_ID = value; } }

        string _ComponentName;
        public string ComponentName { get { return _ComponentName; } set { _ComponentName = value; } }

        string _LogName;
        public string LogName { get { return _LogName; } set { _LogName = value; } }

        int _ILogType;
        public int ILogType { get { return _ILogType; } set { _ILogType = value; } }

        string _LogType;
        public string LogType { get { return _LogType; } set { _LogType = value; } }

        int _Total;
        public int Total { get { return _Total; } set { _Total = value; } }

        public LogSummaryItemInfo()
        { }
        public LogSummaryItemInfo(LogFilterDTO ftObj)
        {
            _Server_ID = ftObj.Server_ID;
            _Component_ID = ftObj.Component_ID;
            _LogName = ftObj.Name;
            _ILogType = ftObj.Type;
            _Total = ftObj.Total;
        }
    }
}
