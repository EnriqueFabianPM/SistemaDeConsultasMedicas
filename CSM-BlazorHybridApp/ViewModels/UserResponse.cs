namespace CSM_BlazorHybridApp.ViewModels
{
    // Para respuesta del Login
    public class UserResponse
    {
        public int id_User { get; set; } = 0;
        public string name { get; set; } = "";
        public string email { get; set; } = "";
        public string? password { get; set; } = "";
        public string? phone { get; set; } = "";
        public int fk_Sex { get; set; } = 0;
        public int fk_Role { get; set; } = 0;
        public int? fk_Consultory { get; set; } = 0;
        public int? fk_Type { get; set; } = 0;
        public bool active { get; set; } = false;
        public string sex { get; set; } = "";
        public string role { get; set; } = "";
        public string? consultory { get; set; } = "";
        public string? type { get; set; } = "";
    }
}
