using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesDisplayServiceNoSox : ICrucesDisplayServiceNoSox
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;
    private readonly ICrucesExcelNoSox _crucesNoSox;

    public CrucesDisplayServiceNoSox(
        ConnectionFactory factory, ILogger logger,
        IConfiguration config, ICrucesExcelNoSox crucesNoSox)
    {
        _factory     = factory;
        _logger      = logger;
        _config      = config;
        _crucesNoSox = crucesNoSox;
    }

    public async Task<HashSet<string>> ObtenerRegistrosProcesadosNoSox()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = @"
                SELECT DISTINCT
                    app || '|' || COALESCE(cedula_consolidado,'') || '|' || COALESCE(login_app,'') || '|' || COALESCE(tipo_cruce,'') AS Key
                FROM retiro_nosox_temp
                WHERE DATE(fecha_ejecucion) = DATE('now')";
            return new HashSet<string>(await conexion.QueryAsync<string>(sql));
        }
        catch { return new HashSet<string>(); }
    }

    public async Task<bool> ProcesarUsuariosSeleccionadosNoSox(string app, string tipoCruce, int cantidad = 50)
    {
        try
        {
            var usuarios = await ObtenerUsuariosPorAppYTipo(app, tipoCruce, cantidad);
            if (!usuarios.Any()) return false;

            var retiros = usuarios.Select(u => new RetirosUserApp
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

            await GuardarTempNoSox(retiros);
            return true;
        }
        catch { throw; }
    }

    private async Task GuardarTempNoSox(IEnumerable<RetirosUserApp> usuarios)
    {
        if (usuarios == null || !usuarios.Any()) return;
        using var connection = _factory.CreateConnection();
        await connection.OpenAsync();
        using var transaction = connection.BeginTransaction();

        try
        {
            var fechaHoy = DateTime.Now.ToString("yyyy-MM-dd");
            var sql = @"
                INSERT INTO retiro_nosox_temp (
                    fecha_ejecucion, app, cedula_app, cedula_consolidado, login_consolidado, login_app,
                    fecha_retiro, estado_consolidado, estado_app, nombre_consolidado, tipo_cruce
                )
                SELECT @FechaEjecucion, @App, @CedulaApp, @CedulaConsolidado, @LoginConsolidado, @LoginApp,
                       @FechaRetiro, @EstadoConsolidado, @EstadoApp, @NombreConsolidado, @TipoCruce
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_nosox_temp t
                    WHERE DATE(t.fecha_ejecucion) = DATE(@FechaEjecucion)
                      AND t.app = @App COLLATE NOCASE
                      AND t.cedula_consolidado = @CedulaConsolidado
                      AND COALESCE(t.login_app,'')  = COALESCE(@LoginApp,'')
                      AND COALESCE(t.tipo_cruce,'') = COALESCE(@TipoCruce,'')
                );";

            await connection.ExecuteAsync(sql, usuarios.Select(r => new
            {
                FechaEjecucion = fechaHoy, r.App, r.CedulaApp, r.CedulaConsolidado,
                r.LoginConsolidado, r.LoginApp, r.FechaRetiro,
                r.EstadoConsolidado, r.EstadoApp, r.NombreConsolidado, r.TipoCruce
            }), transaction);

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

        // Grupo A
        var grupoA = new[] { "nosox_gtc","nosox_open_edatel","nosox_open_etp","nosox_osm","nosox_mss","nosox_tigo_gestion","nosox_viafirma" };
        // Grupo B
        var grupoB = new[] { "nosox_maebog1","nosox_maebog2","nosox_maebaq","nosox_maecore" };

        var queryBase = app.ToLower() switch
        {
            var a when grupoA.Contains(a) => _crucesNoSox.GetQueryNoSoxGrupoA(a),
            var b when grupoB.Contains(b) => _crucesNoSox.GetQueryNoSoxGrupoB(b),
            "nosox_service_desk"          => _crucesNoSox.GetQueryNoSoxGrupoC("nosox_service_desk"),
            _ => throw new ArgumentException($"App No Sox no válida: {app}")
        };

        var sqlFinal = $@"
            WITH UsuariosApp AS ( {queryBase} )
            SELECT * FROM UsuariosApp u
            WHERE NOT EXISTS (
                SELECT 1 FROM retiro_nosox_temp t
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
