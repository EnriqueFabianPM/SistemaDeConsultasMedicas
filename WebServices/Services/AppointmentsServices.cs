using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class AppointmentsServices
    {
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();

        //Devuelve una lista de citas médicas filtradas por doctor
        public List<AppointmentList> Appointments(int IdDoctor)
        {
            List<AppointmentList> list = db.Medical_Appointments
                .Include(p => p.fk_PatientNavigation)
                .Include(s => s.fk_StatusNavigation)
                .Where(a => a.fk_Doctor == IdDoctor)
                .Select(a => new AppointmentList
                {
                    Id = a.Id_Appointment,
                    fk_Doctor = a.fk_Doctor,
                    fk_Patient = a.fk_Patient,
                    fk_Status = a.fk_Status,
                    Status = a.fk_StatusNavigation.Name, 
                    Patient = a.fk_PatientNavigation.Name,
                    Date = a.Date.ToString(),
                })
                .ToList();

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
        public List<ConsultoriesList> Consultories(int IdMunicipalty)
        {
            List<ConsultoriesList> list = db.Consultories
                .Where(c => c.Active)
                .Select(c => new ConsultoriesList
                {
                    Id = c.Id_Consultory,
                    Latitude = c.Latitude,
                    Length = c.Length,
                    Email = c.Email ?? "-",
                })
                .ToList();

            return list;
        }

        //Devuelve una lista de usuarios que estén relacionados a un consultorio
        public List<DoctorList> Doctors(int IdConsultory)
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
                Medical_Appointments newAppintment = new Medical_Appointments
                {
                    fk_Doctor = Appointment.fk_Doctor,
                    fk_Patient = Appointment.fk_Patient,
                    fk_Schedule = Appointment.fk_Schedule,
                    Notes = Appointment.Notes,
                    Date = DateTime.Now,
                    fk_Status = 1,
                };

                db.Medical_Appointments.Add(newAppintment);
                db.SaveChanges();

                response.Success = true;
                response.Message = "¡Se ha agendado la cita!";
            }

            return response;
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
        public string Date { get; set; }
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

    }

    public class DoctorList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Sex { get; set; }
    }
}
