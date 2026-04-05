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

    public async Task ImportarVinculadoDesdeExcel(Stream fileStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaVinculados = new List<Vinculados>();

        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 3))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            listaVinculados.Add(new Vinculados
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                FechaRetiro = SafeString(row.Cell(2)),
                Cedula = SafeString(row.Cell(4)),
                Company = SafeString(row.Cell(5)),
                Nombre = SafeString(row.Cell(3))
            });
        }

        if (listaVinculados.Any())
        {
            _repo.InsertVinculadosBulk(listaVinculados);
            await _logger.EscribirLog($"Se insertaron {listaVinculados.Count} registros desde archivo {fileName}");
        }
    }

    public async Task ImportarDADesdeExcel(Stream fileStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaDA = new List<DirectorioActivo>();

        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 2))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            listaDA.Add(new DirectorioActivo
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                Login = SafeString(row.Cell(1)),
                Identificacion = SafeString(row.Cell(2)),
                NombreCompleto = SafeString(row.Cell(3)),
                Estado = SafeString(row.Cell(4))
            });
        }

        if (listaDA.Any())
        {
            _repo.InsertDABulk(listaDA);
            await _logger.EscribirLog($"Se insertaron {listaDA.Count} registros desde archivo {fileName}");
        }
    }

    public async Task ImportarTerceroDesdeExcel(Stream fileStream, string fileName)
    {
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var workbook = new XLWorkbook(memoryStream);
        var ws = workbook.Worksheets.First();

        var listaTerceros = new List<Terceros>();

        foreach (var row in ws.RowsUsed().Where(r => r.RowNumber() >= 5))
        {
            string? SafeString(IXLCell cell)
            {
                var value = cell.GetString()?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }

            listaTerceros.Add(new Terceros
            {
                FechaCargue = DateTime.Now,
                Origen = fileName,
                Login = SafeString(row.Cell(2)),
                EstadoEntidad = SafeString(row.Cell(4)),
                FechaRetiro = SafeString(row.Cell(6)),
                NombreCompleto = $"{SafeString(row.Cell(21))} {SafeString(row.Cell(22))}".Trim(),
                Cedula = SafeString(row.Cell(12))
            });
        }

        if (listaTerceros.Any())
        {
            _repo.InsertTercerosBulk(listaTerceros);
            await _logger.EscribirLog($"Se insertaron {listaTerceros.Count} registros desde archivo {fileName}");
        }
    }
}
