using WebServices.Data;
using WebServices.Models;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Tls;
#pragma warning disable CS8618

namespace WebServices.Services
{
    public class LoginServices
    {
        private readonly Consultories_System_DevContext db = new Consultories_System_DevContext();
        public LoginServices() { }

        //Método que valida las credenciales que manda el cliente
        public User Login(Credentials credentials)
        {
            //Modelo de usuario vacío
            User user = new();

            //valida que el usuario sea diferente a null
            if (credentials != null)
            {
                //Busca al usuario en la base de datos
                var row = db.Users
                    .Include(s => s.fk_SexNavigation)
                    .Include(r => r.fk_RoleNavigation)
                    .Include(c => c.fk_ConsultoryNavigation)
                    .Include(t => t.fk_TypeNavigation)
                    .Where(u => u.Email == credentials.email && u.Active)
                    .FirstOrDefault();

                //valida que el usuario sea diferente a null
                if (row != null)
                {
                    //valida que la contraseña del usuario enviado sean las mismas que del usuario encontrado
                    if (credentials.password == row.Password)
                    {
                        //Mapear Usuario
                        user.id_User = row.Id_User;
                        user.name = row.Name;
                        user.email = row.Email;
                        user.password = null;
                        user.phone = row.Phone;
                        user.fk_Sex = row.fk_Sex;
                        user.fk_Role = row.fk_Role;
                        user.fk_Consultory = row.fk_Consultory;
                        user.fk_Type = row.fk_Type;
                        user.active = row.Active;
                        user.sex = row.fk_SexNavigation.Name;
                        user.role = row.fk_RoleNavigation.Name;
                        user.consultory = row.fk_ConsultoryNavigation != null ? row.fk_ConsultoryNavigation.Name : null;
                        user.type = row.fk_TypeNavigation != null ? row.fk_TypeNavigation.Name : null;

                        ////Respuesta para el cliente y manejo de alertas
                        //response.Success = true;

                        //propiedad para validar que el usuario tenga sesión iniciada
                        row.Login = true;
                        row.LastLog = DateTime.Now;

                        //guardamos los cambios en la base de datos
                        db.SaveChanges();
                    }
                }
            }
            return user;
        }

        public Response Logout(Credentials credentials) 
        {
            Response response = new Response();
            response.Success = false;

            if (credentials != null) 
            {
                var row = db.Users
                    .Where(u => u.Email == credentials.email)
                    .FirstOrDefault();

                if (row != null) 
                { 
                    row.Login = false;
                    db.SaveChanges();

                    response.Message = "Se ha cerrado sesión";
                    response.Success = true;
                }
            }
            return response;
        }

        //Método para crear un usuario
        public Response CreateUser(User user)
        {

            Response response = new Response();

            //Por defecto esta propiedad será False
            response.Success = false;

            //Manejo en caso de nulos
            if (user != null)
            {
                var row = db.Users
                    .Where(e => e.Email == user.email)
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
                        Name = user.name,
                        Email = user.email,
                        Password = user.password,
                        Phone = user.phone,
                        fk_Sex = user.fk_Sex,
                        fk_Consultory = user.fk_Consultory,
                        fk_Role = user.fk_Role,
                        fk_Type = user.fk_Type,
                        Active = true,
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
                    response.Message = $"Ya existe un usuario registrado con este email '{user.email}', favor de introducir un email diferente";
                }
            }
            return response;
        }

        public void CloseSessions()
        {
            List<Users> users = db.Users.Where(u => u.Login).ToList();

            if(users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (user.LastLog != null) if (CalculateDays(user.LastLog.Value) >= 10) user.Login = false;
                    else user.Login = false;
                }
                db.SaveChanges();
            }
        }

        static int CalculateDays(DateTime date)
        {
            // Fecha actual sin hora
            DateTime fechaActual = DateTime.Now.Date;

            // Fecha recibida sin hora
            DateTime fechaParametro = date.Date;

            // Calcular diferencia
            TimeSpan diferencia = fechaActual - fechaParametro;

            // Retornar días transcurridos (puede ser negativo si la fecha es futura)
            return diferencia.Days;
        }
    }
}