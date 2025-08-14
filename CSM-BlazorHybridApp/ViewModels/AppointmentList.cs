namespace CSM_BlazorHybridApp.ViewModels;

public partial class AppointmentList
{
    public int id { get; set; } = 0;
    public int fk_Doctor { get; set; } = 0;
    public int fk_Patient { get; set; } = 0;
    public string patient { get; set; } = string.Empty;
    public int fk_Status { get; set; } = 0;
    public string created { get; set; } = string.Empty;
    public string assigned { get; set; } = string.Empty;
    public string status { get; set; } = string.Empty;
}