using CSM_BlazorHybridApp.Services;
using CSM_BlazorHybridApp.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CSM_BlazorHybridApp.Components.Pages 
{
    public partial class Appointments : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int? Id { get; set; } = 0;

        private readonly Service _Service = new();
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _nav;

        // Propiedades del estado
        private object map = new(); // Aquí usarás JSInterop para Google Maps
        private List<Municipality>? municipalities = new();
        private List<Consultory>? consultories = new();
        private List<Doctor>? doctors = new();
        private List<AppointmentList>? appointments = new();
        private int? selectedMunicipality;
        private int? selectedConsultory;
        private int? selectedDoctor;
        private string notes = string.Empty;
        private bool isLoading = false;
        private List<Status>? statuses = new();
        private Dictionary<int, int> currentStatuses = new();
        private Authorization Authorization = new();
        private ApiConfig config = new();
        private UserResponse? user = new();

        public Appointments(NavigationManager nav, IJSRuntime jsRuntime)
        {
            _nav = nav;
            _jsRuntime = jsRuntime;
        }

        protected async override Task OnInitializedAsync()
        {
            // Simular asignación de user desde JS o contexto
            // En Blazor Server o WASM podrías obtenerlo del contexto o llamada a JS
            await GetUserAsync();

            if (user != null && user.id_User > 0)
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

        private async Task GenerateMapAsync()
        {
            await _jsRuntime.InvokeVoidAsync("generateMap");
        }

        private async Task UpdateMapMarkersAsync()
        {
            var consultory = consultories?.FirstOrDefault(o => o.id == selectedConsultory) ?? null;
            if (consultory != null)
            {
                await _jsRuntime.InvokeVoidAsync("updateMapMarkers", consultory.latitude, consultory.length, consultory.name);
            }
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
            doctors = await _Service.ConsumeApi<List<Doctor>>(config);
        }

        private async Task GetAppointments()
        {
            config = new()
            {
                IdApi = 4,
                BodyParams = null,
                Param = user?.id_User.ToString()
            };
            appointments = await _Service.ConsumeApi<List<AppointmentList>>(config);

            currentStatuses.Clear();
            if (appointments != null)
            {
                foreach (var appointment in appointments)
                {
                    currentStatuses[appointment.id] = appointment.fk_Status;
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
                    fk_Patient = user?.id_User,
                    notes,
                },
                Param = null
            };

            var response = await _Service.ConsumeApi<ApiResponse>(config);

            isLoading = false;

            if (response != null && response.Success)
            {
                await JS.InvokeVoidAsync("showSweetAlert", "Operación exitosa", "La cita se a generado correctamente", "success", "Cerrar");

                await GetAppointments();
            }
        }

        private async Task UpdateAppointment(AppointmentList appointment)
        {
            if (appointment != null)
            {
                appointment.fk_Status = currentStatuses[appointment.id];

                config = new()
                {
                    IdApi = 13,
                    BodyParams = new
                    {
                        id_Appointment = appointment.id,
                        appointment.fk_Doctor,
                        appointment.fk_Patient,
                        appointment.fk_Status
                    },
                    Param = null
                };

                var response = await _Service.ConsumeApi<ApiResponse>(config);
                if (response != null && response.Success)
                {
                    await GetAppointments();
                }
            }
        }

        private async Task DeleteAppointment(AppointmentList appointment)
        {
            config = new()
            {
                IdApi = 15,
                BodyParams = new
                {
                    id_Appointment = appointment.id,
                    appointment.fk_Doctor,
                    appointment.fk_Patient
                },
                Param = null
            };

            var response = await _Service.ConsumeApi<ApiResponse>(config);
            if (response != null && response.Success)
            {
                await GetAppointments();
            }
        }

        // Método simulado para obtener usuario, reemplaza según tu contexto
        private async Task GetUserAsync()
        {
            config = new()
            {
                IdApi = 12,
                BodyParams = null,
                Param = Id.ToString()
            };
            user = await _Service.ConsumeApi<UserResponse>(config);
        }
    }
}