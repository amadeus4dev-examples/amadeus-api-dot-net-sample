using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using travel_app.Models;

namespace travel_app.Controllers
{
  public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromServices] TravelAPI api, [FromQuery] string cityCode, [FromQuery] int? year)
        {
            if(!string.IsNullOrEmpty(cityCode)
            && year is int intYear)
            {
                await api.ConnectOAuth();
                var results = await api.GetBusiestTravelPeriodsOfYear(cityCode, intYear);
                ViewBag.Results = results;
            }
            return View();
        }

        public IActionResult Privacy()
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
