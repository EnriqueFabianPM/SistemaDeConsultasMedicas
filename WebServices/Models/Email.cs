using WebServices.Data;
using WebServices.Models; 

namespace WebServices.Models
{
    public class Email
    {
        public string subjectEmail { get; set; }

        public string bodyEmail { get; set; }

        public string messageEmail { get; set; }

        public User user { get; set; }

        public Consultory consultory { get; set; }

        public string fromto { get; set; }
    }
}
