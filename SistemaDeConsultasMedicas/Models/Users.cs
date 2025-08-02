namespace SistemaDeConsultasMedicas.Models;

public class Users
{
    public int Id_User { get; set; } = 0;
    public string Name { get; set; } = "";
    public int fk_Sex { get; set; } = 0;
    public int fk_Role { get; set; } = 0;
}