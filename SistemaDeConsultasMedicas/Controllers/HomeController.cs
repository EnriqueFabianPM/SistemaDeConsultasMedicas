using Microsoft.AspNetCore.Mvc;
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

        public HomeController(){}

        //Controladores de las vistas----------------------------------------------------------------------------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users(int id)
        {
            Config config = new Config()
            {
                IdApi = 12,
                BodyParams = null,
                Param = id.ToString(),
            };

            JsonResult jsonResult = await ConsumeApi(config) as JsonResult;
            object response = jsonResult?.Value; // Extraer el objeto real

            if (response != null)
            {
                Users user = JsonSerializer.Deserialize<Users>(JsonSerializer.Serialize(jsonResult.Value));

                if (user.fk_Role == 1)
                {
                    ViewBag.User = response;
                    return View();
                } else return StatusCode(403);
            } else return StatusCode(403);
        }

        [HttpGet]
        public async Task<IActionResult> Appointments(int id)
        {
            Config config = new Config()
            {
                IdApi = 12,
                BodyParams = null,
                Param = id.ToString(),
            };

            JsonResult jsonResult = await ConsumeApi(config) as JsonResult;
            object user = jsonResult?.Value; // Extraer el objeto real


            if (user != null)
            {
                ViewBag.User = user;
                return View();
            } else return StatusCode(403);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            Config config = new Config()
            {
                IdApi = 12,
                BodyParams = null,
                Param = id.ToString(),
            };

            JsonResult jsonResult = await ConsumeApi(config) as JsonResult;
            object user = jsonResult?.Value; // Extraer el objeto real


            if (user != null)
            {
                ViewBag.User = user;
                return View();
            } else return StatusCode(403);
        }

        //Método para conseguir las urls de las Apis que se vayan a consumir
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
                        Param = a.IsGet ? (config.Param != null ? config.Param : "") : "",
                        BodyParams = a.IsPost ? config.BodyParams : null,
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
            return Json(result == null ? null : result);
        }
    }
}