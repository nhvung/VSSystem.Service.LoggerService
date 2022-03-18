using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSSystem.Hosting.Webs.Controllers;
using VSSystem.Hosting;
using VSSystem.Service.LoggerService.Models;
using System.Text;
using VSSystem.Data.Monitoring.BLL;
using VSSystem.Hosting.Webs.Response;
using VSSystem.Data.Monitoring.DTO;
using VSSystem.Data.Monitoring.Define;
using VSSystem.Extensions;
using Newtonsoft.Json;
using System.IO;
using VSSystem.Service.LoggerService.Extensions;
using VSSystem.Data.Monitoring.Filters;

namespace VSSystem.Service.LoggerService.Controllers
{
    public class LogController : AController
    {
        const string _COMMON_LOG_TABLE_NAME = "common_logs";
        const string _BIG_LOG_TABLE_NAME = "big_logs";

        const string _TOOL_LOG_TABLE_NAME = "tool_logs";
        const string _TOOL_AGENCY_TABLE_NAME = "tool_agencies";
        const string _TOOL_USER_TABLE_NAME = "tool_users";

        public LogController() : base("LogController", VSHost.SERVICE_NAME, VSHost.StaticLogger)
        {
        }
        protected override Task _ProcessApiContext(string path, string queryString)
        {
            if (IsPost())
            {
                if (path.Equals($"{_ServicePath}api/log/add/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return AddLog();
                }
                else if (path.Equals($"{_ServicePath}api/log/registerserver/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RegisterServer();
                }
                else if (path.Equals($"{_ServicePath}api/log/registercomponent/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return RegisterComponent();
                }
                else if (path.Equals($"{_ServicePath}api/log/getlogs/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLogs(ServerBLL.DEFAULT_TABLE_NAME, ComponentBLL.DEFAULT_TABLE_NAME, _COMMON_LOG_TABLE_NAME);
                }
                else if (path.Equals($"{_ServicePath}api/log/getlogdetail/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLogDetail(ServerBLL.DEFAULT_TABLE_NAME, ComponentBLL.DEFAULT_TABLE_NAME, _COMMON_LOG_TABLE_NAME);
                }
                else if (path.Equals($"{_ServicePath}api/log/gettoollogs/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLogs(_TOOL_AGENCY_TABLE_NAME, _TOOL_USER_TABLE_NAME, _TOOL_LOG_TABLE_NAME);
                }
                else if (path.Equals($"{_ServicePath}api/log/gettoollogdetail/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetLogDetail(_TOOL_AGENCY_TABLE_NAME, _TOOL_USER_TABLE_NAME, _TOOL_LOG_TABLE_NAME);
                }
            }
            else if(IsGet())
            {
                if (path.Equals($"{_ServicePath}api/log/getfilters/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetFilters(ServerBLL.DEFAULT_TABLE_NAME, ComponentBLL.DEFAULT_TABLE_NAME, _COMMON_LOG_TABLE_NAME);
                }
                else if (path.Equals($"{_ServicePath}api/log/gettoolfilters/", StringComparison.InvariantCultureIgnoreCase))
                {
                    return GetFilters(_TOOL_AGENCY_TABLE_NAME, _TOOL_USER_TABLE_NAME, _TOOL_LOG_TABLE_NAME);
                }
            }
            return base._ProcessApiContext(path, queryString);
        }

        async Task AddLog()
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;
                var requestObj = await this.GetRequestObject<AddLogRequest>(Encoding.UTF8);

                if (requestObj != null)
                {
                    requestObj.ServerTicks = utcNow.Ticks;
                    if(requestObj.LogTicks <= 0)
                    {
                        requestObj.LogTicks = utcNow.Ticks;
                    }

                    _ = Task.Run(() => _MoveToLogPools(requestObj));
                    return;
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }
       
        async Task RegisterServer()
        {
            try
            {
                var requestObj = await this.GetRequestObject<ServerInfo>(Encoding.UTF8);
                if (requestObj != null)
                {
                    var serverObj = LogExtension.GetServer(ServerBLL.DEFAULT_TABLE_NAME, requestObj);
                    if (serverObj != null)
                    {
                        var responseObj = new ServerInfo
                        {
                            ID = serverObj.ID,
                            Name = serverObj.Name,
                            IPAddress = serverObj.IPAddress,
                            Url = serverObj.HostUrl,
                        };

                        await this.ResponseJsonAsync(responseObj, System.Net.HttpStatusCode.OK);
                    }
                    else
                    {
                        await this.ResponseJsonAsync(DefaultResponse.InvalidParameters, System.Net.HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    await this.ResponseJsonAsync(DefaultResponse.InvalidParameters, System.Net.HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                await this.ResponseJsonAsync(DefaultResponse.ServerError, System.Net.HttpStatusCode.BadRequest);
            }
        }

        async Task RegisterComponent()
        {
            try
            {
                var requestObj = await this.GetRequestObject<ComponentInfo>(Encoding.UTF8);
                if (requestObj != null)
                {
                    DateTime utcNow = DateTime.UtcNow;
                    long lUtcNow = utcNow.ToInt64();
                    var componentObj = LogExtension.GetComponent(ComponentBLL.DEFAULT_TABLE_NAME, requestObj);

                    if (componentObj != null)
                    {
                        var responseObj = new ComponentInfo
                        {
                            ID = componentObj.ID,
                            Server_ID = componentObj.Server_ID,
                            Name = componentObj.Name,
                        };

                        await this.ResponseJsonAsync(responseObj, System.Net.HttpStatusCode.OK);
                    }
                    else
                    {
                        await this.ResponseJsonAsync(DefaultResponse.InvalidParameters, System.Net.HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    await this.ResponseJsonAsync(DefaultResponse.InvalidParameters, System.Net.HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                await this.ResponseJsonAsync(DefaultResponse.ServerError, System.Net.HttpStatusCode.BadRequest);
            }
        }

        void _MoveToLogPools(AddLogRequest requestObj)
        {

            try
            {
                if(!string.IsNullOrWhiteSpace(ServiceConfig.pools_add_logs_folder))
                {
                    DateTime utcNow = DateTime.UtcNow;
                    DirectoryInfo folder = new DirectoryInfo(ServiceConfig.pools_add_logs_folder + "/" + string.Format("{0:yyyy-MM-dd}/H_{0:HH}", utcNow));
                    if(!folder.Exists)
                    {
                        folder.Create();
                        
                    }
                    string json = JsonConvert.SerializeObject(requestObj);
                    string guid = Guid.NewGuid().ToString();
                    System.IO.File.WriteAllText(folder.FullName + "/" + guid + ".json", json, Encoding.UTF8);
                    System.IO.File.WriteAllBytes(folder.FullName + "/" + guid + ".sign", new byte[0]);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }

        async Task GetFilters(string serverTableName, string componentTableName, string logsTableName)
        {
            try
            {
                var serverObjs = ServerBLL.GetAllData(serverTableName);
                var mServer = serverObjs.ToDictionary(ite => ite.ID);
                var componentObjs = ComponentBLL.GetAllData(componentTableName);
                var mComponent = componentObjs.ToDictionary(ite => ite.ID);

                var filterObjs = LogBLL.GetFilters(logsTableName, 0, 0);

                var summaryObjs = new List<LogSummaryItemInfo>();
                foreach(var ftObj in filterObjs)
                {
                    LogSummaryItemInfo summaryObj = new LogSummaryItemInfo(ftObj);
                    if(mServer.ContainsKey(ftObj.Server_ID))
                    {
                        summaryObj.HostName = mServer[ftObj.Server_ID].Name;
                    }
                    if (mComponent.ContainsKey(ftObj.Component_ID))
                    {
                        summaryObj.ComponentName = mComponent[ftObj.Component_ID].Name;
                    }
                    summaryObj.LogType = ((ELogType)ftObj.Type).ToString();

                    summaryObjs.Add(summaryObj);
                }

                await this.ResponseJsonAsync(summaryObjs, System.Net.HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                this.LogError(ex);
                await this.ResponseJsonAsync(DefaultResponse.ServerError, System.Net.HttpStatusCode.BadRequest);
            }
        }
        async Task GetLogs(string serverTableName, string componentTableName, string logsTableName)
        {
            try
            {
                var requestObj = await this.GetRequestObject<SearchLogsRequest>(Encoding.UTF8);
                if(requestObj != null)
                {
                    LogFilter logFt = new LogFilter();

                    if(requestObj.LogTimeFrom > 0)
                    {
                        DateTime dtBegin = requestObj.LogTimeFrom.ToDateTime();
                        logFt.BeginTicks = dtBegin.Ticks;
                    }
                    if (requestObj.LogTimeTo > 0)
                    {
                        DateTime dtEnd = requestObj.LogTimeTo.ToDateTime();
                        logFt.EndTicks = dtEnd.Ticks;
                    }

                    if(requestObj.LogName_IDs?.Count > 0)
                    {
                        logFt.FieldsValuesItems.Add(new LogFilter.FieldsValues() { 
                            Fields = new List<string>() { "Server_ID", "Component_ID", "Type", "Name" },
                            
                            Values = requestObj.LogName_IDs.Select(ite => "(" + string.Join(",", ite.Split('-').Select(ite1 => "'" + ite1 + "'")) + ")").ToList()
                        });
                    }
                    else if (requestObj.LogType_IDs?.Count > 0)
                    {
                        logFt.FieldsValuesItems.Add(new LogFilter.FieldsValues()
                        {
                            Fields = new List<string>() { "Server_ID", "Component_ID", "Type" },
                            Values = requestObj.LogType_IDs.Select(ite => "(" + ite.Replace("-", ",") + ")").ToList()
                        });
                    }
                    else if (requestObj.Component_IDs?.Count > 0)
                    {
                        logFt.FieldsValuesItems.Add(new LogFilter.FieldsValues()
                        {
                            Fields = new List<string>() { "Server_ID", "Component_ID" },
                            Values = requestObj.Component_IDs.Select(ite => "(" + ite.Replace("-", ",") + ")").ToList()
                        });
                    }
                    else if (requestObj.Server_IDs?.Count > 0)
                    {
                        logFt.FieldsValuesItems.Add(new LogFilter.FieldsValues()
                        {
                            Fields = new List<string>() { "Server_ID" },
                            Values = requestObj.Server_IDs.Select(ite => "(" + ite.Replace("-", ",") + ")").ToList()
                        });
                    }

                    int pageSize = 0, pageNumber = 0;
                    int.TryParse(HttpContext.Request.Query["pageSize"], out pageSize);
                    int.TryParse(HttpContext.Request.Query["pageNumber"], out pageNumber);
                    if(pageSize <= 0)
                    {
                        pageSize = 100;
                    }
                    if(pageNumber<= 0)
                    {
                        pageNumber = 1;
                    }

                    List<KeyValuePair<string, string>> selectedFields = new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("ID",""),
                        new KeyValuePair<string, string>("Server_ID",""),
                        new KeyValuePair<string, string>("Component_ID",""),
                        new KeyValuePair<string, string>("Type",""),
                        new KeyValuePair<string, string>("Name",""),
                        new KeyValuePair<string, string>("left(Contents, 100)","Contents"),
                        new KeyValuePair<string, string>("CreatedTicks",""),
                        new KeyValuePair<string, string>("Tag",""),
                        new KeyValuePair<string, string>("ServerTicks",""),
                    };
                    var logResult = LogBLL.PageSplitSearch(logsTableName, logFt, pageSize, pageNumber, selectedFields, new List<string> { "CreatedTicks desc" });
                    if(logResult.TotalRecords > 0)
                    {
                        var serverObjs = ServerBLL.GetAllData(serverTableName);
                        var mServer = serverObjs.ToDictionary(ite => ite.ID);
                        var componentObjs = ComponentBLL.GetAllData(componentTableName);
                        var mComponent = componentObjs.ToDictionary(ite => ite.ID);


                        var responseObj = new {
                            logResult.TotalPages,
                            logResult.TotalRecords,
                            logResult.PageNumber,
                            logResult.PageSize,
                            Records = logResult.Records.Select(ite => new {
                                ite.ID,
                                ite.Server_ID,
                                HostName = mServer?.ContainsKey(ite.Server_ID) ?? false ? mServer[ite.Server_ID].Name : "-",
                                ite.Component_ID,
                                ComponentName = mComponent?.ContainsKey(ite.Component_ID) ?? false ? mComponent[ite.Component_ID].Name : "-",
                                ILogType = ite.Type,
                                LogType = ((ELogType)ite.Type).ToString(),
                                LogName = ite.Name,
                                ite.Contents,
                                ite.CreatedTicks
                            }).ToList()
                        };

                        await this.ResponseJsonAsync(responseObj, System.Net.HttpStatusCode.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }
        async Task GetLogDetail(string serverTableName, string componentTableName, string logsTableName)
        {
            try
            {
                long id;
                long.TryParse(HttpContext.Request.Query["id"], out id);
                if(id > 0)
                {
                    LogFilter logFt = new LogFilter();
                    logFt.IDs.Add(id);

                    var logObjs = LogBLL.Search(logsTableName, logFt);
                    if(logObjs?.Count > 0)
                    {
                        var logObj = logObjs.FirstOrDefault();
                        if(logObj != null)
                        {
                            var serverObjs = ServerBLL.GetAllData(serverTableName);
                            var mServer = serverObjs.ToDictionary(ite => ite.ID);
                            var componentObjs = ComponentBLL.GetAllData(componentTableName);
                            var mComponent = componentObjs.ToDictionary(ite => ite.ID);

                            var responseObj = new
                            {
                                logObj.ID,
                                logObj.Server_ID,
                                HostName = mServer?.ContainsKey(logObj.Server_ID) ?? false ? mServer[logObj.Server_ID].Name : "-",
                                logObj.Component_ID,
                                ComponentName = mComponent?.ContainsKey(logObj.Component_ID) ?? false ? mComponent[logObj.Component_ID].Name : "-",
                                ILogType = logObj.Type,
                                LogType = ((ELogType)logObj.Type).ToString(),
                                LogName = logObj.Name,
                                logObj.Contents,
                                logObj.CreatedTicks
                            };

                            await this.ResponseJsonAsync(responseObj, System.Net.HttpStatusCode.OK);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }
    }
}
