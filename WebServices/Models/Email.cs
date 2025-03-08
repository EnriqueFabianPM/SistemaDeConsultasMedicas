using WebServices.Data;
using WebServices.Models; 

namespace WebServices.Models
{
    public class Email
    {
        public string subject { get; set; }

        public string body { get; set; }

        public User? user { get; set; }

        public Consultory? consultory { get; set; }
    }
}
