using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using SistemaDeConsultasMedicas.Models;
using SistemaDeConsultasMedicas.ViewModels;
using System.Text;
using System.Text.Json;
#pragma warning disable CS8600, CS8603, CS8602

namespace SistemaDeConsultasMedicas.Controllers
{
    public class HomeController : Controller
    {
        private readonly Consultories_System_Context db = new Consultories_System_Context();

        public HomeController()
        {

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
        public Api FetchAPI(Config config)
        {
            Api api = new Api();
            if (config.IdApi != null && config.IdApi > 0)
            {
                api = db.APIs
                    .Where(a => a.Id_API == config.IdApi)
                    .Select(a => new Api
                    {
                        Id_API = a.Id_API,
                        Name = a.Name,
                        URL = a.URL,
                        IsGet = a.IsGet,
                        IsPost = a.IsPost,
                        Param = config.Param.ToString() ?? null,
                        BodyParams = config.BodyParams,
                    })
                    .FirstOrDefault();
            }

            return api;
        }

        //Método genérico para consumir APIs Get y Post
        [HttpPost]
        public async Task<ActionResult> ConsumeApi([FromBody] Config config)
        {
            //Buscar la api y mapear sus parámetros
            Api api = FetchAPI(config);

            //Declarar el objeto que se devolverá
            object result = null;

            //Validar que api no sea nulo
            if(api != null)
            {
                using var client = new HttpClient();
                using var request = new HttpRequestMessage(api.IsGet ? HttpMethod.Get : HttpMethod.Post, api.Param != "" ? (api.URL + api.Param) : api.URL);

                if (api.IsPost && api.BodyParams != null)
                {
                    var json = JsonSerializer.Serialize(api.BodyParams);
                    request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                }

                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Leer la respuesta como string
                var responseString = await response.Content.ReadAsStringAsync();

                // Configurar JsonSerializerOptions para evitar la conversión a camelCase
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // Esto evita que los nombres de las propiedades se cambien
                };

                // Deserializar la respuesta a un objeto genérico
                result = JsonSerializer.Deserialize<object>(responseString, options);
            }

            // Devuelve el objeto deserializado como JSON
            return Json(result);
        }
    }
}
