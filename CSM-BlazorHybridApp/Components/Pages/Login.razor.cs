using Microsoft.AspNetCore.Components;
using CSM_BlazorHybridApp.ViewModels;

namespace CSM_BlazorHybridApp.Components.Pages
{
    public partial class Login : ComponentBase
    {
        private readonly NavigationManager _nav;
        private readonly Services.Services _services = new();

        private bool IsLogin { get; set; } = true;
        private Credentials Credentials { get; set; } = new();
        private Authorization Authorization { get; set; } = new();
        private NewUser NewUser { get; set; } = new();
        private string ConfirmPassword { get; set; } = string.Empty;
        public Login(NavigationManager nav)
        {
            _nav = nav;
        }

        public async Task LoginAsync()
        {
            try
            {
                var config = new ApiConfig
                {
                    IdApi = 9,
                    BodyParams = Credentials
                };

                var result = await _services.ConsumeApi<UserResponse>(config);
                if (result != null && result.id_User != 0)
                {
                    Console.WriteLine("Inicio de sesión exitoso");

                    Authorization = new Authorization
                    {
                        Success = true,
                        User = result
                    };

                    GoToAppointments(result.id_User);
                }
                else
                {
                    Console.WriteLine("Credenciales incorrectas");

                    Authorization = new Authorization
                    {
                        Success = false,
                        User = null
                    };

                    Credentials = new Credentials();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public async Task RegisterAsync()
        {
            if (NewUser.Password != ConfirmPassword)
            {
                Console.WriteLine("Las contraseñas no coinciden");
                return;
            }

            try
            {
                var config = new ApiConfig
                {
                    IdApi = 11,
                    BodyParams = NewUser
                };

                var result = await _services.ConsumeApi<ApiResponse>(config);
                if (result != null && result.Success)
                {
                    Console.WriteLine("Registro exitoso");

                    NewUser = new NewUser();
                    ConfirmPassword = string.Empty;
                    IsLogin = true;
                }
                else
                {
                    Console.WriteLine(result?.Message ?? "Error en el registro");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void GoToAppointments(int idUser)
        {
            _nav.NavigateTo($"/Appointments?id={idUser}");
        }
    }
}
