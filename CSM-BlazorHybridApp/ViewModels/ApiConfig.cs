namespace CSM_BlazorHybridApp.ViewModels
{
    public class ApiConfig
    {
        public int? IdApi { get; set; } = 0;
        public object? BodyParams { get; set; } = new();
        public string? Param { get; set; } = "";
    }
}
