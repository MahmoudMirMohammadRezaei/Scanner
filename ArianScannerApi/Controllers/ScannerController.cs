using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArianScannerApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScannerController : ControllerBase
    {
        public SelfHost.IScannerService _scannerService;
        private readonly ILogger<ScannerController> _log;
        private readonly IWebHostEnvironment _env;

        public ScannerController(SelfHost.IScannerService scannerService, ILogger<ScannerController> log, IWebHostEnvironment env)
        {
            _scannerService = scannerService;
            _log = log;
            _env = env;
        }

        [HttpGet]
        public string Get()
        {
            try
            {
                string logPath = Path.Combine(_env.ContentRootPath, "Logs");
                var res = _scannerService.GetScan(logPath);
                _log.LogInformation("GetScan - Result: " + res);
                return res;
            }
            catch (Exception ex)
            {
                _log.LogError(ex, nameof(Get));
                if (ex.InnerException != null)
                {
                    var ex2 = ex.InnerException;
                    if (string.IsNullOrEmpty(ex2.StackTrace))
                    {
                        _log.LogError("GetScan: " + ex2.Message);
                        
                        if (ex2.InnerException != null)
                        {
                            var ex3 = ex2.InnerException;
                            if (string.IsNullOrEmpty(ex3.StackTrace))
                            {
                                _log.LogError("GetScan - InnerException: " + ex3.Message);
                                if (ex3.InnerException != null)
                                {
                                    var ex4 = ex3.InnerException;
                                    if (string.IsNullOrEmpty(ex4.StackTrace))
                                    {
                                        _log.LogError("GetScan - InnerException - InnerException: " + ex4.Message);
                                        if (ex4.InnerException != null)
                                        {
                                            var ex5 = ex4.InnerException;
                                            if (string.IsNullOrEmpty(ex4.StackTrace))
                                            {
                                                _log.LogError("GetScan - InnerException - InnerException: " + ex5.Message);
                                                //if (ex5.InnerException != null)
                                                //{

                                                //}
                                            }
                                            else
                                            {
                                                _log.LogError("GetScan - InnerException - InnerException: " + ex5.StackTrace.ToString());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _log.LogError("GetScan - InnerException - InnerException: " + ex4.StackTrace.ToString());
                                    }
                                }
                            }
                            else
                            {
                                _log.LogError("GetScan - InnerException: " + ex3.StackTrace.ToString());
                            }
                        }
                    }
                    else
                    {
                        _log.LogError("GetScan: " + ex2.StackTrace.ToString());
                    }
                }
                throw ex;
            }
        }

		[HttpGet]
		[Route("check")]
		public string CheckEndpoint()
		{
			return "The end point is reached!";
		}
	}
}
