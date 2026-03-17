using Shared.CapaAplicacion.Interfaces;
namespace Shared.CapaAplicacion.Robot;

public class DwhMovil : IRegistros
{
    public DateTime FechaCargue { get; set; } = DateTime.Now;
    public string? Origen { get; set; }
    public string? DatosJson { get; set; }

    //propiedades propia
    public int IdDwhMovil { get; set; }

}
