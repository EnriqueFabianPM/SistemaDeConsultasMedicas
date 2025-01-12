using Microsoft.AspNetCore.Mvc;
using SistemaDeConsultasMedicas.Models;
using Microsoft.EntityFrameworkCore;
using SistemaDeConsultasMedicas.Services;
using System.Diagnostics;
using Newtonsoft;

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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateUser(User user)
        {           

            Response response = new Response();

            //Por defecto esta propiedad ser� False
            response.Success = false;

            //Manejo en caso de nulos
            if (user != null) 
            {
                var row = db.Users
                    .Where(e => e.Email == user.Email)
                    .FirstOrDefault();

                /*
                 En caso de que no se encuentre un usuario
                 se crea el objeto en la base de datos.
                 */
                if (row == null) 
                { 

                    //Se crea el objeto con las clases de la base de datos
                    Users NewUser = new Users
                    {
                        Name = user.Name,
                        Email = user.Email,
                        Password = user.Password,
                        Phone = user.Phone,
                        fk_Sex = user.fk_Sex,
                        fk_Consultory = user.fk_Consultory,
                        fk_Role = user.fk_Role == 1 ? 1:2,
                        fk_Schedule = user.fk_Schedule,
                        fk_Type = user.fk_Type,
                    };

                    //Se manda esta propiedad en caso de querer utilizarla para activar una alerta Success exitosa
                    response.Success = true;

                    db.Users.Add(NewUser);
                    db.SaveChanges();
                }
                else
                {
                    //De lo contrario, Se a�ade este mensaje para advertir que ya existe un usuario con ese Email.
                    response.Message = $"Ya existe un usuario registrado con este email '{user.Email}', favor de introducir un email diferente";
                }
            }
            return Json(response);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
