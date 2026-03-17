using Shared.CapaAplicacion.Interfaces;

namespace Shared.CapaAplicacion.DA;

public class DirectorioActivo 
{
    public int IdDA { get; set; }
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string? Origen { get; set; }
    public string Login { get; set; } = string.Empty;
    public string? Identificacion { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Estado { get; set; } 
    public DateTime? UltimoLogon { get; set; }
    public DateTime? Expira { get; set; }



}
