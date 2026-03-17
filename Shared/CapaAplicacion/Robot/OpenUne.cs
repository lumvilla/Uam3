using Shared.CapaAplicacion.Interfaces;

namespace Shared.CapaAplicacion.Robot;

public class OpenUne : IRegistros
{
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string? Origen { get; set; }
    public string? DatosJson { get; set; }

    //propiedades propia
    public int IdOpenUne { get; set; }

}
