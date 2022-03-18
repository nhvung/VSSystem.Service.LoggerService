using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using VSSystem.Hosting;
using VSSystem.Logger;
using VSSystem.Service.LoggerService.Service;

namespace VSSystem.Service.LoggerService
{
    public class VSHost : AHost
    {
        public const string PRIVATE_KEY = "304c3357-3376-7645-2164-336e63332139";
        public static string SERVICE_NAME = null;
        static Task Main(string[] args)
        {
            return new VSHost("LoggerService", 2061, 2062, true, null, PRIVATE_KEY)
                .RunAsync(args);
        }
        public VSHost(string name, int httpPort, int httpsPort, bool useOnlineConfiguration, string rootName, string privateKey) 
            : base(name, httpPort, httpsPort, useOnlineConfiguration, rootName, privateKey)
        {
            if (!string.IsNullOrWhiteSpace(rootName))
            {
                if (string.IsNullOrWhiteSpace(SERVICE_NAME))
                {
                    SERVICE_NAME = _name;
                }
            }
        }

        protected override void _UseStartup(IWebHostBuilder webHostBuilder)
        {
            webHostBuilder.UseStartup<VSStartup>();
        }

        protected override void _InitInjectionServices()
        {
            _AddInjectedServices(new VSService(_name, _server_ID, _rootName, _privateKey, _logger));
        }
        public static ALogger StaticLogger = null;
        protected override void _InitLogger()
        {
            base._InitLogger();
            StaticLogger = _logger;
        }
    }
}
