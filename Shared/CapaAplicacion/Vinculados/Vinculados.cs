namespace Shared.CapaAplicacion.Vinculados;

public class Vinculados
{
    public int IdVinculado { get; set; }
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string ?Origen { get; set; } 
    public string ? Nombre { get; set; } 
    public string ? Company { get; set; } 
    public string ? FechaRetiro { get; set; } 
    public string?  Cedula { get; set; } 

}
