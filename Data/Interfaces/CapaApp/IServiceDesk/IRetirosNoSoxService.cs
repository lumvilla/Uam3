using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServiceDesk;

public interface IRetirosNoSoxService
{
    Task<IEnumerable<RetirosUserApp>> ObtenerDatosTemporalesNoSox();
    Task<bool> GenerarOrdenCambioNoSox(string app, string fecha);
    Task<bool> GuardarOCManualNoSox(string app, string fechaSeleccionada, string numeroOc, string estadoOc, string fechaOC);
}
