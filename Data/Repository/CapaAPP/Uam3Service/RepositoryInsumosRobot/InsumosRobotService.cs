using ClosedXML.Excel;
using Data.Interfaces.CapaApp.InsertExcelBD;
using Data.Interfaces.CapaApp.Uam3.IAppInsumosRobot;
using Data.Interfaces.CapaApp.Uam3.IDataInsumosRobot;
using Helper;
using Shared;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Data.Repository.CapaAPP.Uam3Service.RepositoryInsumosRobot;

public class InsumosRobotService : IAppInsumosRobot
{
    private readonly IDataInsumosRobot _repo;
    private readonly ILogger _logger;
    private readonly RutaRobotOptions _rutasRobot;
    private readonly NombreArchivoRobotOptionsCapaAPP _nombresRobot;
    private bool _robotYaEjecutado = false;

    // Mantenemos el mapa solo para saber el NOMBRE DE LA TABLA.
    // Ya NO necesitamos el 'typeof(Clase)' porque la estructura la define el Excel.
    private static readonly Dictionary<string, string> _mapaTablas = new()
    {
        { "CrmPortal", "crm_portal" },
        { "SapErp", "sap_erp" },
        { "DwhFijo", "dwh_fijo" },
        { "DwhMovil", "dwh_movil" },
        { "Fenix", "fenix" },
        { "OpenUne", "open_une" },
        { "Orion", "orion" },
        { "SapGrc", "sap_grc" },
        { "Siebel", "siebel" },
        { "The", "the" },
        { "Iam", "iam" },
        { "BigData", "big_data" },
        { "Cbs", "cbs" },
        { "Cm", "cm" },
        { "Elk", "elk" },
        { "Pcrf", "pcrf" }
    };

    public InsumosRobotService(IDataInsumosRobot repo,
        ILogger logger, RutaRobotOptions rutasRobot,
        NombreArchivoRobotOptionsCapaAPP nombresRobot)
    {
        _repo = repo;
        _logger = logger;
        _rutasRobot = rutasRobot;
        _nombresRobot = nombresRobot;
    }


    // En InsumosRobotService.cs

    public Dictionary<string, string> ObtenerConfiguracionAppsActivas()
    {
        var appsActivas = new Dictionary<string, string>();

        var propsRuta = typeof(RutaRobotOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propsNombre = typeof(NombreArchivoRobotOptionsCapaAPP).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in propsRuta)
        {
            var nombreApp = prop.Name;

            // Obtenemos el nombre del archivo configurado
            var nombreArchivo = propsNombre.FirstOrDefault(p => p.Name == nombreApp)?.GetValue(_nombresRobot)?.ToString();

            // 1. Si no tiene configuración o nombre, saltar
            if (string.IsNullOrWhiteSpace(nombreArchivo)) continue;

            // 2. LA CONDICIÓN CLAVE: Si dice "NO", saltar
            if (nombreArchivo.Equals("NO", StringComparison.OrdinalIgnoreCase)) continue;

            // 3. Verificamos si existe mapeo a tabla de BD
            if (_mapaTablas.TryGetValue(nombreApp, out var nombreTablaBD))
            {
                // Agregamos a la lista que verá el usuario
                // Key: Nombre App (ej: SapErp), Value: Tabla (ej: sap_erp)

                // Opcional: Puedes poner nombres más bonitos manualmente o usar el nombre de la propiedad
                appsActivas.Add(nombreApp, nombreTablaBD);
            }
        }

        return appsActivas;
    }

