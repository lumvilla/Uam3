using Shared.CapaAplicacion.UserDisable;

namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesDisplayServiceNoSox
{
    Task<bool> ProcesarUsuariosSeleccionadosNoSox(string app, string tipoCruce, int cantidad = 50);
    Task<HashSet<string>> ObtenerRegistrosProcesadosNoSox();
}
