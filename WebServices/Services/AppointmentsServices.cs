using Microsoft.EntityFrameworkCore;
using System.Numerics;
using WebServices.Controllers;
using WebServices.Data;
using WebServices.Models;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class AppointmentsServices
    {
        private readonly IViewRenderService _emailServices;
        private readonly Consultories_System_DevContext _db;

        public AppointmentsServices(
            IViewRenderService emailServices,
            Consultories_System_DevContext db)
        {
            _emailServices = emailServices;
            _db = db;
        }

        //Devuelve una lista de citas médicas filtradas por doctor
        public List<AppointmentList> Appointments(int? IdDoctor)
        {
            var user = _db.Users
                .Where(u => u.Id_User == IdDoctor)
                .FirstOrDefault();

            List<AppointmentList> list = new List<AppointmentList>();

            if(user?.fk_Role == 1)
            {
                list = _db.Medical_Appointments
                    .Include(p => p.fk_PatientNavigation)
                    .Include(s => s.fk_StatusNavigation)
                    .Select(a => new AppointmentList
                    {
                        Id = a.Id_Appointment,
                        fk_Doctor = a.fk_Doctor,
                        fk_Patient = a.fk_Patient,
                        fk_Status = a.fk_Status,
                        Patient = a.fk_PatientNavigation.Name,
                        Created = a.Created_Date.ToString(),
                        Assigned = a.Appointment_Date.ToString(),
                        Status = a.fk_StatusNavigation.Name,
                    })
                    .ToList();
            }
            else
            {
                list = _db.Medical_Appointments
                    .Include(p => p.fk_PatientNavigation)
                    .Include(s => s.fk_StatusNavigation)
                    .Where(a => a.fk_Doctor == IdDoctor)
                    .Select(a => new AppointmentList
                    {
                        Id = a.Id_Appointment,
                        fk_Doctor = a.fk_Doctor,
                        fk_Patient = a.fk_Patient,
                        fk_Status = a.fk_Status,
                        Patient = a.fk_PatientNavigation.Name,
                        Created = a.Created_Date.ToString(),
                        Assigned = a.Appointment_Date.ToString(),
                        Status = a.fk_StatusNavigation.Name,
                    })
                    .ToList();
            }
            return list;
        }


        //devuelve la lista total de municipios de la base de datos
        public List<MunicipalitiesList> Municipalities()
        {
            List<MunicipalitiesList> list = _db.Municipalities
                .Select(m => new MunicipalitiesList
                {
                    Id = m.Id_Municipality,
                    Name = m.Name,
                    Zip_Code = m.Zip_Code.ToString(),
                })
                .ToList();

            return list;
        }

        public object Statuses()
        {
            var list = _db.Status
                .Select(s => new 
                {
                    Id = s.Id_Status,
                    s.Name,
                })
                .ToList();

            return list;
        }

        //Devuelve una lista de consultorios filtrados por municipio
        public List<ConsultoriesList> Consultories(int? IdMunicipalty)
        {
            List<ConsultoriesList> list = _db.Consultories
                .Where(c => c.Active && c.fk_Municipality == IdMunicipalty)
                .Select(c => new ConsultoriesList
                {
                    Id = c.Id_Consultory,
                    Latitude = c.Latitude,
                    Length = c.Length,
                    Email = c.Email ?? "-",
                    Name = c.Name,
                })
                .ToList();

            return list;
        }

        //Devuelve una lista de usuarios que estén relacionados a un consultorio
        public List<DoctorList> Doctors(int? IdConsultory)
        {
            List<DoctorList> list = _db.Users
                .Include(s => s.fk_SexNavigation)
                .Where(d => d.Active && d.fk_Consultory == IdConsultory)
                .Select(d => new DoctorList
                {
                    Id = d.Id_User,
                    Name = d.Name,
                    Email = d.Email,
                    Sex = d.fk_SexNavigation.Name,
                })
                .ToList();

            return list;
        }

        //Método para crear una una cita médica
        public async Task<Response> Create(Appointment Appointment)
        {
            Response response = new Response
            {
                Success = false,
                Message = "",
            };

            if (Appointment != null)
            {

                DateTime currentDate = DateTime.Now.Date;
                DateTime appointmentDate = GetToDateFromDateAndWorkDays(currentDate, 1);

                Medical_Appointments newAppintment = new Medical_Appointments
                {
                    fk_Doctor = Appointment.fk_Doctor,
                    fk_Patient = Appointment.fk_Patient,
                    fk_Status = 1,
                    Created_Date = currentDate,
                    Appointment_Date = appointmentDate,
                    Notes = Appointment.notes,
                };

                _db.Medical_Appointments.Add(newAppintment);
                _db.SaveChanges();

                //Buscar al doctor para enviarle un correos
                var doctor = _db.Users
                    .Include(d => d.fk_ConsultoryNavigation)
                    .Where(d => d.Id_User == Appointment.fk_Doctor)
                    .FirstOrDefault();

                if (doctor != null)
                {
                    //crear objeto de correo electrónico
                    Email email = new()
                    {
                        subject = "Cita Solicitada",
                        user = doctor,
                        consultory = doctor.fk_ConsultoryNavigation,
                        message = Appointment.notes ?? "No hay notas disponibles.",
                    };

                    //obtener el cuerpo del correo electrónico renderizado
                    email.body = await _emailServices.RenderToStringAsync("DoctorMessage", email);

                    if(email.body != null)
                    {
                        //enviar el correo electrónico
                        _emailServices.SendEmail(email);
                    }
                }

                //Busca al paciente relacionado con la cita médica
                var patient = _db.Users
                    .Where(d => d.Id_User == Appointment.fk_Patient)
                    .FirstOrDefault();

                if (patient != null && doctor != null)
                {
                    //Busca el consultorio relacionado con el paciente
                    var consultory = _db.Consultories
                        .Where(c => c.Id_Consultory == doctor.fk_Consultory)
                        .FirstOrDefault();

                    if(consultory != null)
                    {
                        //crear objeto de correo electrónico para el paciente
                        Email email = new()
                        {
                            subject = "Cita Agendada",
                            user = patient,
                            consultory = consultory,
                            appointment = newAppintment,
                        };

                        //obtener el cuerpo del correo electrónico renderizado
                        email.body = await _emailServices.RenderToStringAsync("PatientMessage", email);

                        if (email.body != null)
                        {
                            //enviar el correo electrónico al paciente
                            _emailServices.SendEmail(email);
                        }
                    }
                }

                response.Success = true;
                response.Message = "¡Se ha agendado la cita!";
            }

            return response;
        }

        //Método para crear una una cita médica
        public Response Delete(Appointment Appointment)
        {
            Response response = new Response
            {
                Success = false,
                Message = "",
            };

            if (Appointment != null)
            {
                var appointment = _db.Medical_Appointments
                    .Where(a => a.Id_Appointment == Appointment.id_Appointment)
                    .FirstOrDefault();

                if (appointment != null) 
                {
                    _db.Medical_Appointments.Remove(appointment);
                    _db.SaveChanges();
                    response.Message = "¡Se ha Cancelado la cita!";
                }
                else response.Message = "¡La cita no existe o ya ha sido cancelada!";

                response.Success = true;            
            }
            return response;
        }

        //Método para Actualizar una cita médica
        public async Task<Response> Update(Appointment Appointment)
        {
            Response response = new Response
            {
                Success = false,
                Message = "",
            };

            if (Appointment != null) 
            {
                var existingAppintment = _db.Medical_Appointments
                    .Include(p => p.fk_StatusNavigation)
                    .Where(a => Appointment.id_Appointment == a.Id_Appointment)
                    .FirstOrDefault();

                if (existingAppintment != null) 
                { 
                    if (Appointment?.fk_Status != null) existingAppintment.fk_Status = Appointment.fk_Status;
                    if (Appointment?.notes != null) existingAppintment.Notes = Appointment.notes;
                
                    _db.SaveChanges();

                    //Busca al paciente relacionado con la cita médica
                    var patient = _db.Users
                        .Where(d => d.Id_User == existingAppintment.fk_Patient)
                        .FirstOrDefault();

                    if (patient != null)
                    {
                        //crear objeto de correo electrónico para el paciente
                        Email email = new()
                        {
                            subject = "Cita Agendada",
                            user = patient,
                            appointment = existingAppintment,
                        };

                        //obtener el cuerpo del correo electrónico renderizado
                        email.body = await _emailServices.RenderToStringAsync("StatusAppointemnt", email);

                        if (email.body != null)
                        {
                            //enviar el correo electrónico al paciente
                            _emailServices.SendEmail(email);
                        }
                    }

                    response.Success = true;
                    response.Message = "¡Se ha actualizado la cita!";
                }
            }

            return response;
        }

        //Obtiene la fecha actual y además devuelve la fecha de la cita
        public static DateTime GetToDateFromDateAndWorkDays(DateTime fromDate, int workDays)
        {
            // Excluir los fines de semana
            int weekendDays = 0;
            DateTime toDate = fromDate;

            while (workDays > 0)
            {
                toDate = toDate.AddDays(1);
                if (!IsWeekendDay(toDate)) workDays--;
                else weekendDays++;
            }

            return toDate;
        }

        public static bool IsWeekendDay(DateTime date)
        {
            DayOfWeek day = date.DayOfWeek;
            return day == DayOfWeek.Saturday || day == DayOfWeek.Sunday;
        }

    }

    //ViewModels
    public class AppointmentList
    {
        public int Id { get; set; }
        public int fk_Doctor { get; set; }
        public int fk_Patient { get; set; }
        public int fk_Status { get; set; }
        public string Patient { get; set; }
        public string Status { get; set; }
        public string Created { get; set; }
        public string Assigned { get; set; }
        public string Notes { get; set; }
    }

    public class MunicipalitiesList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Zip_Code { get; set; }
    }

    public class ConsultoriesList
    {
        public int Id { get; set; }
        public string Latitude { get; set; }
        public string Length { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }

    }

    public class DoctorList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Sex { get; set; }
    }
}
