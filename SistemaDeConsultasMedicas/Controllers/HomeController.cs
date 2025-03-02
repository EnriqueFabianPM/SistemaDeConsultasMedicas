using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using SistemaDeConsultasMedicas.Models;
using SistemaDeConsultasMedicas.ViewModels;
using System.Text;
using System.Text.Json;

namespace SistemaDeConsultasMedicas.Controllers
{
    public class HomeController : Controller
    {
        private readonly Consultories_System_Context db = new Consultories_System_Context();
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


        //Método para conseguir las urls de las Apis que se vayan a consumir
        [HttpGet]
        public ActionResult GetAPI(int IdApi)
        {
            var api = db.APIs
                .Where(a => a.Id_API == IdApi)
                .Select(a => new
                {
                    Id = a.Id_API,
                    Name = a.Name,
                    URL = a.URL,
                    IsGet = a.IsGet,
                    IsPost = a.IsPost,
                })
                .FirstOrDefault();

            return Json(api);
        }

        //Método genérico para consumir APIs Get y Post
        [HttpPost]
        public async Task<ActionResult> ConsumeAPI(Api api)
        {
            using var client = new HttpClient();
            using var request = new HttpRequestMessage(api.IsGet ? HttpMethod.Get : HttpMethod.Post, api.URL);

            if (api.IsPost && api.BodyParams != null)
            {
                var json = JsonSerializer.Serialize(api.BodyParams);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var resp = await response.Content.ReadAsStringAsync();
            return Json(resp);
        }
    }
}
