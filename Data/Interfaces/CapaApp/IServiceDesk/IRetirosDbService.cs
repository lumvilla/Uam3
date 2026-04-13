using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServiceDesk;

public interface IRetirosDbService
{
    Task<IEnumerable<RetirosUserApp>> ObtenerDatosTemporalesBD();
    Task<bool> GenerarOrdenCambioBD(string app, string fecha);
    Task<bool> GuardarOCManualBD(string app, string fechaSeleccionada, string numeroOc, string estadoOc, string fechaOC);
}
