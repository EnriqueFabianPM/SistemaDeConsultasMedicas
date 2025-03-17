namespace WebServices.Models
#pragma warning disable CS8618

{
    public class User
    {
        public int id_User { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string? password { get; set; }
        public int? phone { get; set; }
        public int fk_Sex { get; set; }
        public int fk_Role { get; set; }
        public int? fk_Consultory { get; set; }
        public int? fk_Type { get; set; }
        public bool active { get; set; }
        public string sex { get; set; }
        public string role { get; set; }
        public string? consultory { get; set; }
        public string? type { get; set; }
    }
}
