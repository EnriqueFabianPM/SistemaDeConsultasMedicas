using SistemaDeConsultasMedicas.Models;
using Microsoft.SqlServer;
using Microsoft.EntityFrameworkCore;
using Newtonsoft;

namespace SistemaDeConsultasMedicas.Services
{
    public class Services
    {

        private readonly Consultories_System_Context db;

        public Services()
        {
            db = new Consultories_System_Context();
        }

    }

    //ViewModels

    public class User
    {
        public int Id_User { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int? Phone { get; set; }
        public int fk_Sex { get; set; }
        public int fk_Role { get; set; }
        public int? fk_Consultory { get; set; }
        public int? fk_Type { get; set; }
        public int? fk_Schedule { get; set; }
        public bool Active { get; set; }

    }

    public class Response
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
