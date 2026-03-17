using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Shared.CapaAplicacion.Portal;

public class Portal
{
    public int IdPotal { get; set; }
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string? Origen { get; set; }
    public string? Nombre { get; set; }
    public string? Cedula { get; set; }
    public string? Login { get; set; }

   // public string? DatosJson { get; set; }

                    //nombre TEXT,
                    //cedula INTEGER,
                    //login TEXT
}
