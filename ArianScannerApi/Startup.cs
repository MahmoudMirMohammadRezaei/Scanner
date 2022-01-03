using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Arian.Core;
using ArianScannerApi.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ArianScannerApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<SelfHost.IScannerService, ScannerService>();

            var environment = Configuration["env:dev"];
            string exePath = "";

            if (environment == true.ToString())
            {
                exePath = "D:\\ProjectRepo\\SelfHostWcfScannerService\\ArianScannerApi\\bin\\Debug\\netcoreapp3.1\\ArianScannerApi.exe";
            }
            else
            {
                exePath = "ArianScannerApi.exe";
            }
            //var result = ServiceOperationHelper.IsInstalledService("ArianScanner");

            RunBatHelper.ExecuteCommand($"sc.exe create \"ArianScanner\" binPath= {exePath}");

            RunBatHelper.ExecuteCommand($"sc start \"ArianScanner\"");
            RunBatHelper.ExecuteCommand($"sc config \"ArianScanner\" start=auto");

            //RunBatHelper.ExecuteCommand($"Set-Service -Name ArianScanner -StartupType Automatic");
            //RunBatHelper.ExecuteCommand($"Set-Service -Name ArianScanner -Status Running -PassThru");
            //RunBatHelper.ExecuteCommand($"Set-Service -InputObject ArianScanner -Status Stopped");
            //RunBatHelper.ExecuteCommand($"sc.exe delete ArianScanner");


            //ServiceOperationHelper.RunService("ArianScanner", environment);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
