namespace CSM_BlazorHybridApp.ViewModels
{
    public class NewUser
    {
        public int Id_User { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int fk_Sex { get; set; } = 0;
        public int fk_Role { get; set; } = 2;
    }
}
