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

        public List<UsersList> UsersList()
        {
            List<UsersList> list = db.Users
                .Include(s =>s.fk_SexNavigation)
                .Include(r => r.fk_RoleNavigation)
                .Select(u => new UsersList { 
                    Id_User = u.Id_User,
                    Name = u.Name, 
                    Email = u.Email,
                    Phone = u.Phone.ToString(),
                    Sex = u.fk_SexNavigation.Name,
                    Role = u.fk_RoleNavigation.Name,
                    Active = u.Active ? "Activo" : "Inactivo"
                }) 
                .ToList();

            return list;
        }

        public Response UpdateUser(Users user)
        {
            var response = new Response();
            response.Success = false;
            response.Message = "";

            if(user != null)
            {
                var row = db.Users
                    .Where(u => u.Id_User == user.Id_User) 
                    .FirstOrDefault();  

                if(row != null)
                {
                    row.Name = user.Name;
                    row.Email = user.Email;
                    row.Phone = user.Phone;
                    row.fk_Sex = user.fk_Sex;
                    row.Active = user.Active;

                    response.Success = true;
                    response.Message = "Se ha actualizado el usuario";

                    db.SaveChanges();
                }

            }
            return response;
        }

        public Response DeleteUser(Users user)
        {
            var response = new Response();
            response.Success = false;
            response.Message = "";

            if (user != null)
            {
                var row = db.Users
                    .Where(u => u.Id_User == user.Id_User)
                    .FirstOrDefault();

                if (row != null)
                {
                    db.Users.Remove(row);
                    db.SaveChanges();

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
