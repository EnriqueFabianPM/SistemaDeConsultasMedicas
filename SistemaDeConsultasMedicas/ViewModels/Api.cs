namespace SistemaDeConsultasMedicas.ViewModels
{
    public class Api
    {
        public int Id_API { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }
        public bool IsGet { get; set; }
        public bool IsPost { get; set; }
        public object? BodyParams { get; set; }
    }
}
