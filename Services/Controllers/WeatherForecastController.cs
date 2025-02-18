using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.Models;
using Azure.Core;
using System.Text.Json;
using Services.Data;
using Municipalities = Services.Models.Municipalities;
using MunicipalitiesDb = Services.Data.Municipalities;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS1030, CS8600, CS8603

namespace Services.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();

        //private static readonly string[] Summaries = new[]
        //{
        //    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        //};

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        //[HttpGet(Name = "GetWeatherForecast")]
        //public IEnumerable<WeatherForecast> Get()
        //{
        //    return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        //    {
        //        Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
        //        TemperatureC = Random.Shared.Next(-20, 55),
        //        Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        //    })
        //    .ToArray();
        //}

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
            return response;
        }

        //Método para crear un usuario
        [HttpPost(Name = "CreateUser")]
        public Response Create(Users user)
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

        [HttpPost(Name = "CreateMunicipalities")]
        public async Task<IEnumerable<Municipalities>> Create()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.economia.gob.mx/apidatamexico/tesseract/data.jsonrecords?parents=false&Establishment+Type=2&Metro+Area=991901&State=19&cube=health_establishments&drilldowns=State%2CMunicipality%2CCodigo+Postal%2CEstablishment+Type%2CLatitud%2CLongitud%2CMetro+Area&locale=es&measures=Clinics");

            // Si la API no requiere contenido en el cuerpo, puedes omitir el StringContent.
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            // Lee la respuesta como cadena
            var jsonData = await response.Content.ReadAsStringAsync();

            // Si el JSON retornado es un array de municipios, se puede deserializar directamente:
            APIMunicipality apiResponse = System.Text.Json.JsonSerializer.Deserialize<APIMunicipality>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Si no se encuentra la propiedad "data", se deberá ajustar según la estructura real
            List<Municipalities> municipalities = apiResponse?.data;

            if (municipalities != null)
            {
                //lista vacía que alamcenará los municipios nuevos que encuentre en la API
                List<MunicipalitiesDb> newMunicipalities = new List<MunicipalitiesDb>();


                foreach (var municipality in municipalities)
                {
                    //Buscar si el objeto ya existe en la base de datos
                    var existingMunicipality = db.Municipalities
                        .Where(e => e.Zip_Code == municipality.CodigoPostal)
                        .FirstOrDefault();

                    if (existingMunicipality == null)
                    {
                        if (!newMunicipalities.Any(e => e.Name == municipality.Municipality))
                        {
                            //Crear un nuevo objeto para la base de datos
                            MunicipalitiesDb newMunicipality = new MunicipalitiesDb
                            {
                                Name = municipality.Municipality,
                                Zip_Code = municipality.CodigoPostal,
                            };

                            //Añadir municipio a la lista de municipios a crear 
                            newMunicipalities.Add(newMunicipality);
                        }
                    }
                }

                //Si existen objetos en la lista, crearlos en la base de datos
                if (newMunicipalities.Any())
                {
                    await db.Municipalities.AddRangeAsync(newMunicipalities);
                }

                await db.SaveChangesAsync();

                CreateTypes();
                CreateConsultories(municipalities);
            }

            return municipalities;
        }

        //Métodó para crear las filas de las especialidades médicas
        public async void CreateTypes()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.economia.gob.mx/apidatamexico/tesseract/data.jsonrecords?Resources+Categories=18&cube=health_resources&drilldowns=Resources+Subcategories&locale=es&measures=Total");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Leer la respuesta como cadena
            var jsonData = await response.Content.ReadAsStringAsync();

            // Si el JSON retornado es un array de municipios, se puede deserializar directamente:
            APIspecialties apiResponse = System.Text.Json.JsonSerializer.Deserialize<APIspecialties>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Si no se encuentra la propiedad "data", se deberá ajustar según la estructura real
            List<Specialties> specialties = apiResponse?.data;

            if (specialties != null)
            {
                //Lista vacía para almacenar las nuevas filas
                List<Types> types = new List<Types>();

                foreach (var speciality in specialties)
                {
                    var existingType = db.Types
                        .Where(e => e.Name == speciality.Name)
                        .FirstOrDefault();

                    if (existingType == null)
                    {
                        if (!types.Any(e => e.Name == speciality.Name))
                        {
                            Types newSpeciality = new Types
                            {
                                Name = speciality.Name,
                                Active = true,
                            };

                            types.Add(newSpeciality);
                        }
                    }
                }

                //Si la lista contiene nuevas especialidades se agregan a la base de datos
                if (types.Any())
                {
                    await db.Types.AddRangeAsync(types);
                }

                //Guardar los cambios
                db.SaveChanges();
            }
        }

        public async void CreateConsultories(List<Municipalities> consultories)
        {
            //Traer todos los municipos desde la base de datos
            var existingMunicipalities = await db.Municipalities.ToListAsync();

            //Verificar que la lista de consultorios no sea nulo
            if (consultories != null)
            {
                if (existingMunicipalities != null)
                {
                    List<Consultories> newConsultories = new List<Consultories>();

                    foreach (var municipality in existingMunicipalities)
                    {
                        var filteredConsultories = consultories
                            .Where(e => e.CodigoPostal == municipality.Zip_Code)
                            .ToList();

                        if (filteredConsultories != null)
                        {
                            foreach (var consultory in filteredConsultories)
                            {
                                var existingConsultory = db.Consultories
                                    .Where(e => e.Length == consultory.Longitud && e.Latitude == consultory.Latitud)
                                    .FirstOrDefault();

                                if (existingConsultory == null)
                                {
                                    if (!newConsultories.Any(e => e.Latitude == consultory.Latitud && e.Length == consultory.Longitud))
                                    {
                                        Consultories newConsultory = new Consultories
                                        {
                                            fk_Municipality = municipality.Id_Municipality,
                                            Latitude = consultory.Latitud,
                                            Length = consultory.Longitud,
                                            Active = true,
                                        };

                                        newConsultories.Add(newConsultory);
                                    }
                                }
                            }
                        }

                    }

                    if (newConsultories.Any())
                    {
                        await db.Consultories.AddRangeAsync(newConsultories);
                    }

                    db.SaveChanges();

                }

            }
        }
    }
}
