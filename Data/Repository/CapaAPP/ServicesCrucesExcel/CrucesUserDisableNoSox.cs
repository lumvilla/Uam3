using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesUserDisableNoSox : ICrucesExcelNoSox
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public CrucesUserDisableNoSox(ConnectionFactory factory, ILogger logger, IConfiguration config)
    {
        _factory = factory;
        _logger  = logger;
        _config  = config;
    }

    #region Métodos Públicos

    // Grupo A
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosGtcDisable()          => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_gtc"),          "nosox_gtc");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenEdatelDisable()    => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_open_edatel"),   "nosox_open_edatel");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenEtpDisable()       => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_open_etp"),      "nosox_open_etp");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosOsmDisable()           => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_osm"),           "nosox_osm");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosMssDisable()           => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_mss"),           "nosox_mss");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosTigoGestionDisable()   => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_tigo_gestion"),  "nosox_tigo_gestion");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosViafirmaDisable()      => await EjecutarQuery(GetQueryNoSoxGrupoA("nosox_viafirma"),      "nosox_viafirma");

    // Grupo B
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebog1Disable()      => await EjecutarQuery(GetQueryNoSoxGrupoB("nosox_maebog1"),  "nosox_maebog1");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebog2Disable()      => await EjecutarQuery(GetQueryNoSoxGrupoB("nosox_maebog2"),  "nosox_maebog2");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosMaebaqDisable()       => await EjecutarQuery(GetQueryNoSoxGrupoB("nosox_maebaq"),   "nosox_maebaq");
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosMaecoreDisable()      => await EjecutarQuery(GetQueryNoSoxGrupoB("nosox_maecore"),  "nosox_maecore");

    // Grupo C
    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosServiceDeskDisable()  => await EjecutarQuery(GetQueryNoSoxGrupoC("nosox_service_desk"), "nosox_service_desk");

    #endregion

    #region Queries

    /// <summary>
    /// Grupo A: LOGIN + ESTADO IN ('OPEN','EXPIRED','EXPIRED(GRACE)')
    /// GTC, OPEN EDATEL, OPEN ETP, OSM, MSS, TIGO GESTION, VIAFIRMA
    /// </summary>
    public string GetQueryNoSoxGrupoA(string tabla) => $@"
        SELECT
            '{tabla}' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.LOGIN AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.ESTADO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.LOGIN = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.ESTADO IN ('OPEN','EXPIRED','EXPIRED(GRACE)') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
        UNION ALL
        SELECT
            '{tabla}' AS App, 'CEDULA' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.LOGIN AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.ESTADO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.LOGIN = c.cedula COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.ESTADO IN ('OPEN','EXPIRED','EXPIRED(GRACE)') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
          AND (s.LOGIN <> c.login OR s.LOGIN IS NULL OR c.login IS NULL)";

    /// <summary>
    /// Grupo B: USER_NAME + DISABLE_ACCOUNT = 'Enabled'
    /// MAEBOG1, MAEBOG2, MAEBAQ, MAECORE
    /// </summary>
    public string GetQueryNoSoxGrupoB(string tabla) => $@"
        SELECT
            '{tabla}' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.USER_NAME AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.DISABLE_ACCOUNT AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.USER_NAME = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.DISABLE_ACCOUNT = 'Enabled' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
        UNION ALL
        SELECT
            '{tabla}' AS App, 'CEDULA' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.USER_NAME AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            s.DISABLE_ACCOUNT AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.USER_NAME = c.cedula COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE s.DISABLE_ACCOUNT = 'Enabled' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
          AND (s.USER_NAME <> c.login OR s.USER_NAME IS NULL OR c.login IS NULL)";

    /// <summary>
    /// Grupo C: LOGIN + ESTADO = 0
    /// SERVICE DESK
    /// </summary>
    public string GetQueryNoSoxGrupoC(string tabla) => $@"
        SELECT
            '{tabla}' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.LOGIN AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            CAST(s.ESTADO AS TEXT) AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.LOGIN = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE CAST(s.ESTADO AS TEXT) = '0'
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
        UNION ALL
        SELECT
            '{tabla}' AS App, 'CEDULA' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado,
            s.LOGIN AS LoginApp, c.login AS LoginConsolidado,
            c.nombre AS NombreConsolidado,
            CAST(s.ESTADO AS TEXT) AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN {tabla} s ON s.LOGIN = c.cedula COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE CAST(s.ESTADO AS TEXT) = '0'
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
          AND (s.LOGIN <> c.login OR s.LOGIN IS NULL OR c.login IS NULL)";

    #endregion

    #region Ejecución

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
                        SELECT 1 FROM retiro_nosox_temp t
                        WHERE t.app = '{app}' COLLATE NOCASE
                          AND t.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                          AND DATE(t.fecha_ejecucion) = DATE('now')
                    )
                    AND NOT EXISTS (
                        SELECT 1 FROM retiro_nosox r
                        WHERE r.app = '{app}' COLLATE NOCASE
                          AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                          AND r.estado_oc = 'COMPLETADA'
                    )
                ) x WHERE rn = 1";

            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    #endregion
}
