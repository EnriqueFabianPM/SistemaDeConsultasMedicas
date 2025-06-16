using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using WebServices.Services;
using Microsoft.EntityFrameworkCore;
using MunicipalitiesDb = WebServices.Data.Municipalities;
using Municipalities = WebServices.Services.Municipalities;
#pragma warning disable CS8600, CS8603, CS8602


namespace WebServices.Controllers
{
    public class ServicesController : Controller
    {
        private readonly Consultories_System_DevContext _db;
        private readonly AppointmentsServices _appointmentServices;
        private readonly UserServices _userServices;
        private readonly LoginServices _loginServices;

        public ServicesController(
            Consultories_System_DevContext db,
            AppointmentsServices appointmentServices,
            UserServices userServices,
            LoginServices loginServices)
        {
            _db = db;
            _appointmentServices = appointmentServices;
            _userServices = userServices;
            _loginServices = loginServices;
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
                List<MunicipalitiesDb> newMunicipalities = new();

                foreach (var municipality in municipalities)
                {
                    //Buscar si el objeto ya existe en la base de datos
                    var existingMunicipality = _db.Municipalities
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
                if (newMunicipalities.Any()) await _db.Municipalities.AddRangeAsync(newMunicipalities);
                await _db.SaveChangesAsync();
            }

            return municipalities;
        }

        //Métodó para crear las filas de las especialidades médicas
        public async Task CreateTypes()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.economia.gob.mx/apidatamexico/tesseract/data.jsonrecords?Resources+Categories=18&cube=health_resources&drilldowns=Resources+Subcategories&locale=es&measures=Total");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            //Leer la respuesta como cadena
            var jsonData = await response.Content.ReadAsStringAsync();

            // Si el JSON retornado es un array de municipios, se puede deserializar directamente:
            APIspecialties apiResponse = JsonSerializer.Deserialize<APIspecialties>(jsonData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Si no se encuentra la propiedad "data", se deberá ajustar según la estructura real
            List<Specialties> specialties = apiResponse.data;

            if (specialties != null)
            {
                //Lista vacía para almacenar las nuevas filas
                List<Types> types = new();

                foreach (var speciality in specialties)
                {
                    var existingType = _db.Types
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
                    await _db.Types.AddRangeAsync(types);
                }

                //Guardar los cambios
                _db.SaveChanges();
            }
        }

        /*
         Método de solo lectura que crea los consultorios que encuentra desde la API
         recibe un listado de consultorios encontrados en la API de DATAMÉXICO
        */
        public async void CreateConsultories(List<Municipalities> consultories)
        {
            //Traer todos los municipos desde la base de datos
            var existingMunicipalities = await _db.Municipalities.ToListAsync();

            //Verificar que la lista de consultorios no sea nulo
            if (consultories != null)
            {
                if (existingMunicipalities != null)
                {
                    List<Consultories> newConsultories = new();

                    foreach (var municipality in existingMunicipalities)
                    {
                        var filteredConsultories = consultories
                            .Where(e => e.CodigoPostal == municipality.Zip_Code)
                            .ToList();

                        if (filteredConsultories != null)
                        {
                            foreach (var consultory in filteredConsultories)
                            {
                                var existingConsultory = _db.Consultories
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
                        await _db.Consultories.AddRangeAsync(newConsultories);
                    }

                    _db.SaveChanges();
                }
            }
        }

        //Servicios de LoginServices---------------------------------------------------------------------------------------------------------------
        //Validar las credenciales del usuario
        [HttpPost]
        public ActionResult Login([FromBody] Credentials credentials)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            User user = _loginServices.Login(credentials);
            return Json(user == null ? null : user);
        }

        //Cerrar Sesión
        [HttpPost]
        public ActionResult Logout([FromBody] Credentials credentials)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _loginServices.Logout(credentials);
            return Json(!response.Success ? null : response);
        }

        [HttpPost]
        public ActionResult Register([FromBody] User user)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _loginServices.CreateUser(user);
            return Json(!response.Success ? null : response);
        }

        //Servicios de AppointmensServices---------------------------------------------------------------------------------------------------------
        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public ActionResult GetMunicipalities()
        {
            //Llama al método del servicio AppointmentServices que consulta la lista total de municipios disponibles
            List<MunicipalitiesList> list = _appointmentServices.Municipalities();
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de consultorios filtrada por Municipio
        [HttpGet]
        public ActionResult GetConsultories(int IdMunicipality)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de consultorios filtrada por municipio
            List<ConsultoriesList> list = _appointmentServices.Consultories(IdMunicipality);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de Doctores filtrados por Consultorio
        [HttpGet]
        public ActionResult GetDoctors(int IdConsultory)
        {
            //Llama al método del servicio AppointmentServices que consulta una lista de doctores filtrada por consultorio
            List<DoctorList> list = _appointmentServices.Doctors(IdConsultory);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpGet]
        public ActionResult GetAppointments(int IdDoctor)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            List<AppointmentList> list = _appointmentServices.Appointments(IdDoctor);
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve la lista de citas filtrada por usuario(Doctor)
        [HttpPost]
        public async Task<ActionResult> CreateAppointment([FromBody] Appointment Appointment)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            Response response = await _appointmentServices.Create(Appointment);
            return Json(!response.Success ? null : response);
        }

        [HttpPost]
        public ActionResult DeleteAppointment([FromBody] Appointment Appointment)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            Response response = _appointmentServices.Delete(Appointment);
            return Json(!response.Success ? null : response);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAppointment([FromBody] Appointment Appointment)
        {
            //Llama al método del servicio AppointmentServices que consulta las citas relacionadas a un doctor
            Response response = await _appointmentServices.Update(Appointment);
            return Json(!response.Success ? null : response);
        }

        //Servicios de UserServices ---------------------------------------------------------------------------------------------------------------
        //Devuelve la lista de usuarios registrados en la base de datos
        [HttpGet]
        public ActionResult GetUsers() 
        {
            //Llama al método del servicio UserServices que devuelve la lista total de usuarios
            List<UsersList> list = _userServices.Users();
            return Json(list.Count() == 0 ? null : list);
        }

        //Devuelve un usuario registrados en la base de datos
        [HttpGet]
        public ActionResult GetUser(int id) 
        {
            //Llama al método del servicio UserServices que devuelve la lista total de usuarios
            User user = _userServices.User(id);
            return Json(user == null ? null : user);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public ActionResult UpdateUser([FromBody] User user)
        {
            //Llama al método del servicio UserServices que actualiza los datos de un usuario existente
            Response response = _userServices.Update(user);
            return Json(!response.Success ? null : response);
        }

        //Devuelve una respuesta con el status de su petición HttpPost
        [HttpPost]
        public ActionResult DeleteUser([FromBody] Users user)
        {
            //Llama al método del servicio UserServices que elimina a un usuario de la base de datos
            Response response = _userServices.Delete(user);
            return Json(!response.Success ? null : response);
        }
    }
}