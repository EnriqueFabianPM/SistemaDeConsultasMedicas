namespace CSM_BlazorHybridApp.ViewModels
{
    public class Credentials
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Authorization
    {
        public bool Success { get; set; }
        public UserResponse? User { get; set; }
    }

    public class NewUser
    {
        public int Id_User { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Fk_Sex { get; set; } = string.Empty;
        public int Fk_Role { get; set; } = 2;
    }

    public class ApiConfig
    {
        public int? IdApi { get; set; }
        public object? BodyParams { get; set; }
        public object? Param { get; set; }
    }

    // Para respuesta del Login
    public class UserResponse
    {
        public int Id_User { get; set; }
        public string Name { get; set; } = string.Empty;
        // Agrega más propiedades si las retorna tu API
    }

    // Para respuesta del Registro
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
    }
}
