namespace Data.Interfaces.CapaApp.IServicesCrucesExcel;

public interface ICrucesDisplayService
{


    Task<HashSet<string>> ObtenerRegistrosProcesados();

    Task<bool> ProcesarUsuariosSeleccionados(string app, string tipoCruce, int cantidad = 50);


   

}
