using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesDisplayServiceBD : ICrucesDisplayServiceBD
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly ICrucesExcelBD _crucesExcelBD;

    public CrucesDisplayServiceBD(
        ConnectionFactory factory,
        ILogger logger,
        IConfiguration config,
        ICrucesExcelBD crucesExcelBD)
    {
        _factory       = factory;
        _logger        = logger;
        _config        = config;
        _crucesExcelBD = crucesExcelBD;
    }

    public async Task<HashSet<string>> ObtenerRegistrosProcesadosBD()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = @"
                SELECT DISTINCT
                    app || '|' || COALESCE(cedula_consolidado,'') || '|' || COALESCE(login_app,'') || '|' || COALESCE(tipo_cruce,'') AS Key
                FROM retiro_db_temp
                WHERE DATE(fecha_ejecucion) = DATE('now')";

            var registros = await conexion.QueryAsync<string>(sql);
            return new HashSet<string>(registros);
        }
        catch { return new HashSet<string>(); }
    }

    public async Task<bool> ProcesarUsuariosSeleccionadosBD(string app, string tipoCruce, int cantidad = 50)
    {
        try
        {
            var usuarios = await ObtenerUsuariosPorAppYTipo(app, tipoCruce, cantidad);
            if (!usuarios.Any()) return false;

            var retirosUserApps = usuarios.Select(u => new RetirosUserApp
            {
                App               = app,
                CedulaApp         = u.CedulaApp,
                CedulaConsolidado = u.CedulaConsolidado,
                LoginConsolidado  = u.LoginConsolidado,
                LoginApp          = u.LoginApp,
                NombreConsolidado = u.NombreConsolidado,
                FechaRetiro       = u.FechaRetiro,
                EstadoConsolidado = u.EstadoConsolidado,
                EstadoApp         = u.EstadoApp,
                FechaEjecucion    = DateTime.Now,
                TipoCruce         = u.TipoCruce ?? ""
            });

            await GuardarTempRetirosDB(retirosUserApps);
            return true;
        }
        catch { throw; }
    }

    private async Task GuardarTempRetirosDB(IEnumerable<RetirosUserApp> usuarios)
    {
        if (usuarios == null || !usuarios.Any()) return;

        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");

            var sqlInsert = @"
                INSERT INTO retiro_db_temp (
                    fecha_ejecucion, app, cedula_app, cedula_consolidado, login_consolidado, login_app,
                    fecha_retiro, estado_consolidado, estado_app, nombre_consolidado, tipo_cruce
                )
                SELECT
                    @FechaEjecucion, @App, @CedulaApp, @CedulaConsolidado, @LoginConsolidado, @LoginApp,
                    @FechaRetiro, @EstadoConsolidado, @EstadoApp, @NombreConsolidado, @TipoCruce
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_db_temp t
                    WHERE DATE(t.fecha_ejecucion) = DATE(@FechaEjecucion)
                      AND t.app = @App COLLATE NOCASE
                      AND t.cedula_consolidado = @CedulaConsolidado
                      AND COALESCE(t.login_app,'')   = COALESCE(@LoginApp,'')
                      AND COALESCE(t.tipo_cruce,'')  = COALESCE(@TipoCruce,'')
                );";

            var parametros = usuarios.Select(r => new
            {
                FechaEjecucion    = fechaHoy,
                r.App, r.CedulaApp, r.CedulaConsolidado,
                r.LoginConsolidado, r.LoginApp, r.FechaRetiro,
                r.EstadoConsolidado, r.EstadoApp, r.NombreConsolidado, r.TipoCruce
            });

            await connection.ExecuteAsync(sqlInsert, parametros, transaction);
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
    }

    private async Task<IEnumerable<UsuarioDisableDto>> ObtenerUsuariosPorAppYTipo(string app, string tipoCruce, int cantidad)
    {
        using var conexion = _factory.CreateConnection();

        string whereClause = tipoCruce.ToUpper() switch
        {
            "LOGIN"  => "AND u.TipoCruce = 'LOGIN'",
            "CEDULA" => "AND u.TipoCruce = 'CEDULA'",
            _        => ""
        };

        var queryBase = app.ToLower() switch
        {
            "bd_siebel"   => _crucesExcelBD.GetQueryBD("bd_siebel"),
            "bd_portal"   => _crucesExcelBD.GetQueryBD("bd_portal"),
            "bd_dwh_fijo" => _crucesExcelBD.GetQueryBD("bd_dwh_fijo"),
            "bd_dwh_movil"=> _crucesExcelBD.GetQueryBD("bd_dwh_movil"),
            "bd_fenix"    => _crucesExcelBD.GetQueryBD("bd_fenix"),
            "bd_open"     => _crucesExcelBD.GetQueryBD("bd_open"),
            "bd_sap_erp"  => _crucesExcelBD.GetQueryBD("bd_sap_erp"),
            "bd_sap_grc"  => _crucesExcelBD.GetQueryBD("bd_sap_grc"),
            "bd_iam"      => _crucesExcelBD.GetQueryBD("bd_iam"),
            "bd_orion"    => _crucesExcelBD.GetQueryBD("bd_orion"),
            "bd_the"      => _crucesExcelBD.GetQueryBD("bd_the"),
            "bd_cbs"      => _crucesExcelBD.GetQueryBD("bd_cbs"),
            "bd_cm"       => _crucesExcelBD.GetQueryBD("bd_cm"),
            _ => throw new ArgumentException($"Aplicación BD no válida: {app}")
        };

        var sqlFinal = $@"
            WITH UsuariosApp AS ( {queryBase} )
            SELECT * FROM UsuariosApp u
            WHERE NOT EXISTS (
                SELECT 1 FROM retiro_db_temp t
                WHERE t.app = '{app}' COLLATE NOCASE
                  AND t.cedula_consolidado = u.CedulaConsolidado
                  AND COALESCE(t.login_app,'')  = COALESCE(u.LoginApp,'')
                  AND COALESCE(t.tipo_cruce,'') = COALESCE(u.TipoCruce,'')
                  AND DATE(t.fecha_ejecucion) = DATE('now')
            )
            {whereClause}
            ORDER BY u.FechaRetiro DESC
            LIMIT @Cantidad";

        return await conexion.QueryAsync<UsuarioDisableDto>(sqlFinal, new { Cantidad = cantidad });
    }
}
