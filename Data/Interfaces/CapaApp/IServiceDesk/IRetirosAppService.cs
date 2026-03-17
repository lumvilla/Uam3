using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServiceDesk;

public interface IRetirosAppService
{

    Task<IEnumerable<RetirosUserApp>> ObtenerDatosTemporales();
    Task<bool> GenerarOrdenCambio(string app, string Fecha);

    Task<bool> GuardarOCManual(
      string app,
      string fechaSeleccionada,
      string numeroOc,
      string estadoOc,
      string fechaOC);


}
