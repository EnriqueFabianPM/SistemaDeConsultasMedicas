using Microsoft.AspNetCore.Components;
using CSM_BlazorHybridApp.ViewModels;
using CSM_BlazorHybridApp.Services;

namespace CSM_BlazorHybridApp.Components.Pages 
{
    public partial class Appointments : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int? Id { get; set; } = 0;

        private readonly Service _Service = new();

        // Propiedades del estado
        private object map = new(); // Aquí usarás JSInterop para Google Maps
        private List<Municipality>? municipalities = new();
        private List<Consultory>? consultories = new();
        private List<User>? doctors = new();
        private List<Appointment>? appointments = new();
        private int? selectedMunicipality;
        private int? selectedConsultory;
        private int? selectedDoctor;
        private string notes = string.Empty;
        private bool isLoading = false;
        private bool showSuccessMessage = false;
        private bool showErrorMessage = false;
        private string errorMessage = string.Empty;
        private List<Status>? statuses = new();
        private Dictionary<int, int> currentStatuses = new();
        private Authorization Authorization = new();
        private ApiConfig config = new();
        private User user = new();


        protected async override Task OnInitializedAsync()
        {
            // Simular asignación de user desde JS o contexto
            // En Blazor Server o WASM podrías obtenerlo del contexto o llamada a JS
            user = await GetUserAsync();

            if (user != null && user.Id_User > 0)
            {
                Authorization.Success = true;
                // Si quieres puedes asignar Authorization.User aquí
                if (user.fk_Role != 2)
                {
                    await GetAppointments();
                }
            }

            await GetMunicipalities();
            await GetStatuses();
        }

        private async Task GetMunicipalities()
        {
            config = new()
            {
                IdApi = 1,
                BodyParams = null,
                Param = null
            };
            municipalities = await _Service.ConsumeApi<List<Municipality>>(config);
        }

        private async Task GetStatuses()
        {
            config = new()
            {
                IdApi = 14,
                BodyParams = null,
                Param = null
            };
            statuses = await _Service.ConsumeApi<List<Status>>(config);
        }

        private async Task GetConsultories()
        {
            config = new()
            {
                IdApi = 2,
                BodyParams = null,
                Param = selectedMunicipality?.ToString()
            };
            consultories = await _Service.ConsumeApi<List<Consultory>>(config);
        }

        private async Task GetDoctors()
        {
            config = new()
            {
                IdApi = 3,
                BodyParams = null,
                Param = selectedConsultory?.ToString()
            };
            doctors = await _Service.ConsumeApi<List<User>>(config);
        }

        private async Task GetAppointments()
        {
            config = new()
            {
                IdApi = 4,
                BodyParams = null,
                Param = user.Id_User.ToString()
            };
            appointments = await _Service.ConsumeApi<List<Appointment>>(config);

            currentStatuses.Clear();
            if (appointments != null)
            {
                foreach (var appointment in appointments)
                {
                    currentStatuses[appointment.Id] = appointment.fk_Status;
                }
            }

            // Aquí podrías usar JSInterop para inicializar DataTables si usas jQuery
        }

        private async Task SubmitAppointment()
        {
            isLoading = true;
            config = new()
            {
                IdApi = 5,
                BodyParams = new
                {
                    fk_Doctor = selectedDoctor,
                    fk_Patient = user.Id_User,
                    notes = notes,
                },
                Param = null
            };

            var response = await _Service.ConsumeApi<ApiResponse>(config);

            isLoading = false;

            if (response != null && response.Success)
            {
                // Puedes usar un componente de alertas o llamar a JS para SweetAlert
                showSuccessMessage = true;
                // Por ejemplo, recargar la página o refrescar datos
                await GetAppointments();
            }
            else
            {
                showErrorMessage = true;
                errorMessage = response?.Message ?? "Error al enviar cita";
            }
        }

        private async Task UpdateAppointment(Appointment appointment)
        {
            if (appointment != null)
            {
                appointment.fk_Status = currentStatuses[appointment.Id];

                config = new()
                {
                    IdApi = 13,
                    BodyParams = new
                    {
                        id_Appointment = appointment.Id,
                        appointment.fk_Doctor,
                        appointment.fk_Patient,
                        appointment.fk_Status
                    },
                    Param = null
                };

                var response = await _Service.ConsumeApi<ApiResponse>(config);
                if (response != null && response.Success)
                {
                    showSuccessMessage = true;
                    // Recarga o refresca como gustes
                    await GetAppointments();
                }
                else
                {
                    showErrorMessage = true;
                    errorMessage = response?.Message ?? "Error al actualizar cita";
                }
            }
        }

        private async Task DeleteAppointment(Appointment appointment)
        {
            config = new()
            {
                IdApi = 15,
                BodyParams = new
                {
                    id_Appointment = appointment.Id,
                    appointment.fk_Doctor,
                    appointment.fk_Patient
                },
                Param = null
            };

            var response = await _Service.ConsumeApi<ApiResponse>(config);
            if (response != null && response.Success)
            {
                showSuccessMessage = true;
                await GetAppointments();
            }
            else
            {
                showErrorMessage = true;
                errorMessage = response?.Message ?? "Error al borrar cita";
            }
        }

        // Método simulado para obtener usuario, reemplaza según tu contexto
        private Task<User> GetUserAsync()
        {
            // Podrías obtener desde JSInterop o servicio autenticación
            return Task.FromResult(new User { Id_User = 1, fk_Role = 1 });
        }
    }
}