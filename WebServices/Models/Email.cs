using WebServices.Data;
using WebServices.Models;
#pragma warning disable CS8618

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
