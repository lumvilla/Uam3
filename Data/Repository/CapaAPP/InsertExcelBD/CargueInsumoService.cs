//// Data.Services.CapaApp/CargueService.cs
//using Data.Interfaces.CapaApp.InsertExcelBD;
//using Helper;
//using Shared;
//using Shared.CapaAplicacion.DA;
//using Shared.CapaAplicacion.Robot;
//using System.Reflection;

//namespace Data.Repository.CapaAPP.InsertExcelBD;

//public class CargueInsumoService : ICargueInsumos
//{
//    private readonly IExcelImporter _importer; // tu interfaz
//    private readonly ILogger _logger;
//    private readonly string _rutaExcel;
//    private readonly string _rutaCsv;
//    private readonly string _rutaTerceros;
//    private readonly string _rutaDa;
//    private readonly RutaRobotOptions _rutasRobot;
//    private readonly NombreArchivoRobotOptionsCapaAPP _nombresRobot;

//    private bool _portalYaEjecutado = false;
//    private bool _tercerosYaEjecutado = false;
//    private bool _excelYaEjecutado = false;
//    private bool _daYaEjecutado = false;
//    private bool _robotYaEjecutado = false;

//    // dicionario para mapear nombre de tb en la bd
//    private static readonly Dictionary<string, (string Tabla, Type Tipo)> _mapaApps =
//        new()
//        {
//            { "CrmPortal", ("crm_portal", typeof(CrmPortal)) },
//            { "SapErp", ("sap_erp", typeof(SapErp)) },
//            { "DwhFijo", ("dwh_fijo", typeof(DwhFijo)) },
//            { "DwhMovil", ("dwh_movil", typeof(DwhMovil)) },
//            { "Fenix", ("fenix", typeof(Fenix)) },
//            { "OpenUne", ("open_une", typeof(OpenUne)) },
//            { "Orion", ("orion", typeof(Orion)) },
//            { "SapGrc", ("sap_grc", typeof(SapGrc)) },
//            { "Siebel", ("siebel", typeof(Siebel)) },
//            { "The", ("the", typeof(The)) },
//            { "Iam", ("iam", typeof(Iam)) }
//        };

//    public CargueInsumoService(
//        IExcelImporter importer,
//        ILogger logger,
//        string rutaExcel,
//        string RutaDA,
//        string rutaCsv,
//        string rutaTerceros, 
//        RutaRobotOptions rutasRobot,
//        NombreArchivoRobotOptionsCapaAPP nombresRobot)
//    {
//        _importer = importer;
//        _logger = logger;
//        _rutaExcel = rutaExcel;
//        _rutaDa = RutaDA;
//        _rutaCsv = rutaCsv;
//        _rutaTerceros = rutaTerceros;            
//        _rutasRobot = rutasRobot;
//        _nombresRobot = nombresRobot;
//    }

//    public void EjecutarCargaVinculados()
//        => EjecutarCargaGenerica(_rutaExcel, "*.xlsx", _importer.ImportarVinculadoDesdeExcel, ref _excelYaEjecutado);
//    public void EjecutarCargaDA()
//        => EjecutarCargaGenerica(_rutaDa, "*.xlsx", _importer.ImportarDADesdeExcel, ref _daYaEjecutado);

//    public void EjecutarCargaTerceros()
//        => EjecutarCargaGenerica(_rutaTerceros, "*.xlsx", _importer.ImportarTerceroDesdeExcel, ref _tercerosYaEjecutado);

//    public void EjecutarCargaPortal()
//        => EjecutarCargaGenerica(_rutaCsv, "*.csv", _importer.ImportarDesdecsvComoExcel, ref _portalYaEjecutado);



//    public void EjecutarCargaRobot()
//    {
//        if (_robotYaEjecutado)
//        {
//            Console.WriteLine(" Ya ejecutaste el proceso de carga de Insunos del Robot antes ¿Repetir toda la carga Robot? (Y/N)");
//            if (Console.ReadKey(true).Key != ConsoleKey.Y)
//            {
//                Console.WriteLine("Cancelado.");
//                return;
//            }
//        }

//        var propsRuta = typeof(RutaRobotOptions).GetProperties(BindingFlags.Public | BindingFlags.Instance);
//        var propsNombre = typeof(NombreArchivoRobotOptionsCapaAPP).GetProperties(BindingFlags.Public | BindingFlags.Instance);

//        foreach (var prop in propsRuta)
//        {
//            var nombreApp = prop.Name;
//            var rutaCarpeta = prop.GetValue(_rutasRobot)?.ToString();
//            var nombreArchivo = propsNombre.FirstOrDefault(p => p.Name == nombreApp)?.GetValue(_nombresRobot)?.ToString();

