using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesDisplayService : ICrucesDisplayService
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly ICrucesExcel _crucesExcel;

    public CrucesDisplayService(
        ConnectionFactory factory,
        ILogger logger,
        IConfiguration config,
        ICrucesExcel crucesExcel)
    {
        _factory = factory;
        _logger = logger;
        _config = config;
        _crucesExcel = crucesExcel;
    }

    private async Task GuardarTempRetirosApp(IEnumerable<RetirosUserApp> usuarios)
    {
        if (usuarios == null || !usuarios.Any()) return;

        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");

            // Nota: ahora la comprobacion incluye login_app y tipo_cruce para no "fusionar" login vs cedula
            var sqlInsert = @"
                INSERT INTO retiro_app_temp (
                    fecha_ejecucion, app, cedula_app, cedula_consolidado, login_consolidado, login_app, 
                    fecha_retiro, estado_consolidado, estado_app, nombre_consolidado, tipo_cruce
                )
                SELECT 
                    @FechaEjecucion, @App, @CedulaApp, @CedulaConsolidado, @LoginConsolidado, @LoginApp, 
                    @FechaRetiro, @EstadoConsolidado, @EstadoApp, @NombreConsolidado, @TipoCruce
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_app_temp t
                    WHERE DATE(t.fecha_ejecucion) = DATE(@FechaEjecucion)
                      AND t.app = @App COLLATE NOCASE
                      AND t.cedula_consolidado = @CedulaConsolidado
                      AND COALESCE(t.login_app, '') = COALESCE(@LoginApp, '')
                      AND COALESCE(t.tipo_cruce, '') = COALESCE(@TipoCruce, '')
                );";

            var parametros = usuarios.Select(r => new
            {
                FechaEjecucion = fechaHoy,
                r.App,
                r.CedulaApp,
                r.CedulaConsolidado,
                r.LoginConsolidado,
                r.LoginApp,
                r.FechaRetiro,
                r.EstadoConsolidado,
                r.EstadoApp,
                r.NombreConsolidado,
                r.TipoCruce
            });

            int filas = await connection.ExecuteAsync(sqlInsert, parametros, transaction);

            transaction.Commit();
            //_logger.LogInformation($"Proceso Temp finalizado. Se insertaron {filas} registros nuevos.");
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            //_logger.LogError($"Error insertando en Temp: {ex.Message}");
            throw;
        }
    }

    public async Task<HashSet<string>> ObtenerRegistrosProcesados()
    {
        try
        {
            using var conexion = _factory.CreateConnection();

            var sql = @"
                SELECT DISTINCT 
                    app || '|' || COALESCE(cedula_consolidado, '') || '|' || COALESCE(login_app, '') || '|' || COALESCE(tipo_cruce, '') as Key
                FROM retiro_app_temp
                WHERE DATE(fecha_ejecucion) = DATE('now')";

            var registros = await conexion.QueryAsync<string>(sql);
            return new HashSet<string>(registros);
        }
        catch (Exception ex)
        {
            //_logger.LogError($"Error obteniendo registros procesados: {ex.Message}");
            return new HashSet<string>();
        }
    }

    public async Task<bool> ProcesarUsuariosSeleccionados(string app, string tipoCruce, int cantidad = 50)
    {
        try
        {
            var usuarios = await ObtenerUsuariosPorAppYTipo(app, tipoCruce, cantidad);

            if (!usuarios.Any())
            {
                //_logger.LogWarning($"No hay usuarios para procesar en {app} con tipo {tipoCruce}");
                return false;
            }

            var retirosUserApps = usuarios.Select(u => new RetirosUserApp
            {
                App = app,
                CedulaApp = u.CedulaApp,
                CedulaConsolidado = u.CedulaConsolidado,
                LoginConsolidado = u.LoginConsolidado,
                LoginApp = u.LoginApp,
                NombreConsolidado = u.NombreConsolidado,
                FechaRetiro = u.FechaRetiro,
                EstadoConsolidado = u.EstadoConsolidado,
                EstadoApp = u.EstadoApp,
                FechaEjecucion = DateTime.Now,
                TipoCruce = u.TipoCruce ?? ""
            });

            await GuardarTempRetirosApp(retirosUserApps);

            return true;
        }
        catch (Exception ex)
        {
            //_logger.LogError($"Error procesando usuarios seleccionados: {ex.Message}");
            throw;
        }
    }

    private async Task<IEnumerable<UsuarioDisableDto>> ObtenerUsuariosPorAppYTipo(string app, string tipoCruce, int cantidad)
    {
        using var conexion = _factory.CreateConnection();

        string whereClause = tipoCruce.ToUpper() switch
        {
            "AMBOS" => "",
            "LOGIN" => "AND u.TipoCruce = 'LOGIN'",
            "CEDULA" => "AND u.TipoCruce = 'CEDULA'",
            _ => ""
        };

        var queryBase = app.ToLower() switch
        {
            "portal" => _crucesExcel.GetQueryPortal(),
            "da" => _crucesExcel.GetQueryDA(),
            "siebelmovil" => _crucesExcel.GetQuerySiebelMovil(),
            "siebelfijo" => _crucesExcel.GetQuerySiebelFijo(),
            "fenix" => _crucesExcel.GetQueryFenix(),
            "open" => _crucesExcel.GetQueryOpen(),
            "dwhmovil" => _crucesExcel.GetQueryDwhMovil(),
            "dwhfijo" => _crucesExcel.GetQueryDwhFijo(),
            "saperp" => _crucesExcel.GetQuerySapErp(),
            "sapgrc" => _crucesExcel.GetQuerySapGrc(),
            "iam" => _crucesExcel.GetQueryIam(),
            _ => throw new ArgumentException($"Aplicación no valida: {app}")
        };

        var sqlFinal = $@"
                            WITH UsuariosApp AS (
                                {queryBase}
                            )
                            SELECT * FROM UsuariosApp u
                            WHERE NOT EXISTS (
                                SELECT 1 FROM retiro_app_temp t
                                WHERE t.app = '{app}' COLLATE NOCASE
                                AND t.cedula_consolidado = u.CedulaConsolidado
                                AND COALESCE(t.login_app, '') = COALESCE(u.LoginApp, '')
                                AND COALESCE(t.tipo_cruce, '') = COALESCE(u.TipoCruce, '')
                                AND DATE(t.fecha_ejecucion) = DATE('now')
                            )
                            {whereClause}
                            ORDER BY u.FechaRetiro DESC
                            LIMIT @Cantidad";

        return await conexion.QueryAsync<UsuarioDisableDto>(sqlFinal, new { Cantidad = cantidad });
    }

    // Ejemplo: ajuste en UsuariosSiebelMovilDisable para no eliminar filas con ROW_NUMBER innecesario
    

}
