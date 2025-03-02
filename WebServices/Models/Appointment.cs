namespace WebServices.Models
{
    public class Appointment
    {
        public int fk_Doctor { get; set; }
        public int fk_Patient { get; set; }
        public int fk_Schedule { get; set; }
        public string? Notes { get; set; }
    }
}

