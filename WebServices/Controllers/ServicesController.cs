using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using WebServices.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using MunicipalitiesDb = WebServices.Data.Municipalities;
using Municipalities = WebServices.Services.Municipalities;
using System.Net.Mail;
using System.Net;
#pragma warning disable CS8600, CS8603, CS8602


namespace WebServices.Controllers
{
    public class ServicesController : Controller
    {
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext(); 
        private readonly AppointmentsServices _AppointmentServices = new AppointmentsServices(); 
        private readonly UserServices _UserServices = new UserServices(); 

        private readonly ILogger<ServicesController> _logger;

        public ServicesController(ILogger<ServicesController> logger)
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //Método que valida las credenciales que manda el cliente
        [HttpPost]
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
        [HttpPost]
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

        [HttpPost]
        public async Task<IEnumerable<Municipalities>> CreateMunicipalities()
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
            List<Municipalities> municipalities = apiResponse.data;

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
            List<Specialties> specialties = apiResponse.data;

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

        /*
         Método de solo lectura que crea los consultorios que encuentra desde la API
         recibe un listado de consultorios encontrados en la API de DATAMÉXICO
        */
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

        //Servicios de AppointmensServices--------------------------------------------------------------------------------------

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public JsonResult GetMunicipalities()
        {
            //Llama al método del servicio AppointmentServices que consulta la lista total de municipios disponibles
            List<MunicipalitiesList> list = _AppointmentServices.Municipalities();
            return Json(list);
        }

        //Devuelve la lista de consultorios filtrada por Municipio
        [HttpGet]
        public JsonResult GetConsultories(int IdMunicipality)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de consultorios filtrada por municipio
            List<ConsultoriesList> list = _AppointmentServices.Consultories(IdMunicipality);
            return Json(list);
        }

        //Devuelve la lista de Doctores filtrados por Consultorio
        [HttpGet]
        public JsonResult GetDoctors(int IdConsultory)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de doctores filtrada por consultorio
            List<DoctorList> list = _AppointmentServices.Doctors(IdConsultory);
            return Json(list);
        }

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public JsonResult GetAppointments(int IdDoctor)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            List<AppointmentList> list = _AppointmentServices.Appointments(IdDoctor);
            return Json(list);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public JsonResult CreateAppointment([FromBody] Appointment newAppointment)
        {
            //Llama al método del servicio AppointmentServices que crea una nueva cita
            Response response = _AppointmentServices.CreateAppointment(newAppointment);
            return Json(response);
        }

        //Servicios de UserServices -----------------------------------------------------------------------------------------
        //Devuelve la lista de usuarios registrados en la base de datos
        [HttpGet]
        public JsonResult GetUsers() 
        {
            //Llama al método del servicio UserServices que devuelve la lista total de usuarios
            List<UsersList> Users = _UserServices.Users();
            return Json(Users); //<------ retorna la lista de usuarios en formato Json
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public JsonResult UpdateUser(Users user)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _UserServices.Update(user);
            return Json(response);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public JsonResult DeleteUser(Users user)
        {
            //Llama al método del servicio UserServices que elimina a un usuario de la base de datos
            Response response = _UserServices.Delete(user);
            return Json(response);
        }

        //Manejo de servicios de correos----------------------------------------------------------------------------------------------

        public void SendEmails(Email data)
        {
            // Validación básica de entrada
            if (string.IsNullOrEmpty(data.user.Email) || string.IsNullOrEmpty(data.subjectEmail) || string.IsNullOrEmpty(data.messageEmail))
            {
                Console.WriteLine("Error: los parámetros no pueden estar vacíos.");
                return;
            }

            try
            {
                // Configuración del servidor SMTP
                using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com"))
                {
                    clienteSmtp.Port = 587;
                    clienteSmtp.Credentials = new NetworkCredential("utconsultorio16@gmail.com", "UTSC2025"); // Usar contraseña de aplicación si tienes 2FA activado
                    clienteSmtp.EnableSsl = true;

                    // Crear el mensaje
                    using (MailMessage email = new MailMessage())
                    {
                        email.From = new MailAddress("utconsultorio16@gmail.com");
                        email.Subject = data.subjectEmail;
                        email.Body = data.messageEmail;
                        email.IsBodyHtml = true; // Si el mensaje tiene formato HTML
                        email.To.Add(data.user.Email);  // Agregar destinatario

                        // Enviar correo
                        clienteSmtp.Send(email);

                        Console.WriteLine("Correo enviado exitosamente.");
                    }
                }
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"Error al enviar correo (SMTP): {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar correo: {ex.Message}");
            }
        }
    }
}
