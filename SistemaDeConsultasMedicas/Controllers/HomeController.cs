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

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //Método que valida las credenciales que manda el cliente
        [HttpPost]
        public ActionResult Login(User user)
        {
            Response response = new Response();
            response.Success = false;

            //valida que el usuario sea diferente a null
            if (user != null)
            {
                //Busca al usuario en la base de datos
                var row = db.Users
                    .Where(u => u.Email == user.Email)
                    .FirstOrDefault();

                //valida que el usuario sea diferente a null
                if (row != null) 
                { 
                    //valida que la contraseña del usuario enviado sean las mismas que del usuario encontrado
                    bool success = user.Password == row.Password;

                    if (success) 
                    {
                        //Respuesta para el cliente y manejo de alertas
                        response.Success = true;

                        //propiedad para validar que el usuario tenga sesión iniciada
                        row.Login = true;

                        //guardamos los cambios en la base de datos
                        db.SaveChanges();
                    }
                    else
                    {
                        //si "success" es false entonces mandamos este mensaje al template
                        response.Message = "La contraseña es incorrecta";
                    }

                   
                }
            }

            //regresamos la respuesta en formato JSON
            return Json(response);
        }

        //Método para crear un usuario
        [HttpPost]
        public ActionResult CreateUser(User user)
        {

            Response response = new Response();

            //Por defecto esta propiedad será False
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
                        fk_Role = 1,
                        fk_Schedule = user.fk_Schedule,
                        fk_Type = user.fk_Type,
                        Login = false,
                    };

                    //Se manda esta propiedad en caso de querer utilizarla para activar una alerta Success exitosa
                    response.Success = true;

                    db.Users.Add(NewUser);
                    db.SaveChanges();
                }
                else
                {
                    //De lo contrario, Se añade este mensaje para advertir que ya existe un usuario con ese Email.
                    response.Message = $"Ya existe un usuario registrado con este email '{user.Email}', favor de introducir un email diferente";
                }
            }
            return Json(response);
        }
    }
}
