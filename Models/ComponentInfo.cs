using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSSystem.Service.LoggerService.Models
{
    public class ComponentInfo
    {

        int _ID;
        public int ID { get { return _ID; } set { _ID = value; } }

        string _Name;
        public string Name { get { return _Name; } set { _Name = value; } }

        byte _Type;
        public byte Type { get { return _Type; } set { _Type = value; } }

        int _Server_ID;
        public int Server_ID { get { return _Server_ID; } set { _Server_ID = value; } }

        string _RootName;
        public string RootName { get { return _RootName; } set { _RootName = value; } }

        int _HttpPort;
        public int HttpPort { get { return _HttpPort; } set { _HttpPort = value; } }
        int _HttpsPort;
        public int HttpsPort { get { return _HttpsPort; } set { _HttpsPort = value; } }
    }
}