    public void EjecutarCargaRobot(Action<string>? onProgress = null)
    {
        // Función local auxiliar para reportar (envía a Consola y al Action si existe)
        void Reportar(string mensaje)
        {
            Console.WriteLine(mensaje); // Mantiene el log del servidor
            onProgress?.Invoke(mensaje); // Envía el log a la UI
        }

        if (_robotYaEjecutado)
        {
            // Omitir lógica de ReadKey para web, o manejarlo diferente
            Reportar("El proceso ya fue ejecutado previamente en esta instancia.");
            // return; // Opcional, dependiendo de tu lógica
        }

        Reportar("--- INICIANDO EJECUCIÓN ROBOT ---");

        var propsRuta = typeof(RutaRobotOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propsNombre = typeof(NombreArchivoRobotOptionsCapaAPP).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in propsRuta)
        {
            var nombreApp = prop.Name;
            var rutaCarpeta = prop.GetValue(_rutasRobot)?.ToString();
            var nombreArchivo = propsNombre.FirstOrDefault(p => p.Name == nombreApp)?.GetValue(_nombresRobot)?.ToString();

            if (string.IsNullOrWhiteSpace(rutaCarpeta) || string.IsNullOrWhiteSpace(nombreArchivo)) continue;
            if (nombreArchivo.Equals("NO", StringComparison.OrdinalIgnoreCase)) continue;

            if (!_mapaTablas.TryGetValue(nombreApp, out var nombreTablaBD))
            {
                Reportar($"[SKIP] No hay mapeo de tabla para {nombreApp}");
                continue;
            }

            var archivoPath = Path.Combine(rutaCarpeta, nombreArchivo);

            if (!Directory.Exists(rutaCarpeta) || !File.Exists(archivoPath))
            {
                Reportar($"[ERROR] Archivo no encontrado: {nombreArchivo}");
                _logger.EscribirLog($"No se encontró: {archivoPath}");
                continue;
            }

            try
            {
                Reportar($"[PROCESANDO] {nombreArchivo}...");
                Reportar($"   -> Destino: Tabla '{nombreTablaBD}'");

                // Importar
                ImportarCapaAPPDinamico(archivoPath, nombreTablaBD);

                Reportar($"[OK] Carga finalizada para {nombreApp}.");
                _logger.EscribirLogRobotCapaApp($"OK: Importado {nombreArchivo} -> {nombreTablaBD}\n");
            }
            catch (Exception ex)
            {
                Reportar($"[FATAL] Error en {nombreApp}: {ex.Message}");
                _logger.EscribirLogRobotCapaApp($"ERROR: {nombreArchivo} -> {ex.Message}\n");
            }

            Reportar("------------------------------------------------");
        }

        _robotYaEjecutado = true;
        Reportar("--- PROCESO TERMINADO ---");
    }

    private void ImportarCapaAPPDinamico(string rutaExcel, string nombreTabla)
    {
        using var wb = new XLWorkbook(rutaExcel);
        // Intentamos tomar la primera hoja
        var ws = wb.Worksheets.First();

        var firstRowUsed = ws.FirstRowUsed();
        if (firstRowUsed == null) return;

        // 1. Obtener y Limpiar Encabezados (Para que sirvan como nombres de columnas SQL)
        var rawHeaders = firstRowUsed.CellsUsed().Select(c => c.GetString()).ToList();
        var cleanHeaders = new List<string>();
        var headerMap = new Dictionary<int, string>(); // Indice Excel -> Nombre Columna Limpio

        int colIndex = 0;
        foreach (var header in rawHeaders)
        {
            string clean = LimpiarNombreColumna(header);

            // Evitar duplicados (ej: si hay dos columnas "Fecha", la segunda será "Fecha_1")
            if (cleanHeaders.Contains(clean))
            {
                int count = 1;
                while (cleanHeaders.Contains($"{clean}_{count}")) count++;
                clean = $"{clean}_{count}";
            }

            cleanHeaders.Add(clean);
            headerMap[colIndex] = clean; // Guardamos qué índice del Excel corresponde a qué columna
            colIndex++;
        }

        // 2. Leer los datos
        var filasParaInsertar = new List<Dictionary<string, object?>>();

        // Saltamos la cabecera
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var rowData = new Dictionary<string, object?>();
            bool rowHasData = false;

            // Iteramos solo hasta la cantidad de encabezados que encontramos
            // Usamos indexación base 0 para nuestra lista cleanHeaders
            var cells = row.Cells(1, cleanHeaders.Count).ToList();

            for (int i = 0; i < cleanHeaders.Count; i++)
            {
                string colName = cleanHeaders[i];

                // Obtener valor de celda de forma segura
                // Nota: XLCell índice empieza en 1, nuestras listas en 0. 
                // Pero row.Cell(i+1) es más seguro.
                var cell = row.Cell(i + 1);

                var val = cell.GetValue<string>()?.Trim();

                if (string.IsNullOrEmpty(val))
                {
                    rowData[colName] = null;
                }
                else
                {
                    rowData[colName] = val;
                    rowHasData = true;
                }
            }

            if (rowHasData)
            {
                filasParaInsertar.Add(rowData);
            }
        }

        if (filasParaInsertar.Any())
        {
            // Enviamos al repositorio para crear tabla e insertar
            _repo.CreateAndBulkInsertDynamic(nombreTabla, Path.GetFileName(rutaExcel), cleanHeaders, filasParaInsertar);
        }
        else
        {
            _logger.EscribirLogRobotCapaApp($"El archivo {rutaExcel} no contiene filas de datos.");
        }
    }

    /// <summary>
    /// Convierte un encabezado de Excel en un nombre de columna SQL válido y limpio.
    /// </summary>
    private string LimpiarNombreColumna(string header)
    {
        if (string.IsNullOrWhiteSpace(header)) return "Columna_Sin_Nombre";

        // Reemplazar espacios y caracteres raros por guiones bajos
        string clean = Regex.Replace(header, @"[^a-zA-Z0-9_]", "_");

        // Eliminar guiones bajos duplicados o al inicio/fin
        clean = clean.Trim('_');

        // Si empieza con número, agregar prefijo
        if (char.IsDigit(clean[0])) clean = "C_" + clean;

        return clean;
    }
}