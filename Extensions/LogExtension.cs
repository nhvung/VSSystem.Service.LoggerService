using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSSystem.Data.Monitoring.BLL;
using VSSystem.Data.Monitoring.Define;
using VSSystem.Data.Monitoring.DTO;
using VSSystem.Extensions;
using VSSystem.Service.LoggerService.Models;

namespace VSSystem.Service.LoggerService.Extensions
{
    public class LogExtension
    {
        static object _lockObj;
        public static ServerDTO GetServer(string tableName, ServerInfo requestObj)
        {
            if (_lockObj == null)
            {
                _lockObj = new object();
            }
            DateTime utcNow = DateTime.UtcNow;
            long lUtcNow = utcNow.ToInt64();
            ServerDTO result = default;
            try
            {
                if (!string.IsNullOrWhiteSpace(requestObj.IPAddress) || !string.IsNullOrWhiteSpace(requestObj.Name))
                {
                    lock (_lockObj)
                    {
                        if (!string.IsNullOrWhiteSpace(requestObj.IPAddress))
                        {
                            result = ServerBLL.GetServerByIPAddress(tableName, requestObj.IPAddress);
                        }
                        if (result == null && !string.IsNullOrWhiteSpace(requestObj.Name))
                        {
                            result = ServerBLL.GetServerByName(tableName, requestObj.Name);
                        }
                        if (result == null)
                        {
                            result = new ServerDTO();
                            result.HostUrl = requestObj.Url;
                            result.IPAddress = requestObj.IPAddress;
                            result.Name = requestObj.Name;
                            result.Status = (byte)EStatus.Active;
                            result.CreatedDateTime = lUtcNow;
                            result.UpdatedDateTime = lUtcNow;

                            try
                            {
                                ServerBLL.Insert(tableName, result);
                                result.ID = ServerBLL.GetMaxValue<int>(tableName, "ID");
                            }
                            catch { }
                        }
                    }
                }

            }
            catch { }
            return result;
        }
        public static ComponentDTO GetComponent(string tableName, ComponentInfo requestObj)
        {
            if (_lockObj == null)
            {
                _lockObj = new object();
            }
            DateTime utcNow = DateTime.UtcNow;
            long lUtcNow = utcNow.ToInt64();
            ComponentDTO result = default;
            try
            {
                if (!string.IsNullOrWhiteSpace(requestObj.Name))
                {
                    lock (_lockObj)
                    {
                        var componentObjs = ComponentBLL.GetComponentByName(tableName, requestObj.Name, requestObj.Server_ID);
                        result = componentObjs?.FirstOrDefault();

                        if (result == null)
                        {
                            result = new ComponentDTO();
                            result.Server_ID = requestObj.Server_ID;
                            result.Status = (byte)EStatus.Active;
                            result.Name = requestObj.Name;
                            result.CreatedDateTime = result.UpdatedDateTime = lUtcNow;


                            try
                            {
                                ComponentBLL.Insert(tableName, result);
                                result.ID = ComponentBLL.GetMaxValue<int>(tableName, "ID");
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
            return result;
        }
    }
}
