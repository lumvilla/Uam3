namespace Shared.CapaAplicacion.Interfaces;

public interface IRegistros
{
    DateTime FechaCargue { get; set; }
    string? Origen { get; set; }
    string? DatosJson { get; set; }

}
