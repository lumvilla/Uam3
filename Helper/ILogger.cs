namespace Helper;

public interface ILogger
{
    // Métodos no estáticos de FileLogger
    Task EscribirLog(string mensaje);
    void EscribirLogPortal(string mensaje);
    void EscribirLogRobotCapaApp(string mensaje);
    void EscribirLogTerceros(string mensaje);
    Task ExportUserTxtMessage(string message, string directoryPath, string fileName);
}