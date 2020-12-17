using IotGui.Data;
using IotGui.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

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
            if (data != null) return View(data);
            return Problem("Json is empty, come back after filling measurements");
        }

        public IActionResult History()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }
    }
}