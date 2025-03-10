namespace WebServices.Models
{
    public class Appointment
    {
        public int? id_Appointment { get; set; }
        public int fk_Doctor { get; set; }
        public int fk_Patient { get; set; }
        public int fk_Schedule { get; set; }
        public string? notes { get; set; }
        public int fk_Status { get; set; }
    }
}

