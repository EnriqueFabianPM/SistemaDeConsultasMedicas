namespace CSM_BlazorHybridApp.ViewModels;

public partial class Consultory
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Latitude { get; set; } = string.Empty;
    public string Length { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? fk_Municipality { get; set; } = 0;
    public bool Active { get; set; } = false;
}