using Microsoft.AspNetCore.Mvc;
using Services.Data;
using Newtonsoft.Json;
using Services.Models;
using Azure.Core;

namespace Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();

        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        //Método que valida las credenciales que manda el cliente
        [HttpPost(Name = "Login")]
        public Response Login(Users user)
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
            return response;
        }

        //Método para crear un usuario
        [HttpPost(Name = "CreateUser")]
        public Response CreateUser(Users user)
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
            return response;
        }

    }
}
