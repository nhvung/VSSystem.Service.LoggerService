using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSSystem.Service.LoggerService.Models
{
    public class AddLogRequest
    {
        int _Server_ID;
        public int Server_ID { get { return _Server_ID; } set { _Server_ID = value; } }

        string _HostName;
        public string HostName { get { return _HostName; } set { _HostName = value; } }

        string _IPAddress;
        public string IPAddress { get { return _IPAddress; } set { _IPAddress = value; } }


        int _Component_ID;
        public int Component_ID { get { return _Component_ID; } set { _Component_ID = value; } }

        string _ComponentName;
        public string ComponentName { get { return _ComponentName; } set { _ComponentName = value; } }

        string _LogType;
        public string LogType { get { return _LogType; } set { _LogType = value; } }

        int _ILogType;
        public int ILogType { get { return _ILogType; } set { _ILogType = value; } }

        string _Contents;
        public string Contents { get { return _Contents; } set { _Contents = value; } }

        long _LogTicks;
        public long LogTicks { get { return _LogTicks; } set { _LogTicks = value; } }

        string _LogName;
        public string LogName { get { return _LogName; } set { _LogName = value; } }

        string _TagName;
        public string TagName { get { return _TagName; } set { _TagName = value; } }

        long _ServerTicks;
        public long ServerTicks { get { return _ServerTicks; } set { _ServerTicks = value; } }

        public AddLogRequest()
        {
            _Server_ID = -1;
            _Component_ID = -1;
            _LogType = string.Empty;
            _Contents = string.Empty;
            _LogTicks = 0;
            _LogName = string.Empty;
            _HostName = string.Empty;
            _ComponentName = string.Empty;
            _ILogType = 0;
            _TagName = string.Empty;
            _ServerTicks = 0;
        }
    }
}
