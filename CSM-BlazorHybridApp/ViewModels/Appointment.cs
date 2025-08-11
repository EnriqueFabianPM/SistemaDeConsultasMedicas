namespace CSM_BlazorHybridApp.ViewModels;

public partial class Appointment
{
    public int Id { get; set; } = 0;
    public int fk_Doctor { get; set; } = 0;
    public int fk_Patient { get; set; } = 0;
    public int fk_Status { get; set; } = 0;
    public DateTime Created_Date { get; set; } 
    public DateTime Appointment_Date { get; set; }
    public string Notes { get; set; } = string.Empty;
}