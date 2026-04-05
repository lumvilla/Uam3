using ClosedXML.Excel;
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

    private static readonly Dictionary<string, string> _mapaTablas = new()
    {
        { "CrmPortal", "crm_portal" },
        { "SapErp",    "sap_erp"    },
        { "DwhFijo",   "dwh_fijo"   },
        { "DwhMovil",  "dwh_movil"  },
        { "Fenix",     "fenix"      },
        { "OpenUne",   "open_une"   },
        { "Orion",     "orion"      },
        { "SapGrc",    "sap_grc"    },
        { "Siebel",    "siebel"     },
        { "The",       "the"        },
        { "Iam",       "iam"        },
        { "BigData",   "big_data"   },
        { "Cbs",       "cbs"        },
        { "Cm",        "cm"         },
        { "Elk",       "elk"        },
        { "Pcrf",      "pcrf"       }
    };

    public InsumosRobotService(IDataInsumosRobot repo, ILogger logger,
        RutaRobotOptions rutasRobot, NombreArchivoRobotOptionsCapaAPP nombresRobot)
    {
        _repo = repo;
        _logger = logger;
        _rutasRobot = rutasRobot;
        _nombresRobot = nombresRobot;
    }

    // ── Obtener sistemas activos ──
    public Dictionary<string, string> ObtenerConfiguracionAppsActivas()
    {
        var result = new Dictionary<string, string>();
        var propsRuta = typeof(RutaRobotOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propsNombre = typeof(NombreArchivoRobotOptionsCapaAPP).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in propsRuta)
        {
            var nombreApp = prop.Name;
            var nombreArchivo = propsNombre.FirstOrDefault(p => p.Name == nombreApp)?.GetValue(_nombresRobot)?.ToString();
            if (string.IsNullOrWhiteSpace(nombreArchivo)) continue;
            if (nombreArchivo.Equals("NO", StringComparison.OrdinalIgnoreCase)) continue;
            if (_mapaTablas.TryGetValue(nombreApp, out var tabla))
                result.Add(nombreApp, tabla);
        }
        return result;
    }

    // ── Robot completo ──
    public void EjecutarCargaRobot(Action<string>? onProgress = null)
        => EjecutarInterno(null, onProgress);

    // ── Robot filtrado por selección ──
    public void EjecutarCargaRobotFiltrado(HashSet<string> appsSeleccionadas, Action<string>? onProgress = null)
        => EjecutarInterno(appsSeleccionadas, onProgress);

    // ── Cargue manual desde stream (upload UI) ──
    public async Task ImportarArchivoManual(Stream stream, string nombreArchivo, string nombreTabla, Action<string>? onProgress = null)
    {
        void Log(string msg) { Console.WriteLine(msg); onProgress?.Invoke(msg); }

        Log($"[PROCESANDO] {nombreArchivo}...");
        Log($"   -> Destino: Tabla '{nombreTabla}'");

        try
        {
            // Copiar stream a memoria para ClosedXML
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            ImportarCapaAPPDinamico(ms, nombreArchivo, nombreTabla);

            Log($"[OK] Carga manual finalizada para '{nombreTabla}'.");
            _logger.EscribirLogRobotCapaApp($"OK: Importado manual {nombreArchivo} -> {nombreTabla}\n");
        }
        catch (Exception ex)
        {
            Log($"[ERROR] {ex.Message}");
            _logger.EscribirLogRobotCapaApp($"ERROR manual: {nombreArchivo} -> {ex.Message}\n");
            throw;
        }
    }

    // ── Lógica central robot ──
    private void EjecutarInterno(HashSet<string>? appsPermitidas, Action<string>? onProgress)
    {
        void Log(string msg) { Console.WriteLine(msg); onProgress?.Invoke(msg); }

        Log("--- INICIANDO EJECUCIÓN ROBOT ---");

        var propsRuta = typeof(RutaRobotOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var propsNombre = typeof(NombreArchivoRobotOptionsCapaAPP).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in propsRuta)
        {
            var nombreApp = prop.Name;
            var rutaCarpeta = prop.GetValue(_rutasRobot)?.ToString();
            var nombreArchivo = propsNombre.FirstOrDefault(p => p.Name == nombreApp)?.GetValue(_nombresRobot)?.ToString();

            if (string.IsNullOrWhiteSpace(rutaCarpeta) || string.IsNullOrWhiteSpace(nombreArchivo)) continue;
            if (nombreArchivo.Equals("NO", StringComparison.OrdinalIgnoreCase)) continue;

            if (appsPermitidas != null && !appsPermitidas.Contains(nombreApp))
            {
                Log($"[OMITIDO] {nombreApp} no fue seleccionado.");
                continue;
            }

            if (!_mapaTablas.TryGetValue(nombreApp, out var nombreTablaBD))
            {
                Log($"[SKIP] Sin mapeo de tabla para {nombreApp}");
                continue;
            }

            var archivoPath = Path.Combine(rutaCarpeta, nombreArchivo);

            if (!Directory.Exists(rutaCarpeta) || !File.Exists(archivoPath))
            {
                Log($"[ERROR] Archivo no encontrado: {archivoPath}");
                _logger.EscribirLog($"No se encontró: {archivoPath}");
                continue;
            }

            try
            {
                Log($"[PROCESANDO] {nombreArchivo}...");
                Log($"   -> Destino: Tabla '{nombreTablaBD}'");

                using var fs = File.OpenRead(archivoPath);
                ImportarCapaAPPDinamico(fs, nombreArchivo, nombreTablaBD);

                Log($"[OK] Carga finalizada para {nombreApp}.");
                _logger.EscribirLogRobotCapaApp($"OK: Importado {nombreArchivo} -> {nombreTablaBD}\n");
            }
            catch (Exception ex)
            {
                Log($"[FATAL] Error en {nombreApp}: {ex.Message}");
                _logger.EscribirLogRobotCapaApp($"ERROR: {nombreArchivo} -> {ex.Message}\n");
            }

            Log("------------------------------------------------");
        }

        Log("--- PROCESO TERMINADO ---");
    }

    // ── Importación dinámica desde Stream ──
    private void ImportarCapaAPPDinamico(Stream stream, string nombreArchivo, string nombreTabla)
    {
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.First();
        var firstRow = ws.FirstRowUsed();
        if (firstRow == null) return;

        var rawHeaders = firstRow.CellsUsed().Select(c => c.GetString()).ToList();
        var cleanHeaders = new List<string>();

        int colIdx = 0;
        foreach (var header in rawHeaders)
        {
            string clean = LimpiarNombreColumna(header);
            if (cleanHeaders.Contains(clean))
            {
                int n = 1;
                while (cleanHeaders.Contains($"{clean}_{n}")) n++;
                clean = $"{clean}_{n}";
            }
            cleanHeaders.Add(clean);
            colIdx++;
        }

        var filas = new List<Dictionary<string, object?>>();
        foreach (var row in ws.RowsUsed().Skip(1))
        {
            var rowData = new Dictionary<string, object?>();
            bool hasData = false;
            var lastCell = row.LastCellUsed()?.Address.ColumnNumber ?? 0;

            for (int i = 0; i < cleanHeaders.Count; i++)
            {
                var val = (i + 1 <= lastCell)
                    ? row.Cell(i + 1).GetValue<string>()?.Trim()
                    : null;
                if (string.IsNullOrEmpty(val)) { rowData[cleanHeaders[i]] = null; }
                else { rowData[cleanHeaders[i]] = val; hasData = true; }
            }
            if (hasData) filas.Add(rowData);
        }

        if (filas.Any())
            _repo.CreateAndBulkInsertDynamic(nombreTabla, nombreArchivo, cleanHeaders, filas);
        else
            _logger.EscribirLogRobotCapaApp($"El archivo {nombreArchivo} no contiene filas de datos.");
    }

    private string LimpiarNombreColumna(string header)
    {
        if (string.IsNullOrWhiteSpace(header)) return "Columna_Sin_Nombre";
        string clean = Regex.Replace(header, @"[^a-zA-Z0-9_]", "_").Trim('_');
        if (clean.Length > 0 && char.IsDigit(clean[0])) clean = "C_" + clean;
        return string.IsNullOrEmpty(clean) ? "Columna_Sin_Nombre" : clean;
    }
}
