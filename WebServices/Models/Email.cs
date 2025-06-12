#pragma warning disable CS8618

using WebServices.Data;

namespace WebServices.Models
{
    public class Email
    {
        public string subject { get; set; } //Asunto del correo
        public string body { get; set; } //Cuerpo del correo en HTML
        public string message { get; set; } //Mensaje del correo
        public Users? user { get; set; } //Datos del usuario para personalización
        public Consultories? consultory { get; set; } //Datos del consultorio para personalización
    }
}
