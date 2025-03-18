using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using WebServices.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using MunicipalitiesDb = WebServices.Data.Municipalities;
using Municipalities = WebServices.Services.Municipalities;
#pragma warning disable CS8600, CS8603, CS8602


namespace WebServices.Controllers
{
    public class ServicesController : Controller
    {
        //Instancia del contexto de los modelos de la base de datos
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext(); 

        //Instancias de las clases con los servicios
        private readonly AppointmentsServices _AppointmentServices = new AppointmentsServices(); 
        private readonly UserServices _UserServices = new UserServices();
        private readonly LoginServices _LoginServices = new LoginServices();
        //private readonly EmailServices _EmailServices = new EmailServices();

        public ServicesController()
        {
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

        //Servicios de LoginServices---------------------------------------------------------------------------------------------------------------
        //Validar las credenciales del usuario
        [HttpPost]
        public ActionResult Login([FromBody] Credentials credentials)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            User user = _LoginServices.Login(credentials);
            return Json(user == null ? null : user);
        }

        //Cerrar Sesión
        [HttpPost]
        public ActionResult Logout([FromBody] Credentials credentials)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _LoginServices.Logout(credentials);
            return Json(!response.Success ? null : response);
        }

        [HttpPost]
        public ActionResult Register([FromBody] User user)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _LoginServices.CreateUser(user);
            return Json(!response.Success ? null : response);
        }

        //Servicios de AppointmensServices---------------------------------------------------------------------------------------------------------
        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public ActionResult GetMunicipalities()
        {
            //Llama al método del servicio AppointmentServices que consulta la lista total de municipios disponibles
            List<MunicipalitiesList> list = _AppointmentServices.Municipalities();
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de consultorios filtrada por Municipio
        [HttpGet]
        public ActionResult GetConsultories(int IdMunicipality)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de consultorios filtrada por municipio
            List<ConsultoriesList> list = _AppointmentServices.Consultories(IdMunicipality);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de Doctores filtrados por Consultorio
        [HttpGet]
        public ActionResult GetDoctors(int IdConsultory)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de doctores filtrada por consultorio
            List<DoctorList> list = _AppointmentServices.Doctors(IdConsultory);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public ActionResult GetAppointments(int IdDoctor)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            List<AppointmentList> list = _AppointmentServices.Appointments(IdDoctor);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpPost]
        public ActionResult CreateAppointment([FromBody] Appointment Appointment)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            Response response = _AppointmentServices.CreateAppointment(Appointment);
            return Json(!response.Success ? null : response);
        }
        public ActionResult DeleteAppointment([FromBody] Appointment Appointment)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            Response response = _AppointmentServices.DeleteAppointment(Appointment);
            return Json(!response.Success ? null : response);
        }
        //Servicios de UserServices ---------------------------------------------------------------------------------------------------------------
        //Devuelve la lista de usuarios registrados en la base de datos
        [HttpGet]
        public ActionResult GetUsers() 
        {
            //Llama al método del servicio UserServices que devuelve la lista total de usuarios
            List<UsersList> list = _UserServices.Users();
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve un usuario registrados en la base de datos
        [HttpGet]
        public ActionResult GetUser(int id) 
        {
            //Llama al método del servicio UserServices que devuelve la lista total de usuarios
            User user = _UserServices.User(id);
            return Json(user == null ? null : user);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public ActionResult UpdateUser([FromBody] User user)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _UserServices.Update(user);
            return Json(!response.Success ? null : response);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public ActionResult DeleteUser([FromBody] Users user)
        {
            //Llama al método del servicio UserServices que elimina a un usuario de la base de datos
            Response response = _UserServices.Delete(user);
            return Json(!response.Success ? null : response);
        }

        //Manejo de servicios de correos-----------------------------------------------------------------------------------------------------------
        public void SendEmails(Email data)
        {
            // Configurar el cliente SMTP
            using (SmtpClient clienteSmtp = new SmtpClient("smtp.gmail.com"))
            {
                clienteSmtp.Port = 587;
                clienteSmtp.Credentials = new NetworkCredential("perezmedellinenriquefabian@gmail.com", "inwqkdvoubvdugcv"); // Contraseña de aplicación Fabian
                clienteSmtp.EnableSsl = true;

                // Crear y enviar el correo
                using (MailMessage email = new MailMessage())
                {
                    email.From = new MailAddress("perezmedellinenriquefabian@gmail.com");
                    email.Subject = data.subject;
                    email.Body = data.body;
                    email.IsBodyHtml = true;
                    email.To.Add(data.user.email);

                    //Mandar el correo
                    clienteSmtp.Send(email);
                }
            }
        }
    }
}
