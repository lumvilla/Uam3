namespace Data.Interfaces.CapaApp.Uam3.IAppInsumosRobot;

public interface IAppInsumosRobot
{
    // Ejecuta el robot con TODOS los sistemas activos en appsettings
    void EjecutarCargaRobot(Action<string>? onProgress = null);

    // Ejecuta el robot solo con los sistemas seleccionados
    void EjecutarCargaRobotFiltrado(HashSet<string> appsSeleccionadas, Action<string>? onProgress = null);

    // Cargue manual desde stream (UI upload)
    Task ImportarArchivoManual(Stream stream, string nombreArchivo, string nombreTabla, Action<string>? onProgress = null);

    // Retorna sistemas activos (NombreArchivoRobotCapaAPP != "NO")
    Dictionary<string, string> ObtenerConfiguracionAppsActivas();
}
