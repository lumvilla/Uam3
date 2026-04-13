using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesUserDisableBD : ICrucesExcelBD
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public CrucesUserDisableBD(ConnectionFactory factory, ILogger logger, IConfiguration config)
    {
        _factory = factory;
        _logger = logger;
        _config = config;
    }

    #region Métodos Públicos

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSiebelDisable()    => await EjecutarQuery(GetQueryBD("bd_siebel"),    "bd_siebel");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdPortalDisable()    => await EjecutarQuery(GetQueryBD("bd_portal"),    "bd_portal");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdDwhFijoDisable()   => await EjecutarQuery(GetQueryBD("bd_dwh_fijo"),  "bd_dwh_fijo");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdDwhMovilDisable()  => await EjecutarQuery(GetQueryBD("bd_dwh_movil"), "bd_dwh_movil");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdFenixDisable()     => await EjecutarQuery(GetQueryBD("bd_fenix"),     "bd_fenix");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdOpenDisable()      => await EjecutarQuery(GetQueryBD("bd_open"),      "bd_open");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSapErpDisable()    => await EjecutarQuery(GetQueryBD("bd_sap_erp"),   "bd_sap_erp");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdSapGrcDisable()    => await EjecutarQuery(GetQueryBD("bd_sap_grc"),   "bd_sap_grc");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdIamDisable()       => await EjecutarQuery(GetQueryBD("bd_iam"),       "bd_iam");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdOrionDisable()     => await EjecutarQuery(GetQueryBD("bd_orion"),     "bd_orion");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdTheDisable()       => await EjecutarQuery(GetQueryBD("bd_the"),       "bd_the");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdCbsDisable()       => await EjecutarQuery(GetQueryBD("bd_cbs"),       "bd_cbs");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBdCmDisable()        => await EjecutarQuery(GetQueryBD("bd_cm"),        "bd_cm");

    #endregion

    #region Query Genérico BD

    private async Task<IEnumerable<UsuarioDisableDto>> EjecutarQuery(string queryBase, string app)
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {queryBase} )
                SELECT *
                FROM (
                    SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                    FROM datos d
                    WHERE NOT EXISTS (
                        SELECT 1 FROM retiro_db_temp t
                        WHERE t.app = '{app}' COLLATE NOCASE
                          AND t.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                          AND DATE(t.fecha_ejecucion) = DATE('now')
                    )
                    AND NOT EXISTS (
                        SELECT 1 FROM retiro_db r
                        WHERE r.app = '{app}' COLLATE NOCASE
                          AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                          AND r.estado_oc = 'COMPLETADA'
                    )
                ) x WHERE rn = 1";

            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    /// <summary>
    /// Query genérico para todos los sistemas de Capa BD.
    /// Todas las tablas BD tienen: BASE_DE_DATOS, USERNAME, ACCOUNT_STATUS
    /// El cruce es por USERNAME = login del consolidado, con cedula via DA.
    /// </summary>
    public string GetQueryBD(string tabla) => $@"
        SELECT
            '{tabla}' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.USERNAME AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.ACCOUNT_STATUS AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.USERNAME = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.ACCOUNT_STATUS IN ('OPEN','EXPIRED','EXPIRED(GRACE)') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
        UNION ALL
        SELECT
            '{tabla}' AS App, 'CEDULA' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.USERNAME AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.ACCOUNT_STATUS AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.USERNAME = c.cedula COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.ACCOUNT_STATUS IN ('OPEN','EXPIRED','EXPIRED(GRACE)') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
          AND (s.USERNAME <> c.login OR s.USERNAME IS NULL OR c.login IS NULL)";

    #endregion
}
