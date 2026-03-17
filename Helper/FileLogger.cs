namespace Helper;

public class FileLogger : ILogger
{

    private readonly string _logDirectory;

    public FileLogger(string logDirectory)
    {
        _logDirectory = logDirectory;

        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public async Task EscribirLog(string mensaje)
    {
        string logFile = System.IO.Path.Combine(_logDirectory, $"log_Insumos{DateTime.Now:yyyyMMdd}.txt");

        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}";

        if (!File.Exists(logFile))
        {
            await File.WriteAllTextAsync(logFile, logMessage + Environment.NewLine);
        }
        else
        {
            await File.AppendAllTextAsync(logFile, logMessage + Environment.NewLine);
        }
    }


    public void EscribirLogPortal(string mensaje)
    {
        string logFile = Path.Combine(_logDirectory, $"log_Portal{DateTime.Now:yyyyMMdd}.txt\n");

        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}";
        File.AppendAllText(logFile, logMessage + Environment.NewLine);
    }

    public void EscribirLogRobotCapaApp(string mensaje)
    {
        string logFile = Path.Combine(_logDirectory, $"log_RobotCapaApp{DateTime.Now:yyyyMMdd}.txt");

        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}";
        File.AppendAllText(logFile, logMessage + Environment.NewLine);
    }

    public void EscribirLogTerceros(string mensaje)
    {
        string logFile = Path.Combine(_logDirectory, $"log_Terceros{DateTime.Now:yyyyMMdd}.txt");

        string logMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensaje}";
        File.AppendAllText(logFile, logMessage + Environment.NewLine);
    }


    public  async Task ExportUserTxtMessage(string message, string directoryPath, string fileName)
    {
        Directory.CreateDirectory(directoryPath);
        var filePath = Path.Combine(directoryPath, $"{fileName}_{DateTime.Now:yyyyMMdd}.txt");

        var line = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}\n";

        if (!File.Exists(filePath))
        {
            await File.WriteAllTextAsync(filePath, line + Environment.NewLine);
        }
        else
        {
            await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
        }
    }



    public static async Task ExportUserTxt<T>(IEnumerable<T> data, string directoryPath, string fileName)
    {
        if (data == null || !data.Any())
        {
            Console.WriteLine(" No hay datos para exportar.");
            return;
        }

        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(directoryPath, $"{fileName}_{DateTime.Now:yyyyMMdd}.txt");
        var properties = typeof(T).GetProperties();

        
        List<string> contenidoExistente = new();

        if (File.Exists(filePath))
        {
            contenidoExistente = (await File.ReadAllLinesAsync(filePath)).ToList();
        }
        else
        {
            contenidoExistente.Add(string.Join(" | ", properties.Select(p => p.Name)));
        }

        var nuevasLineas = new List<string>();
        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
            nuevasLineas.Add(string.Join(" | ", values));
        }

        contenidoExistente.AddRange(nuevasLineas);

       
        
        contenidoExistente.Add($"Total registros a deshabilitar : {data.Count()}\n");

        await File.WriteAllLinesAsync(filePath, contenidoExistente);

        // Mostrar en consola en tabla
        Console.WriteLine();
        Console.WriteLine(string.Join(" | ", properties.Select(p => p.Name)));
        Console.WriteLine(new string('-', 90));
        foreach (var item in data)
        {
            var values = properties.Select(p => p.GetValue(item)?.ToString() ?? "");
            Console.WriteLine(string.Join(" | ", values));
        }

        Console.WriteLine($" Total Usuarios : {data.Count()}\n");
        Console.WriteLine($" Archivo exportado en: {filePath}\n");
    }



}
