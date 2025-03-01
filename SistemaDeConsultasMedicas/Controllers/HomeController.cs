using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;

namespace SistemaDeConsultasMedicas.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //Controladores de las vistas----------------------------------------------------------------------------------------
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Users()
        {
            return View();
        }

        public IActionResult Appointments()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //Método genérico para consumir una api de tipo get
        [HttpGet]
        public async Task<ActionResult> GetMunicipalities()
        {
            //Variable para compilar en otros equipos
            string url = "https://localhost:7098/Services/GetMunicipalities";
            //string url = "https://localhost:7098/Services/GetMunicipalities";
            //string url = "https://localhost:7098/Services/GetMunicipalities";
            //string url = "https://localhost:7098/Services/GetMunicipalities";
            //string url = "https://localhost:7098/Services/GetMunicipalities";
            //string url = "https://localhost:7098/Services/GetMunicipalities";

            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return Json(await response.Content.ReadAsStringAsync());
        }
    }
}
