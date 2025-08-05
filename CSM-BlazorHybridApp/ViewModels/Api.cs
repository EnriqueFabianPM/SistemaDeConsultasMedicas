namespace CSM_BlazorHybridApp.ViewModels
{
    //Clase que se utiliza como parámetro para consumir cualquier tipo de api
    public class Api
    {
        public int Id_API { get; set; } = 0;
        public string? Name { get; set; } = "";
        public string? URL { get; set; } = "";
        public bool IsGet { get; set; } = false;
        public bool IsPost { get; set; } = false;
        public object? BodyParams { get; set; } = new(); //Por si el parámetro es un objeto
        public string Param { get; set; } = ""; //Por si el parámetro es un valor
    }
}
