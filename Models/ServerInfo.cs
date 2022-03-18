using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSSystem.Service.LoggerService.Models
{
    public class ServerInfo
    {
        int _ID;
        public int ID { get { return _ID; } set { _ID = value; } }

        string _Name;
        public string Name { get { return _Name; } set { _Name = value; } }

        string _IPAddress;
        public string IPAddress { get { return _IPAddress; } set { _IPAddress = value; } }

        string _Url;
        public string Url { get { return _Url; } set { _Url = value; } }
    }
}
