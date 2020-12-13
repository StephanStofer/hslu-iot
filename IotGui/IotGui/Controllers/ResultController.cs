using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using IotGui.Models;
using IotGui.Data;

namespace IotGui.Controllers
{
    public class ResultController : Controller
    {
        private readonly IDataService _dataService;
        private readonly ILogger<ResultController> _logger;

        public ResultController(IDataService dataService, ILogger<ResultController> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var data = _dataService.GetData();
            return View(data);
        }

        public IActionResult History()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
