using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ArianScannerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScannerController : ControllerBase
    {
        public SelfHost.IScannerService _scannerService;
        public ScannerController(SelfHost.IScannerService scannerService)
        {
            _scannerService = scannerService;
        }
        [HttpGet]
        public string Get()
        {
            return _scannerService.GetScan();
        }

		[HttpGet]
		[Route("check")]
		public string CheckEndpoint()
		{
			return "The end point is reached!";
		}
	}
}
