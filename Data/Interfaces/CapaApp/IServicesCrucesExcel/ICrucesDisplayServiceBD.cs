namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesDisplayServiceBD
{
    Task<bool> ProcesarUsuariosSeleccionadosBD(string app, string tipoCruce, int cantidad = 50);
    Task<HashSet<string>> ObtenerRegistrosProcesadosBD();
}
