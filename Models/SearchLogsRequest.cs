using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSSystem.Service.LoggerService.Models
{
    public class SearchLogsRequest
    {

        List<string> _Server_IDs;
        public List<string> Server_IDs { get { return _Server_IDs; } set { _Server_IDs = value; } }

        List<string> _ServerNames;
        public List<string> ServerNames { get { return _ServerNames; } set { _ServerNames = value; } }


        List<string> _Component_IDs;
        public List<string> Component_IDs { get { return _Component_IDs; } set { _Component_IDs = value; } }

        List<string> _ComponentNames;
        public List<string> ComponentNames { get { return _ComponentNames; } set { _ComponentNames = value; } }


        List<string> _LogName_IDs;
        public List<string> LogName_IDs { get { return _LogName_IDs; } set { _LogName_IDs = value; } }


        List<string> _LogNames;
        public List<string> LogNames { get { return _LogNames; } set { _LogNames = value; } }


        List<string> _LogType_IDs;
        public List<string> LogType_IDs { get { return _LogType_IDs; } set { _LogType_IDs = value; } }


        List<string> _LogTypes;
        public List<string> LogTypes { get { return _LogTypes; } set { _LogTypes = value; } }

        public long LogTimeFrom { get; set; }
        public long LogTimeTo { get; set; }
        public SearchLogsRequest()
        {
            _Server_IDs = new List<string>();
            _ServerNames = new List<string>();
            _Component_IDs = new List<string>();
            _ComponentNames = new List<string>();
            _LogType_IDs = new List<string>();
            _LogTypes = new List<string>();
            _LogName_IDs = new List<string>();
            _LogNames = new List<string>();
            LogTimeFrom = 0;
            LogTimeTo = 0;
        }
    }
}
