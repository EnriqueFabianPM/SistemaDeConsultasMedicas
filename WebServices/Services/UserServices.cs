using WebServices.Data;
using WebServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
#pragma warning disable CS8618, CS8603

namespace WebServices.Services
{
    public class UserServices
    {

        private readonly IViewRenderService _emailServices;
        private readonly Consultories_System_DevContext _db;
        public UserServices(IViewRenderService emailServices,
            Consultories_System_DevContext db)
        {
            _emailServices = emailServices;
            _db = db;
        }

        public object Users()
        {
            var list =  _db.Users
                .Include(s =>s.fk_SexNavigation)
                .Include(r => r.fk_RoleNavigation)
                .Include(c => c.fk_ConsultoryNavigation)
                .Select(u => new
                { 
                    u.Id_User,
                    u.Name, 
                    u.Email,
                    u.fk_Role,
                    u.fk_Consultory,
                    u.fk_ConsultoryNavigation.fk_Municipality,
                    u.fk_Type,
                    Phone = u.Phone == null ? "-" : u.Phone.ToString() ?? "",
                    Sex = u.fk_SexNavigation.Name,
                    Role = u.fk_RoleNavigation.Name,
                    Active = u.Active ? "Activo" : "Inactivo",
                }) 
                .ToList();

            return list;
        } 

        public User User(int id)
        {
                var user = _db.Users
                .Include(s => s.fk_SexNavigation)
                .Include(r => r.fk_RoleNavigation)
                .Where(u => u.Id_User == id && u.Login && u.Active)
                .Select(u => new User
                {
                    id_User = u.Id_User,
                    name = u.Name,
                    email = u.Email,
                    phone = u.Phone,
                    sex = u.fk_SexNavigation.Name,
                    fk_Role = u.fk_Role,
                    role = u.fk_RoleNavigation.Name,
                    active = u.Active
                })
                .FirstOrDefault();

            return user;
        }

        public object Roles()
        {
            var roles = _db.Roles
                .Select(r => new
                {
                    Id = r.Id_Role,
                    r.Name
                })
                .ToList();
            return roles;
        }

        public async Task<Response> Update(Users user)
        {
            Response response = new();

            if(user != null)
            {
                var row = _db.Users
                    .Include(s => s.fk_RoleNavigation)
                    .Where(u => u.Id_User == user.Id_User) 
                    .FirstOrDefault();  

                if(row != null)
                {
                    row.Name = user.Name != null ? user.Name : row.Name;
                    row.Email = user.Email != null ? user.Email : row.Email;
                    row.Phone = user.Phone != null ? user.Phone : row.Phone;
                    row.fk_Role = user.fk_Role != 0 ? user.fk_Role : row.fk_Role;
                    row.fk_Consultory = user.fk_Consultory != null && user.fk_Role == 3  ? user.fk_Consultory : null;
                    row.fk_Type = user.fk_Type != null && user.fk_Role == 3  ? user.fk_Type : null;
                    row.Active = user.Active;

                    _db.SaveChanges();

                    response.Success = true;
                    response.Message = "Se ha actualizado el usuario";

                    if (user.fk_Role != row.fk_Role)
                    {
                        Email email = new()
                        {
                            user = row,
                            subject = "Cambio de rol en Consultorios System Dev",
                        };

                        email.body = await _emailServices.RenderToStringAsync("UserRoleStatus", email);

                        if (email.body != null)
                        {
                            _emailServices.SendEmail(email);
                            response.Message = "Se ha cambiado el rol del usuario";
                        }
                    }

                }
            }
            return response;
        }

        //Método que elimina a un usuario y objetos derivados del mismo
        public Response Delete(Users user)
        {
            Response response = new();
            if (user != null)
            {
                //Busca al usuario en la base de datos
                var row = _db.Users
                    .Where(u => u.Id_User == user.Id_User)
                    .FirstOrDefault();

                //Valida que el usuario no sea nulo
                if (row != null)
                {
                    //Obtiene la lista de citas médicas del usuario a eliminar
                    List<Medical_Appointments> Appointments = _db.Medical_Appointments
                        .Where(a => a.fk_Patient == row.Id_User)
                        .ToList();

                    if (Appointments.Any())
                    {
                        //elimina la lista de citas médicas relacionadas con el usario
                        _db.Medical_Appointments.RemoveRange(Appointments);
                    }

                    _db.Users.Remove(row);
                    _db.SaveChanges();

                    //Se actualiza la respuesta del servidor
                    response.Success = true;
                    response.Message = "Se ha eliminado el usuario";
                }

            }
            return response;
        }
    }

    public class UsersList
    {
        public int Id_User { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Sex { get; set; }
        public string Role { get; set; }
        public string Active { get; set; }
    }
}