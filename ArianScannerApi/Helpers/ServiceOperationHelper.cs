using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace ArianScannerApi.Helpers
{
    public class ServiceOperationHelper
    {
        public static void RunService(string serviceName, string envi)
        {
            string exePath = "";

            if (envi == true.ToString())
            {
                exePath = "D:\\ProjectRepo\\SelfHostWcfScannerService\\ArianScannerApi\\bin\\Debug\\netcoreapp3.1\\ArianScannerApi.exe";
            }
            else
            {
                exePath = "ArianScannerApi.exe";
            }
            ServiceController ctl = ServiceController.GetServices()
                                                     .FirstOrDefault(s => s.ServiceName == serviceName);
            //var res = IsInstalledService(serviceName);

            if (ctl == null)
            {
                //RunBatHelper.ExecuteCommand($"sc.exe create \"ArianScanner\" binPath={exePath}");
                //ServiceController ctlinstalled = ServiceController.GetServices()
                //                         .FirstOrDefault(s => s.ServiceName == serviceName);
                //ctlinstalled.Start();
                InstallRunService(exePath);
            }
            else
            {
                if (ctl.Status == ServiceControllerStatus.Stopped)
                {
                    RunBatHelper.ExecuteCommand($"Set-Service -InputObject ArianScanner -Status Stopped");
                    RunBatHelper.ExecuteCommand($"sc.exe delete \"ArianScanner\"");
                    //RunBatHelper.ExecuteCommand($"sc.exe create \"ArianScanner\" binPath={exePath}");
                    //ServiceController ctlinstalled = ServiceController.GetServices()
                    //                         .FirstOrDefault(s => s.ServiceName == serviceName);
                    //ctlinstalled.Start();
                    ServiceController ctlDeleted = ServiceController.GetServices()
                                             .FirstOrDefault(s => s.ServiceName == serviceName);
                    InstallRunService(exePath);

                }
                else
                {
                    RunBatHelper.ExecuteCommand($"Set-Service -InputObject ArianScanner -Status Stopped");
                    RunBatHelper.ExecuteCommand($"sc.exe delete \"ArianScanner\"");
                    ServiceController ctlFind = ServiceController.GetServices()
                         .FirstOrDefault(s => s.ServiceName == serviceName);
                    //ctlFind.Stop();
                    //RunBatHelper.ExecuteCommand($"sc delete \"ArianScanner\"");
                    //RunBatHelper.ExecuteCommand($"sc.exe create \"ArianScanner\" binPath={exePath}");
                    //ServiceController ctlinstalled = ServiceController.GetServices()
                    //                         .FirstOrDefault(s => s.ServiceName == serviceName);
                    //ctlinstalled.Start();
                    InstallRunService(exePath);
                }

            }
        }

        public static void InstallRunService(string exePath)
        {
            //RunBatHelper.ExecuteCommand($"Set-Service -InputObject ArianScanner -Status Stopped");
            //RunBatHelper.ExecuteCommand($"sc.exe delete ArianScanner");

            RunBatHelper.ExecuteCommand($"sc.exe create \"ArianScanner\" binPath={exePath}");
            //ServiceController ctlinstalled = ServiceController.GetServices()
            //                         .FirstOrDefault(s => s.ServiceName == "ArianScanner");

            RunBatHelper.ExecuteCommand($"Set-Service -Name ArianScanner -StartupType Automatic");
            RunBatHelper.ExecuteCommand($"Set-Service -Name ArianScanner -Status Running -PassThru");
            //ctlinstalled.Start();
        }

        //public static ServiceController IsInstalledService(string serviceName)
        //{
        //    ServiceController ctl = ServiceController.GetServices()
        //                                             .FirstOrDefault(s => s.ServiceName == serviceName);

        //    if (ctl == null)
        //    {
        //        Console.WriteLine("Not installed");
        //        return null;
        //    }
        //    else
        //    {
        //        Console.WriteLine(ctl.Status);
        //        return ctl;
        //    }
        //}
    }
}
