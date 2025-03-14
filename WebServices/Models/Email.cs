using WebServices.Data;
using WebServices.Models;
#pragma warning disable CS8618

namespace WebServices.Models
{
    public class Email
    {
        public string subject { get; set; } //Asunto del correo
        public string body { get; set; } //Cuerpo del correo en HTML
        public User? user { get; set; } //Datos del usuario para personalización
        public Consultory? consultory { get; set; } //Datos del consultorio para personalización
    }
}
