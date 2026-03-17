using Shared.CapaAplicacion.Robot;

namespace Shared.CapaAplicacion.Terceros;

public class Terceros
{
    public int IdTerceros { get; set; }
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string? Origen { get; set; }
    public string? Login { get; set; }
    public string? EstadoEntidad { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Cedula { get; set; }
    public string? FechaRetiro { get; set; }

                    //login TEXT,
                    //estado_entidad TEXT,
                    //nombre_completo TEXT, -- este cmpo es la union de 2
                    //cedula INTEGER,
                    //fecha_retiro TEXT
    //public string? DatosJson { get; set; }


}
