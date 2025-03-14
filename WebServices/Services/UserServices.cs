using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using WebServices.Data;
using WebServices.Models;
using WebServices.Services;
using Microsoft.EntityFrameworkCore;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class UserServices
    {

        public UserServices() { }

        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();

        public List<UsersList> Users()
        {
            List<UsersList> list = db.Users
                .Include(s =>s.fk_SexNavigation)
                .Include(r => r.fk_RoleNavigation)
                .Select(u => new UsersList { 
                    Id_User = u.Id_User,
                    Name = u.Name, 
                    Email = u.Email,
                    Phone = u.Phone.ToString() ?? "-",
                    Sex = u.fk_SexNavigation.Name,
                    Role = u.fk_RoleNavigation.Name,
                    Active = u.Active ? "Activo" : "Inactivo"
                }) 
                .ToList();

            return list;
        } 

        public User User(int id)
        {
                var user = db.Users
                .Include(s => s.fk_SexNavigation)
                .Include(r => r.fk_RoleNavigation)
                .Where(u => u.Id_User == id && u.Login)
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

        public Response Update(User user)
        {
            var response = new Response();
            response.Success = false;
            response.Message = "";

            if(user != null)
            {
                var row = db.Users
                    .Where(u => u.Id_User == user.id_User) 
                    .FirstOrDefault();  

                if(row != null)
                {
                    row.Name = user.name;
                    row.Email = user.email;
                    row.Phone = user.phone;
                    row.fk_Sex = user.fk_Sex;
                    row.Active = user.active;

                    response.Success = true;
                    response.Message = "Se ha actualizado el usuario";

                    db.SaveChanges();
                }

            }
            return response;
        }

        //Método que elimina a un usuario y objetos derivados del mismo
        public Response Delete(Users user)
        {
            Response response = new Response();
            response.Success = false;
            response.Message = "";

            if (user != null)
            {
                //Busca al usuario en la base de datos
                var row = db.Users
                    .Where(u => u.Id_User == user.Id_User)
                    .FirstOrDefault();

                //Valida que el usuario no sea nulo
                if (row != null)
                {
                    //Obtiene la lista de citas médicas del usuario a eliminar
                    List<Medical_Appointments> Appointments = db.Medical_Appointments
                        .Where(a => a.fk_Patient == row.Id_User)
                        .ToList();

                    if (Appointments.Any())
                    {
                        //elimina la lista de citas médicas relacionadas con el usario
                        db.Medical_Appointments.RemoveRange(Appointments);
                    }

                    //Elimina al usuario de la base de datos
                    db.Users.Remove(row);

                    //Guarda los cambios
                    db.SaveChanges();

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
