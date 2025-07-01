using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using CSM_BlazorHybridApp.ViewModels;

namespace CSM_BlazorHybridApp.Components.Pages
{
    public partial class Login : ComponentBase
    {
        private bool IsLogin { get; set; } = true;

        private Credentials Credentials { get; set; } = new();

        private Authorization Authorization { get; set; } = new();

        private NewUser NewUser { get; set; } = new();

        private string ConfirmPassword { get; set; } = string.Empty;

        private ApiConfig Config { get; set; } = new();

        private readonly HttpClient _http;
        private readonly NavigationManager _nav;

        public Login(HttpClient http, NavigationManager nav)
        {
            _http = http;
            _nav = nav;
        }

        public async Task LoginAsync()
        {
            try
            {
                Config = new ApiConfig
                {
                    IdApi = 9,
                    BodyParams = Credentials,
                    Param = null
                };

                Console.WriteLine("Iniciando sesión...");

                var response = await _http.PostAsJsonAsync("callApiAsync", Config);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<UserResponse>();

                    if (result != null && result.Id_User != 0)
                    {
                        Console.WriteLine("Inicio de sesión exitoso");
                        Authorization = new Authorization
                        {
                            Success = true,
                            User = result
                        };
                        GoToIndex(result.Id_User);
                    }
                    else
                    {
                        Console.WriteLine("Credenciales incorrectas");
                        Authorization = new Authorization { Success = false, User = null };
                        Credentials = new Credentials();
                    }
                }
                else
                {
                    Console.WriteLine("Error en la petición de login");
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
                Config = new ApiConfig
                {
                    IdApi = 11,
                    BodyParams = NewUser,
                    Param = null
                };

                Console.WriteLine("Registrando usuario...");

                var response = await _http.PostAsJsonAsync("callApiAsync", Config);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ApiResponse>();

                    if (result != null && result.Success)
                    {
                        Console.WriteLine("Registro exitoso");

                        NewUser = new NewUser();
                        ConfirmPassword = string.Empty;

                        IsLogin = true;
                    }
                    else Console.WriteLine(result?.Message ?? "Error en el registro");
                }
                else Console.WriteLine("Error en la petición de registro");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void GoToIndex(int idUser)
        {
            var root = _nav.BaseUri;
            _nav.NavigateTo($"{root}?id={idUser}", forceLoad: true);
        }
    }
}