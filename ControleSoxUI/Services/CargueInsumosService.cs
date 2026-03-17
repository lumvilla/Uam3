using Data.Interfaces.CapaApp.InsertExcelBD;
using Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControleSoxUI.Services
{
        public interface ICargueInsumosService
        {
            Task<CargueResultado> CargarArchivoAsync(string rutaArchivo, TipoInsumo tipoInsumo, IProgress<string>? progress = null);
            Task<List<Dictionary<string, object>>> ObtenerDatosAsync(TipoInsumo tipoInsumo, string? filtro = null);
        }

        public enum TipoInsumo
        {
            DirectorioActivo,
            Vinculados,
            Terceros
        }

        public class CargueResultado
        {
            public bool Exitoso { get; set; }
            public string Mensaje { get; set; } = string.Empty;
            public int RegistrosCargados { get; set; }
            public string? Error { get; set; }
        }

        public class CargueInsumosService : ICargueInsumosService
        {
            private readonly IExcelImporter _importer;
            private readonly FileLogger _logger;

            public CargueInsumosService(IExcelImporter importer, FileLogger logger)
            {
                _importer = importer;
                _logger = logger;
            }

            public async Task<CargueResultado> CargarArchivoAsync(
                string rutaArchivo,
                TipoInsumo tipoInsumo,
                IProgress<string>? progress = null)
            {
                return await Task.Run(() =>
                {
                    var resultado = new CargueResultado();

                    try
                    {
                        if (!File.Exists(rutaArchivo))
                        {
                            resultado.Exitoso = false;
                            resultado.Mensaje = "El archivo no existe";
                            resultado.Error = $"No se encontró el archivo: {rutaArchivo}";
                            return resultado;
                        }

                        progress?.Report($"Iniciando carga de {Path.GetFileName(rutaArchivo)}...");

                        // Ejecutar la importación según el tipo
                        switch (tipoInsumo)
                        {
                            case TipoInsumo.DirectorioActivo:
                                progress?.Report("Procesando Directorio Activo...");
                                _importer.ImportarDADesdeExcel(rutaArchivo);
                                break;

                            case TipoInsumo.Vinculados:
                                progress?.Report("Procesando Vinculados...");
                                _importer.ImportarVinculadoDesdeExcel(rutaArchivo);
                                break;

                            case TipoInsumo.Terceros:
                                progress?.Report("Procesando Terceros...");
                                _importer.ImportarTerceroDesdeExcel(rutaArchivo);
                                break;

                            default:
                                throw new ArgumentException("Tipo de insumo no válido");
                        }

                        progress?.Report("Carga completada exitosamente");

                        resultado.Exitoso = true;
                        resultado.Mensaje = $"Archivo cargado correctamente: {Path.GetFileName(rutaArchivo)}";
                        // Aquí podrías obtener el número de registros si el importer lo devuelve
                        resultado.RegistrosCargados = 0; // TODO: Implementar contador real

                        _logger.EscribirLog($"OK: Importado {Path.GetFileName(rutaArchivo)} - {tipoInsumo}");
                    }
                    catch (Exception ex)
                    {
                        resultado.Exitoso = false;
                        resultado.Mensaje = "Error al cargar el archivo";
                        resultado.Error = ex.Message;

                        _logger.EscribirLog($"ERROR: {Path.GetFileName(rutaArchivo)} - {tipoInsumo} -> {ex.Message}");
                        progress?.Report($"Error: {ex.Message}");
                    }

                    return resultado;
                });
            }

            public async Task<List<Dictionary<string, object>>> ObtenerDatosAsync(
                TipoInsumo tipoInsumo,
                string? filtro = null)
            {
                // TODO: Implementar consulta a la base de datos según el tipo
                // Por ahora retornamos lista vacía
                return await Task.FromResult(new List<Dictionary<string, object>>());
            }
        }
    
}
