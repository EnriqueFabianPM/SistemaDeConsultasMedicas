using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using WebServices.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PatternContexts;
using WebServices.Controllers;
using Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore;
using NuGet.Common;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class AppointmentsServices
    {
        private readonly ServicesController _servicesController;
        //private readonly EmailServices _EmailServices = new EmailServices();
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();

        //Devuelve una lista de citas médicas filtradas por doctor
        public List<AppointmentList> Appointments(int? IdDoctor)
        {
            var user = db.Users
                .Where(u => u.Id_User == IdDoctor)
                .FirstOrDefault();

            List<AppointmentList> list = new List<AppointmentList>();

            if(user?.fk_Role == 1)
            {
                list = db.Medical_Appointments
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
                list = db.Medical_Appointments
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
            List<MunicipalitiesList> list = db.Municipalities
                .Select(m => new MunicipalitiesList
                {
                    Id = m.Id_Municipality,
                    Name = m.Name,
                    Zip_Code = m.Zip_Code.ToString(),
                })
                .ToList();

            return list;
        }

        //Devuelve una lista de consultorios filtrados por municipio
        public List<ConsultoriesList> Consultories(int? IdMunicipalty)
        {
            List<ConsultoriesList> list = db.Consultories
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
            List<DoctorList> list = db.Users
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
        public Response CreateAppointment(Appointment Appointment)
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

                db.Medical_Appointments.Add(newAppintment);
                db.SaveChanges();

                response.Success = true;
                response.Message = "¡Se ha agendado la cita!";
            }
            return response;
        }

        //Método para crear una una cita médica
        public Response DeleteAppointment(Appointment Appointment)
        {
            Response response = new Response
            {
                Success = false,
                Message = "",
            };

            if (Appointment != null)
            {
                var appointment = db.Medical_Appointments
                    .Where(a => a.Id_Appointment == Appointment.id_Appointment)
                    .FirstOrDefault();

                if (appointment != null) 
                {
                    db.Medical_Appointments.Remove(appointment);
                    db.SaveChanges();
                    response.Message = "¡Se ha Cancelado la cita!";
                }
                else
                {
                    response.Message = "¡La cita no existe o ya ha sido cancelada!";
                }

                response.Success = true;            
            }
            return response;
        }

        //Método para Actualizar una cita médica
        public Response UpdateAppointment(Appointment Appointment)
        {
            Response response = new Response
            {
                Success = false,
                Message = "",
            };

            if (Appointment != null) 
            {
                var existingAppintment = db.Medical_Appointments
                    .Where(a => Appointment.id_Appointment == a.Id_Appointment)
                    .FirstOrDefault();

                if (existingAppintment != null) 
                { 
                    existingAppintment.fk_Patient = Appointment.fk_Patient;
                    existingAppintment.fk_Doctor = Appointment.fk_Doctor;
                    existingAppintment.fk_Status = Appointment.fk_Status;
                    existingAppintment.Notes = Appointment.notes;
                
                    db.SaveChanges();

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
                if (!IsWeekendDay(toDate))
                {
                    workDays--;
                }
                else
                {
                    weekendDays++;
                }
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