//            if (string.IsNullOrWhiteSpace(rutaCarpeta) || string.IsNullOrWhiteSpace(nombreArchivo))
//            {
//                Console.WriteLine($" No hay configuracion para {nombreApp}");
//                continue;
//            }

//            if (nombreArchivo.Equals("NO", StringComparison.OrdinalIgnoreCase))
//            {
//                Console.WriteLine($" Omitiendo {nombreApp} — esta configurado como NO.");
//                continue;
//            }

//            if (!_mapaApps.TryGetValue(nombreApp, out var config))
//            {
//                Console.WriteLine($" No hay mapeo tabla/tipo para {nombreApp}");
//                continue;
//            }

//            var archivo = Path.Combine(rutaCarpeta, nombreArchivo);

//            if (!Directory.Exists(rutaCarpeta))
//            {
//                Console.WriteLine($"La carpeta {rutaCarpeta} no existe.");
//                _logger.EscribirLog($"ERROR: La carpeta {rutaCarpeta} no existe.");
//                Console.ReadKey();
//                return;
//            }

//            if (!File.Exists(archivo))
//            {
//                Console.WriteLine($"No se encontro {nombreArchivo} en {rutaCarpeta}");
//                _logger.EscribirLogRobotCapaApp($"INFO: No se encontro {nombreArchivo} en {rutaCarpeta}");
//                continue;
//            }

//            try
//            {
//                Console.WriteLine($"Procesando {nombreArchivo} para {nombreApp} ...");

//                // Llamamos al método genérico ImportarCapaAPPDesdeExcel<T>(archivo, tabla)
//                var metodo = _importer.GetType()
//                    .GetMethod(nameof(IExcelImporter.ImportarCapaAPPDesdeExcel))
//                    .MakeGenericMethod(config.Tipo);

//                metodo.Invoke(_importer, new object[] { archivo, config.Tabla });

//                _logger.EscribirLogRobotCapaApp($"OK: Importado {nombreArchivo} -> {config.Tabla}\n");
//                // ya no escribimos controlFile
//            }
//            catch (TargetInvocationException tie)
//            {
//                Console.WriteLine($"ERROR: {tie.InnerException?.Message ?? tie.Message}");
//                _logger.EscribirLogRobotCapaApp($"ERROR: {nombreArchivo} -> {tie.InnerException ?? tie}\n");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"ERROR procesando {archivo}: {ex.Message}");
//                _logger.EscribirLogRobotCapaApp($"ERROR: {nombreArchivo} -> {ex} \n");
//            }
//        }

//        _robotYaEjecutado = true;

//        Console.WriteLine("Proceso Robot terminado. Presione una tecla...");
//        Console.ReadKey();
//    }



//    private void EjecutarCargaGenerica(string carpeta, string filtro, Action<string> metodoImport, ref bool yaEjecutadoFlag)
//    {
//        Console.Clear();
//        Console.WriteLine($">>> Iniciando carga desde: {carpeta}");

//        if (yaEjecutadoFlag)
//        {
//            Console.WriteLine(" Ya ejecutaste este proceso. ¿Repetir? (Y/N)");
//            if (Console.ReadKey(true).Key != ConsoleKey.Y)
//            {
//                Console.WriteLine("Cancelado");
//                return;
//            }
//        }

//        if (!Directory.Exists(carpeta))
//        {
//            Console.WriteLine($"La carpeta {carpeta} no existe.");
//            _logger.EscribirLog($"ERROR: La carpeta {carpeta} no existe.");
//            Console.ReadKey();
//            return;
//        }

//        var archivos = Directory.GetFiles(carpeta, filtro);
//        if (archivos.Length == 0)
//        {
//            Console.WriteLine($"No se encontraron archivos {filtro}.");
//            _logger.EscribirLog($"INFO: No se encontraron archivos {filtro} en {carpeta}");
//            Console.ReadKey();
//            return;
//        }

//        foreach (var archivo in archivos)
//        {
//            try
//            {
//                Console.WriteLine($"Procesando {Path.GetFileName(archivo)}...");
//                metodoImport(archivo);
//                _logger.EscribirLog($"OK: Importado {Path.GetFileName(archivo)}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"ERROR procesando {archivo}: {ex.Message}");
//                _logger.EscribirLog($"ERROR: {Path.GetFileName(archivo)} -> {ex}");
//            }
//        }

//        yaEjecutadoFlag = true;
//        Console.WriteLine("Proceso terminado. Presione una tecla...");
//        Console.ReadKey();
//    }




//}
