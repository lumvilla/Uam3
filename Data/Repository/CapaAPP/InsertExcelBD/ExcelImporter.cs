//using ClosedXML.Excel;
//using Data.Interfaces.CapaApp.InsertExcelBD;
//using Helper;
//using Shared.CapaAplicacion.DA;
//using Shared.CapaAplicacion.Interfaces;
//using Shared.CapaAplicacion.Portal;
//using Shared.CapaAplicacion.Terceros;
//using Shared.CapaAplicacion.Vinculados;
//using System.IO.Compression;
//using System.Xml.Linq;

//namespace Data.Repository.CapaAPP.InsertExcelBD;

//public class ExcelImporter : IExcelImporter
//{
//    private readonly IDatosExcelRepository _repo;
//    private readonly ILogger _logger;
//    public ExcelImporter(IDatosExcelRepository repo, ILogger logger)
//    {
//        _repo = repo;
//        _logger = logger;
//    }


//    public void ImportarVinculadoDesdeExcel(string rutaExcel)
//    {
//        using var workbook = new XLWorkbook(rutaExcel);
//        var ws = workbook.Worksheets.First();

//        var listaVinculados = new List<Vinculados>();

//        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 3))
//        {
//            string? SafeString(IXLCell cell)
//            {
//                var value = cell.GetString()?.Trim();
//                return string.IsNullOrEmpty(value) ? null : value;
//            }

//            var vinculado = new Vinculados
//            {
//                FechaCargue = DateTime.Now,
//                Origen = Path.GetFileName(rutaExcel),
//                FechaRetiro = SafeString(row.Cell(2)),
//                Cedula = SafeString(row.Cell(4)),
//                Company = SafeString(row.Cell(5)),
//                Nombre = SafeString(row.Cell(3))
//            };

//            listaVinculados.Add(vinculado);
//        }

//        if (listaVinculados.Any())
//        {
//            _repo.InsertVinculadosBulk(listaVinculados);
//            _logger.EscribirLog($" Se insertaron {listaVinculados.Count} registros desde archivo {rutaExcel}");
//        }
//    }

//    public void ImportarDADesdeExcel(string rutaExcel)
//    {
//        using var workbook = new XLWorkbook(rutaExcel);
//        var ws = workbook.Worksheets.First();

//        var listaDA = new List<DirectorioActivo>();

//        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 2))
//        {
//            string? SafeString(IXLCell cell)
//            {
//                var value = cell.GetString()?.Trim();
//                return string.IsNullOrEmpty(value) ? null : value;
//            }

//            var da = new DirectorioActivo
//            {
//                FechaCargue = DateTime.Now,
//                Origen = Path.GetFileName(rutaExcel),
//                Login = SafeString(row.Cell(1)),
//                Identificacion = SafeString(row.Cell(2)),
//                NombreCompleto = SafeString(row.Cell(3)),
//                Estado = SafeString(row.Cell(4))
//            };

//            listaDA.Add(da);
//        }

//        if (listaDA.Any())
//        {
//            _repo.InsertDABulk(listaDA);
//            _logger.EscribirLog($" Se insertaron {listaDA.Count} registros desde archivo {rutaExcel}");
//        }
//    }






//    public void ImportarTerceroDesdeExcel(string rutaTercero)
//    {
//        using var workbook = new XLWorkbook(rutaTercero);
//        var ws = workbook.Worksheets.First();

//        var listaTerceros = new List<Terceros>();

//        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 5)) // salto los encabezados
//        {
//            string? SafeString(IXLCell cell)
//            {
//                var value = cell.GetString()?.Trim();
//                return string.IsNullOrEmpty(value) ? null : value;
//            }

//            var tercero = new Terceros
//            {
//                FechaCargue = DateTime.Now,
//                Origen = Path.GetFileName(rutaTercero),
//                Login = row.Cell(2).GetString(),
//                EstadoEntidad = row.Cell(4).GetString(),
//                FechaRetiro = row.Cell(6).GetString(),
//                NombreCompleto = $"{row.Cell(21).GetString()} {row.Cell(22).GetString()}".Trim(),
//                Cedula = row.Cell(12).GetString()
//                //SEGUNDO_NOMBRE = row.Cell(21).GetString(),
//                //SEGUNDO_APELLIDO = row.Cell(22).GetString(),
//                //DatosJson = System.Text.Json.JsonSerializer.Serialize(new
//                //{
//                //    // se esatn saltando algunas columnas por estar unidas
//                //    //IMR_USERDN = row.Cell(1).GetString(),   
//                //    LOGINID = row.Cell(2).GetString(),     
//                //    ESTADO_IDENTIDAD = row.Cell(4).GetString(),  
//                //    FECHA_ELIMINACION = row.Cell(5).GetString(),
//                //    FECHA_RETIRO = row.Cell(6).GetString(),
//                //    //FECHA_CREACIONIDM = row.Cell(7).GetString(),
//                //    FECHA_INGRESO = row.Cell(9).GetString(),
//                //    CADUCIDAD = row.Cell(10).GetString(),
//                //    TIPO_IDENTIFICACION = row.Cell(11).GetString(),
//                //    NUM_IDENTIFICACION = row.Cell(12).GetString(),
//                //    TIPO_USUARIO = row.Cell(13).GetString(),
//                //    NOM_EMPRESA = row.Cell(14).GetString(),
//                //    CANAL_UNIDAD = row.Cell(15).GetString(),
//                //    RECURSO = row.Cell(16).GetString(),
//                //    CARGO = row.Cell(17).GetString(),
//                //    JEFE_LOGINID = row.Cell(19).GetString(),
//                //    USER_LOGINCRM = row.Cell(20).GetString(),
//                //    SEGUNDO_NOMBRE = row.Cell(21).GetString(),
//                //    SEGUNDO_APELLIDO = row.Cell(22).GetString(),
//                //    PROVEEDOR = row.Cell(23).GetString()
//                //})
//            };

