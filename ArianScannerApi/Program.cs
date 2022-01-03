using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace ArianScannerApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.Configure<EventLogSettings>(config =>
                        {
                            config.LogName = "Arian Scanner API Service";
                            config.SourceName = "Arian Scanner API Service Source";
                        });
                    })
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    })
                    //.ConfigureWebHost(config =>
                    //{
                    //    //config.UseUrls("http://*:5050");
                    //    config.UseUrls("http://*:18580");
                    //})
            .UseWindowsService();
    }
}
