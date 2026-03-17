using ClosedXML.Excel;
using Data.Interfaces.CapaApp.ICargueInsumosRetiros;
using Data.Interfaces.CapaApp.InsertExcelBD;
using Helper;
using Shared;
using Shared.CapaAplicacion.DA;
using Shared.CapaAplicacion.Robot;
using Shared.CapaAplicacion.Terceros;
using Shared.CapaAplicacion.Vinculados;

namespace Data.Repository.CapaAPP.InsumosRetiros;

public class InsumosRetiroService : IRetirosInsumos
{

    private readonly IDatosExcelRepository _repo;
    private readonly ILogger _logger;

    public InsumosRetiroService(IDatosExcelRepository repo, ILogger logger)
    {
        _repo = repo;
        _logger = logger;
    }


    // Modificación para Vinculados
    public async Task ImportarVinculadoDesdeExcel(Stream fileStream, string fileName)
    {
        // 1. Convertir el Stream asíncrono de Blazor a un MemoryStream (síncrono)
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Rewind para que ClosedXML pueda leer desde el inicio

        // 2. Usamos el MemoryStream con ClosedXML (Ahora funcionará, ya que MemoryStream soporta lectura síncrona)
        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaVinculados = new List<Vinculados>();

        // El resto de la lógica de lectura sigue igual
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 3))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var vinculado = new Vinculados
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                FechaRetiro = SafeString(row.Cell(2)),
                Cedula = SafeString(row.Cell(4)),
                Company = SafeString(row.Cell(5)),
                Nombre = SafeString(row.Cell(3))
            };

            listaVinculados.Add(vinculado);
        }

        if (listaVinculados.Any())
        {
            _repo.InsertVinculadosBulk(listaVinculados);
            _logger.EscribirLog($" Se insertaron {listaVinculados.Count} registros desde archivo {fileName}");
        }
    }

    // Modificación para Directorio Activo (DA)
    public async Task ImportarDADesdeExcel(Stream fileStream, string fileName)
    {
        // 1. Convertir a MemoryStream de forma asíncrona
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // 2. Usamos el MemoryStream con ClosedXML
        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaDA = new List<DirectorioActivo>();

        // El resto de la lógica de lectura sigue igual
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 2))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var da = new DirectorioActivo
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                Login = SafeString(row.Cell(1)),
                Identificacion = SafeString(row.Cell(2)),
                NombreCompleto = SafeString(row.Cell(3)),
                Estado = SafeString(row.Cell(4))
            };

            listaDA.Add(da);
        }

        if (listaDA.Any())
        {
            _repo.InsertDABulk(listaDA);
            _logger.EscribirLog($" Se insertaron {listaDA.Count} registros desde archivo {fileName}");
        }
    }

    // Modificación para Terceros
    public async Task ImportarTerceroDesdeExcel(Stream fileStream, string fileName)
    {
        // 1. Convertir a MemoryStream de forma asíncrona
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // 2. Usamos el MemoryStream con ClosedXML
        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaTerceros = new List<Terceros>();

        // El resto de la lógica de lectura sigue igual
        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 5))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            var tercero = new Terceros
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                Login = SafeString(row.Cell(2)),
                EstadoEntidad = SafeString(row.Cell(4)),
                FechaRetiro = SafeString(row.Cell(6)),
                NombreCompleto = $"{SafeString(row.Cell(21))} {SafeString(row.Cell(22))}".Trim(),
                Cedula = SafeString(row.Cell(12))
            };

            listaTerceros.Add(tercero);
        }

        if (listaTerceros.Any())
        {
            _repo.InsertTercerosBulk(listaTerceros);
            _logger.EscribirLog($" Se insertaron {listaTerceros.Count} registros desde archivo {fileName}");
        }
    }


}