//            listaTerceros.Add(tercero);
//        }

//        if (listaTerceros.Any())
//        {
//            _repo.InsertTercerosBulk(listaTerceros);
//            _logger.EscribirLog($" Se insertaron {listaTerceros.Count} registros desde archivo {rutaTercero}");
//        }
//    }




//    public void ImportarDesdecsvComoExcel(string rutaCsv)
//    {
//        //leo el csv
//        var lineas = File.ReadAllLines(rutaCsv);

//        //  creo un archivo excel temporal en memoria
//        using var wb = new XLWorkbook();
//        var ws = wb.Worksheets.Add("Datos");

//        int fila = 1;
//        foreach (var linea in lineas)
//        {
//            var columnas = linea.Split('|');
//            for (int col = 0; col < columnas.Length; col++)
//            {
//                ws.Cell(fila, col + 1).Value = columnas[col];
//            }
//            fila++;
//        }

//        var portales = new List<Portal>();

//        foreach (var row in ws.RowsUsed().Skip(1))
//        {
//            string? SafeString(IXLCell cell)
//            {
//                var value = cell.GetString()?.Trim();
//                return string.IsNullOrEmpty(value) ? null : value;
//            }
//            var portal = new Portal
//            {
//                FechaCargue = DateTime.Now,
//                Origen = Path.GetFileName(rutaCsv),
//                Nombre = row.Cell(3).GetString(),
//                Cedula = row.Cell(2).GetString(),
//                Login = row.Cell(1).GetString()
//                //DatosJson = System.Text.Json.JsonSerializer.Serialize(new
//                //{
//                //    Login = row.Cell(1).GetString(),
//                //    Cedula = row.Cell(2).GetString(),
//                //    Nombre = row.Cell(3).GetString(),
//                //    Fecha_Creacion = row.Cell(4).GetString(),
//                //    Fecha_Ultima_Modificacion = row.Cell(5).GetString(),
//                //    Fecha_Ultimo_Logueo = row.Cell(6).GetString(),
//                //    Comentario = row.Cell(7).GetString(),
//                //    Roles = row.Cell(8).GetString(),
//                //    Nombre_Rol = row.Cell(9).GetString()
//                //})
//            };

//            portales.Add(portal);
//        }

//        //  inseto todos en un solo insert masivo
//        _repo.BulkInsertPortal(portales);

//        _logger.EscribirLog($" Finalizada importacion de {rutaCsv} con {portales.Count} registros");
//    }


//    public void ImportarCapaAPPDesdeExcel<T>(string rutaExcel, string nombreTabla) where T : IRegistros, new()
//    {
//        using var wb = new XLWorkbook(rutaExcel);
//        var ws = wb.Worksheets.First();

//        // Obtengo la primera fila con datos reales (encabezados)
//        var firstRowUsed = ws.FirstRowUsed();
//        if (firstRowUsed == null)
//        {
//            _logger.EscribirLogRobotCapaApp($"No se encontraron filas con datos en {rutaExcel}");
//            return;
//        }

//        var headers = firstRowUsed.CellsUsed().Select(c => c.GetString()).ToList();

//        var registros = new List<T>();

//        // Solo recorro filas que realmente tienen celdas usadas (no vacías)
//        foreach (var row in ws.RowsUsed().Skip(1))
//        {
//            var data = new Dictionary<string, object?>();
//            var cells = row.Cells(1, headers.Count).ToList();

//            for (int i = 0; i < headers.Count; i++)
//            {
//                var header = headers[i];
//                var cell = i < cells.Count ? cells[i] : null;
//                var cellValue = cell == null || cell.IsEmpty() ? null : cell.GetString();
//                data[header] = string.IsNullOrWhiteSpace(cellValue) ? null : cellValue;
//            }

//            // Si toda la fila está vacía, la ignoro
//            if (data.Values.All(v => v == null))
//                continue;

//            var registro = new T
//            {
//                FechaCargue = DateTime.Now,
//                Origen = Path.GetFileName(rutaExcel),
//                DatosJson = System.Text.Json.JsonSerializer.Serialize(data)
//            };
//            registros.Add(registro);
//        }

//        if (registros.Any())
//        {
//            _repo.BulkInsertDatoCapaApp(registros, nombreTabla);
//            _logger.EscribirLogRobotCapaApp($" Finalizada importación de {rutaExcel} con {registros.Count} registros en tabla {nombreTabla}");
//        }
//        else
//        {
//            _logger.EscribirLogRobotCapaApp($"No se encontraron registros válidos en {rutaExcel} para importar en {nombreTabla}");
//        }
//    }

    
    
//}
