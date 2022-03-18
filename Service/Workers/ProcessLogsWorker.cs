using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSSystem.ServiceProcess.Workers;
using VSSystem.ServiceProcess.Extensions;
using VSSystem.Logger;
using System.IO;
using VSSystem.Service.LoggerService.Models;
using VSSystem.Data.Monitoring.DTO;
using VSSystem.Data.Monitoring.Define;
using VSSystem.Extensions;
using VSSystem.Data.Monitoring.BLL;
using System.Text;
using VSSystem.Service.LoggerService.Extensions;
using Newtonsoft.Json;

namespace VSSystem.Service.LoggerService.Service.Workers
{
    public class ProcessLogsWorker : PoolWorker
    {
        const string _COMMON_LOG_TABLE_NAME = "common_logs";
        const string _BIG_LOG_TABLE_NAME = "big_logs";
        const int _BIG_LOG_CONTENT_LENGTH = 2000000;

        const string _TOOL_AGENCY_TABLE_NAME = "tool_agencies";
        const string _TOOL_USER_TABLE_NAME = "tool_users";
        const string _TOOL_LOG_TABLE_NAME = "tool_logs";
        public ProcessLogsWorker(bool enabled, string serviceName, int interval, int numberOfThreads, ALogger logger) 
            : base(new IntervalWorkerStartInfo("ProcessLogsWorker", enabled, serviceName, interval, numberOfThreads, EWorkerIntervalUnit.Second), logger)
        {
            _initPoolFolderAction = delegate 
            {
                _poolFolder = new System.IO.DirectoryInfo(ServiceConfig.pools_add_logs_folder);
            };
            _signFileLevel = 2;
#if DEBUG
            //_deleteSignFileWhenFinish = false;
#endif
        }

        protected override void ProcessSignFile(FileInfo signFile)
        {

            try
            {
                FileInfo jsonFile = new FileInfo(signFile.Directory.FullName + "/" + signFile.Name.Replace(signFile.Extension, ".json"));
                if (jsonFile.Exists)
                {
                    string json = File.ReadAllText(jsonFile.FullName, Encoding.UTF8);
                    AddLogRequest requestObj = JsonConvert.DeserializeObject<AddLogRequest>(json);
                    if(requestObj != null)
                    {
                        if (requestObj.LogName.Equals("CaptureTool.log", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _ = Task.Run(() => _AddToolLog(requestObj));
                        }
                        else
                        {
                            _ = Task.Run(() => _AddCommonLog(requestObj));
                        }
                    }

                    if(_deleteSignFileWhenFinish)
                    {

                        try
                        {
                            jsonFile.Attributes = FileAttributes.Archive;
                            jsonFile.Delete();
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                _ = this.LogErrorAsync(ex);
            }
        }
        
        
        void _AddToolLog(AddLogRequest requestObj)
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;

                if (requestObj != null)
                {
                    var logObj = new LogDTO();
                    logObj.Contents = requestObj.Contents;
                    logObj.CreatedTicks = requestObj.LogTicks;
                    logObj.Name = requestObj.LogName;
                    logObj.Tag = requestObj.TagName;
                    logObj.Server_ID = requestObj.Server_ID;
                    logObj.Component_ID = requestObj.Component_ID;
                    logObj.ServerTicks = requestObj.ServerTicks;

                    ELogType logType;
                    if (!Enum.TryParse(requestObj.LogType, true, out logType))
                    {
                        logType = ELogType.None;
                    }
                    logObj.Type = (byte)logType;

                    if (requestObj.Server_ID <= 0 && !string.IsNullOrWhiteSpace(requestObj.HostName))
                    {
                        var serverObj = LogExtension.GetServer(_TOOL_AGENCY_TABLE_NAME, new ServerInfo()
                        {
                            Name = requestObj.HostName,
                        });
                        if (serverObj != null)
                        {
                            logObj.Server_ID = serverObj.ID;
                        }
                    }

                    if (requestObj.Component_ID <= 0 && !string.IsNullOrWhiteSpace(requestObj.ComponentName))
                    {
                        var componentObj = LogExtension.GetComponent(_TOOL_USER_TABLE_NAME, new ComponentInfo()
                        {
                            Name = requestObj.ComponentName + " (" + requestObj.IPAddress + ")",
                            Server_ID = logObj.Server_ID
                        });
                        if (componentObj != null)
                        {
                            logObj.Component_ID = componentObj.ID;
                        }
                    }

                    try
                    {
                        var clearContentsBytes = Convert.FromBase64String(logObj.Contents);
                        logObj.Contents = Encoding.UTF8.GetString(clearContentsBytes);
                        LogBLL.Insert(_TOOL_LOG_TABLE_NAME, logObj);
                    }
                    catch { }
                }

            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }
        void _AddCommonLog(AddLogRequest requestObj)
        {
            try
            {
                DateTime utcNow = DateTime.UtcNow;

                if (requestObj != null)
                {
                    var logObj = new LogDTO();
                    logObj.Contents = requestObj.Contents;
                    logObj.CreatedTicks = requestObj.LogTicks;
                    logObj.Name = requestObj.LogName;
                    logObj.Tag = requestObj.TagName;
                    logObj.Server_ID = requestObj.Server_ID;
                    logObj.Component_ID = requestObj.Component_ID;
                    logObj.ServerTicks = requestObj.ServerTicks;

                    ELogType logType;
                    if (!Enum.TryParse(requestObj.LogType, true, out logType))
                    {
                        logType = ELogType.None;
                    }
                    logObj.Type = (byte)logType;

                    if (requestObj.Server_ID <= 0 && !string.IsNullOrWhiteSpace(requestObj.HostName))
                    {
                        var serverObj = LogExtension.GetServer(ServerBLL.DEFAULT_TABLE_NAME, new ServerInfo()
                        {
                            Name = requestObj.HostName,
                            IPAddress = requestObj.IPAddress
                        });
                        if (serverObj != null)
                        {
                            logObj.Server_ID = serverObj.ID;
                        }
                    }

                    if (requestObj.Component_ID <= 0 && !string.IsNullOrWhiteSpace(requestObj.ComponentName))
                    {
                        var componentObj = LogExtension.GetComponent(ComponentBLL.DEFAULT_TABLE_NAME, new ComponentInfo()
                        {
                            Name = requestObj.ComponentName,
                            Server_ID = logObj.Server_ID
                        });
                        if (componentObj != null)
                        {
                            logObj.Component_ID = componentObj.ID;
                        }
                    }

                    try
                    {
                        if (logObj.Server_ID > 0 && logObj.Component_ID > 0)
                        {
                            if (logObj.Contents.Length >= _BIG_LOG_CONTENT_LENGTH)
                            {
                                LogBLL.Insert(_BIG_LOG_TABLE_NAME, logObj);
                            }
                            else
                            {
                                LogBLL.Insert(_COMMON_LOG_TABLE_NAME, logObj);
                            }
                        }
                    }
                    catch { }
                }

            }
            catch (Exception ex)
            {
                this.LogError(ex);
            }
        }
    }
}
