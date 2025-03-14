namespace SistemaDeConsultasMedicas.ViewModels
{
    //Clase que se utiliza como parámetro para consumir cualquier tipo de api
    public class Api
    {
        public int Id_API { get; set; }
        public string? Name { get; set; }
        public string? URL { get; set; }
        public bool IsGet { get; set; }
        public bool IsPost { get; set; }
        public object? BodyParams { get; set; } //Por si el parámetro es un objeto
        public string? Param { get; set; } //Por si el parámetro es un valor
    }
}
