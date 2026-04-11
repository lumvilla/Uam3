using BaseDatoSqLite.Conexion;
using Dapper;
using Data.Interfaces.CapaApp.IServicesCrucesExcel;
using Helper;
using Microsoft.Extensions.Configuration;
using Shared.CapaAplicacion.UserDisable;

namespace Data.Repository.CapaAPP.ServicesCrucesExcel;

public class CrucesUserDisable : ICrucesExcel
{
    private readonly ConnectionFactory _factory;
    private readonly ILogger _logger;
    private readonly IConfiguration _config;

    public CrucesUserDisable(ConnectionFactory factory, ILogger logger, IConfiguration config)
    {
        _factory = factory;
        _logger = logger;
        _config = config;
    }

    #region Métodos Públicos para Obtener Usuarios

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosPortalDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryPortal()} )
                SELECT *
                FROM datos d
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_app_temp t
                    WHERE t.app = 'portal' COLLATE NOCASE
                      AND t.cedula_consolidado = d.CedulaConsolidado
                      AND COALESCE(t.login_app, '') = COALESCE(d.LoginApp, '')
                      AND COALESCE(t.tipo_cruce, '') = COALESCE(d.TipoCruce, '')
                      AND DATE(t.fecha_ejecucion) = DATE('now')
                )
                AND NOT EXISTS (
                    SELECT 1 FROM retiro_app r
                    WHERE r.app = 'portal' COLLATE NOCASE
                    AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                    AND r.estado_oc = 'COMPLETADA'
                )
                ORDER BY d.FechaRetiro DESC;";
            var lista = (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
            return lista.GroupBy(x => new { x.CedulaConsolidado, x.LoginApp, x.TipoCruce })
                        .Select(g => g.OrderByDescending(x => x.FechaRetiro).First()).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosDADisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryDA()} )
                SELECT *
                FROM datos d
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_app_temp t
                    WHERE t.app = 'da' COLLATE NOCASE
                      AND t.cedula_consolidado = d.CedulaConsolidado
                      AND COALESCE(t.login_app, '') = COALESCE(d.LoginApp, '')
                      AND COALESCE(t.tipo_cruce, '') = COALESCE(d.TipoCruce, '')
                      AND DATE(t.fecha_ejecucion) = DATE('now')
                )
                AND NOT EXISTS (
                    SELECT 1 FROM retiro_app r
                    WHERE r.app = 'da' COLLATE NOCASE
                    AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                    AND r.estado_oc = 'COMPLETADA'
                )
                ORDER BY d.FechaRetiro DESC;";
            var lista = (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
            return lista.GroupBy(x => new { x.CedulaConsolidado, x.CedulaApp, x.TipoCruce })
                        .Select(g => g.OrderByDescending(x => x.FechaRetiro).First()).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosSiebelMovilDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQuerySiebelMovil()} )
                SELECT *
                FROM datos d
                WHERE NOT EXISTS (
                    SELECT 1 FROM retiro_app_temp t
                    WHERE t.app = 'siebelmovil' COLLATE NOCASE
                      AND t.cedula_consolidado = d.CedulaConsolidado
                      AND COALESCE(t.login_app, '') = COALESCE(d.LoginApp, '')
                      AND COALESCE(t.tipo_cruce, '') = COALESCE(d.TipoCruce, '')
                      AND DATE(t.fecha_ejecucion) = DATE('now')
                )
                AND NOT EXISTS (
                    SELECT 1 FROM retiro_app r
                    WHERE r.app = 'siebelmovil' COLLATE NOCASE
                    AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                    AND r.estado_oc = 'COMPLETADA'
                )
                ORDER BY d.FechaRetiro DESC;";
            var lista = (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
            return lista.GroupBy(x => new { x.CedulaConsolidado, x.LoginApp, x.TipoCruce })
                        .Select(g => g.OrderByDescending(x => x.FechaRetiro).First()).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosSiebelFijoDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQuerySiebelFijo()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'siebelfijo' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'siebelfijo' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosFenixDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryFenix()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'fenix' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'fenix' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosOpenDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryOpen()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'open' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'open' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosDwhMovilDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryDwhMovil()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'dwhmovil' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'dwhmovil' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosDwhFijoDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryDwhFijo()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'dwhfijo' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'dwhfijo' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosSapErpDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQuerySapErp()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'saperp' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'saperp' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosSapGrcDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQuerySapGrc()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'sapgrc' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'sapgrc' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosIamDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryIam()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'iam' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND t.login_consolidado = d.LoginConsolidado
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'iam' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosCbsDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryCbs()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'cbs' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND COALESCE(t.login_app,'') = COALESCE(d.LoginApp,'')
                       AND COALESCE(t.tipo_cruce,'') = COALESCE(d.TipoCruce,'')
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'cbs' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosCmDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryCm()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'cm' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND COALESCE(t.login_app,'') = COALESCE(d.LoginApp,'')
                       AND COALESCE(t.tipo_cruce,'') = COALESCE(d.TipoCruce,'')
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'cm' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosElkDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryElk()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'elk' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND COALESCE(t.login_app,'') = COALESCE(d.LoginApp,'')
                       AND COALESCE(t.tipo_cruce,'') = COALESCE(d.TipoCruce,'')
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'elk' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosPcrfDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryPcrf()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'pcrf' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND COALESCE(t.login_app,'') = COALESCE(d.LoginApp,'')
                       AND COALESCE(t.tipo_cruce,'') = COALESCE(d.TipoCruce,'')
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'pcrf' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    public async Task<IEnumerable<UsuarioDisableDto>> UsuariosBigDataDisable()
    {
        try
        {
            using var conexion = _factory.CreateConnection();
            var sql = $@"
                WITH datos AS ( {GetQueryBigData()} )
                SELECT *
                FROM (
                   SELECT d.*, ROW_NUMBER() OVER (PARTITION BY LoginConsolidado, LoginApp ORDER BY FechaRetiro DESC) AS rn
                   FROM datos d
                   WHERE NOT EXISTS (
                       SELECT 1 FROM retiro_app_temp t
                       WHERE t.app = 'bigdata' COLLATE NOCASE
                       AND t.cedula_consolidado = d.CedulaConsolidado
                       AND COALESCE(t.login_app,'') = COALESCE(d.LoginApp,'')
                       AND COALESCE(t.tipo_cruce,'') = COALESCE(d.TipoCruce,'')
                       AND DATE(t.fecha_ejecucion) = DATE('now')
                   )
                   AND NOT EXISTS (
                       SELECT 1 FROM retiro_app r
                       WHERE r.app = 'bigdata' COLLATE NOCASE
                       AND r.login_consolidado = d.LoginConsolidado COLLATE NOCASE
                       AND r.estado_oc = 'COMPLETADA'
                   )
                ) x WHERE rn = 1";
            return (await conexion.QueryAsync<UsuarioDisableDto>(sql)).ToList();
        }
        catch (Exception) { throw; }
    }

    #endregion

    #region Queries (usadas por CrucesDisplayService)

    public string GetQueryPortal() => @"
        SELECT DISTINCT
            'portal' AS App, 'LOGIN' AS TipoCruce,
            p.cedula AS CedulaApp, c.cedula AS CedulaConsolidado,
            p.Login AS LoginApp, c.cedula AS LoginConsolidado,
            c.nombre AS NombreConsolidado, 'Habilitado' AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN crm_portal p ON c.cedula = p.Login
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT DISTINCT
            'portal' AS App, 'CEDULA' AS TipoCruce,
            p.cedula AS CedulaApp, c.cedula AS CedulaConsolidado,
            p.Login AS LoginApp, c.cedula AS LoginConsolidado,
            c.nombre AS NombreConsolidado, 'Habilitado' AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN crm_portal p ON c.cedula = p.Cedula
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    public string GetQueryDA() => @"
        SELECT
            'da' AS App, 'LOGIN' AS TipoCruce,
            da.Identificacion AS CedulaApp, c.cedula AS CedulaConsolidado,
            da.Login AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, da.Estado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN directorio_activo da ON c.login = da.login
        WHERE da.estado = 'ACTIVO' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT
            'da' AS App, 'CEDULA' AS TipoCruce,
            da.Identificacion AS CedulaApp, c.cedula AS CedulaConsolidado,
            da.Login AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, da.Estado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN directorio_activo da ON c.cedula = da.Identificacion
        WHERE da.estado = 'ACTIVO' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (da.login <> c.login OR da.login IS NULL OR c.login IS NULL)";

    public string GetQuerySiebelMovil() => @"
        SELECT * FROM (
            SELECT DISTINCT
                CASE WHEN s.responsabilidad IN ('UNE Siebel Mobile General','UNE Siebel Mobile Supervisor','UNE Siebel Mobile Manager') COLLATE NOCASE THEN 'siebelMovil' ELSE 'siebelFijo' END AS App,
                'LOGIN' AS TipoCruce,
                c.cedula AS CedulaApp, c.cedula AS CedulaConsolidado,
                s.Login AS LoginApp, c.Login AS LoginConsolidado,
                c.nombre AS NombreConsolidado, s.responsabilidad AS EstadoApp,
                c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
            FROM consolidado c
            INNER JOIN siebel s ON s.login = c.login COLLATE NOCASE
            WHERE s.responsabilidad <> 'Z_UNE_DESHABILITADO'
              AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        ) sub WHERE sub.App = 'siebelMovil'
        UNION ALL
        SELECT * FROM (
            SELECT DISTINCT
                CASE WHEN s.responsabilidad IN ('UNE Siebel Mobile General','UNE Siebel Mobile Supervisor','UNE Siebel Mobile Manager') COLLATE NOCASE THEN 'siebelMovil' ELSE 'siebelFijo' END AS App,
                'CEDULA' AS TipoCruce,
                c.cedula AS CedulaApp, c.cedula AS CedulaConsolidado,
                s.Login AS LoginApp, c.Login AS LoginConsolidado,
                c.nombre AS NombreConsolidado, s.responsabilidad AS EstadoApp,
                c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
            FROM consolidado c
            INNER JOIN siebel s ON s.login = c.cedula COLLATE NOCASE
            WHERE s.responsabilidad <> 'Z_UNE_DESHABILITADO'
              AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
              AND (s.login <> c.login OR s.login IS NULL OR c.login IS NULL)
        ) sub WHERE sub.App = 'siebelMovil'";

    public string GetQuerySiebelFijo() => @"
        WITH ClasificacionLogin AS (
            SELECT s.Login,
                SUM(CASE WHEN s.responsabilidad NOT IN ('UNE Siebel Mobile General','UNE Siebel Mobile Supervisor','UNE Siebel Mobile Manager') COLLATE NOCASE THEN 1 ELSE 0 END) AS ResponsabilidadesNoMobile
            FROM siebel s
            WHERE s.responsabilidad <> 'Z_UNE_DESHABILITADO'
            GROUP BY s.Login
        )
        SELECT
            'siebelFijo' AS App, 'LOGIN' AS TipoCruce,
            c.cedula AS CedulaApp, c.cedula AS CedulaConsolidado,
            c.Login AS LoginConsolidado, cl.Login AS LoginApp,
            c.nombre AS NombreConsolidado, MIN(s.responsabilidad) AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN siebel s ON s.login = c.login COLLATE NOCASE
        INNER JOIN ClasificacionLogin cl ON s.login = cl.Login
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND cl.ResponsabilidadesNoMobile > 0
        GROUP BY c.cedula, c.login, c.nombre, c.estado_entidad, c.fecha_retiro, cl.Login";

    public string GetQueryFenix() => @"
        SELECT
            'fenix' AS App, 'LOGIN' AS TipoCruce,
            f.REGISTRO AS CedulaApp, c.cedula AS CedulaConsolidado,
            f.USUARIO_ID AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, f.ESTADO_USUARIO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN fenix f ON f.USUARIO_ID = c.login COLLATE NOCASE
        WHERE f.ESTADO_USUARIO <> 'RETIRADO' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT
            'fenix' AS App, 'CEDULA' AS TipoCruce,
            f.REGISTRO AS CedulaApp, c.cedula AS CedulaConsolidado,
            f.USUARIO_ID AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, f.ESTADO_USUARIO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN fenix f ON f.USUARIO_ID = c.cedula COLLATE NOCASE
        WHERE f.ESTADO_USUARIO <> 'RETIRADO' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (f.REGISTRO <> c.login OR f.USUARIO_ID IS NULL OR c.login IS NULL)";

    public string GetQueryOpen() => @"
        SELECT DISTINCT
            'open_une' AS App, 'LOGIN' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            op.LOGIN AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, op.ESTADO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN open_une op ON op.LOGIN = c.login COLLATE NOCASE
        WHERE op.ESTADO IN ('OPEN','EXPIRED') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT DISTINCT
            'open_une' AS App, 'CEDULA' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            op.LOGIN AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, op.ESTADO AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN open_une op ON 'No Tiene cc el Ins.' = c.cedula COLLATE NOCASE
        WHERE op.ESTADO IN ('OPEN','EXPIRED') COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (op.LOGIN <> c.login OR op.LOGIN IS NULL OR c.login IS NULL)";

    public string GetQueryDwhMovil() => @"
        SELECT
            'dwhMovil' AS App, 'LOGIN' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            dm.Usuario AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, dm.Habilitado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN dwh_movil dm ON dm.Usuario = c.login COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT
            'dwhMovil' AS App, 'CEDULA' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            dm.Usuario AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, dm.Habilitado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN dwh_movil dm ON 'No Tiene cc el Ins.' = c.cedula COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (dm.Usuario <> c.login OR dm.Usuario IS NULL OR c.login IS NULL)";

    public string GetQueryDwhFijo() => @"
        SELECT
            'dwhFijo' AS App, 'LOGIN' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            df.Usuario AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, df.Habilitado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN dwh_fijo df ON df.Usuario = c.login COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND df.Habilitado = 'Habilitado' COLLATE NOCASE
        UNION ALL
        SELECT
            'dwhFijo' AS App, 'CEDULA' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            df.Usuario AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, df.Habilitado AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN dwh_fijo df ON 'No Tiene cc el Ins.' = c.cedula COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND df.Habilitado = 'Habilitado' COLLATE NOCASE
          AND (df.Usuario <> c.login OR df.Usuario IS NULL OR c.login IS NULL)";

    public string GetQuerySapErp() => @"
        SELECT
            'sapErp' AS App, 'LOGIN' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            se.Usuarios AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, se.Tipo_usuario AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN sap_erp se ON se.Usuarios = c.login COLLATE NOCASE
        WHERE se.Tipo_usuario COLLATE NOCASE <> 'L Referen.'
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT
            'sapErp' AS App, 'CEDULA' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            se.Usuarios AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, se.Tipo_usuario AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN sap_erp se ON 'No Tiene cc el Ins.' = c.cedula COLLATE NOCASE
        WHERE se.Tipo_usuario COLLATE NOCASE <> 'L Referen.'
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (se.Usuarios <> c.login OR se.Usuarios IS NULL OR c.login IS NULL)";

    public string GetQuerySapGrc() => @"
        SELECT
            'sapGrc' AS App, 'LOGIN' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            sg.Usuarios AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, sg.Tipo_usuario AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN sap_grc sg ON sg.Usuarios = c.login COLLATE NOCASE
        WHERE sg.Tipo_usuario <> 'L Referen.' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
        UNION ALL
        SELECT
            'sapGrc' AS App, 'CEDULA' AS TipoCruce,
            'No Tiene cc el Ins.' AS CedulaApp, c.cedula AS CedulaConsolidado,
            sg.Usuarios AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, sg.Tipo_usuario AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN sap_grc sg ON 'No Tiene cc el Ins.' = c.cedula COLLATE NOCASE
        WHERE sg.Tipo_usuario <> 'L Referen.' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND (sg.Usuarios <> c.login OR sg.Usuarios IS NULL OR c.login IS NULL)";

    public string GetQueryIam() => @"
        SELECT
            'iam' AS App, 'LOGIN' AS TipoCruce,
            i.Numero_de_identificacion AS CedulaApp, c.cedula AS CedulaConsolidado,
            i.Login_ID AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, i.Estado_de_la_identidad AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN iam i ON i.Login_ID = c.login COLLATE NOCASE
        WHERE i.Estado_de_la_Identidad = 'Habilitado' COLLATE NOCASE
          AND i.ESTADO = '1' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
        UNION ALL
        SELECT
            'iam' AS App, 'CEDULA' AS TipoCruce,
            i.Numero_de_identificacion AS CedulaApp, c.cedula AS CedulaConsolidado,
            i.Login_ID AS LoginApp, c.Login AS LoginConsolidado,
            c.nombre AS NombreConsolidado, i.Estado_de_la_identidad AS EstadoApp,
            c.estado_entidad AS EstadoConsolidado, c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN iam i ON i.Numero_de_identificacion = c.cedula COLLATE NOCASE
        WHERE i.Estado_de_la_Identidad = 'Habilitado' COLLATE NOCASE
          AND i.ESTADO = '1' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE
          AND c.estado_entidad IS NOT NULL
          AND (i.Login_ID <> c.login OR i.Login_ID IS NULL OR c.login IS NULL)";

    public string GetQueryCbs() => @"
        SELECT
            'cbs' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado, s.STAFF_NO AS LoginApp,
            c.Login AS LoginConsolidado, c.nombre AS NombreConsolidado,
            c.estado_entidad AS EstadoApp, c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN cbs s ON CAST(s.STAFF_NO AS TEXT) = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE CAST(s.STATUS AS TEXT) = '1'
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    public string GetQueryCm() => @"
        SELECT
            'cm' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado, m.USERNAME AS LoginApp,
            c.Login AS LoginConsolidado, c.nombre AS NombreConsolidado,
            c.estado_entidad AS EstadoApp, c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN cm m ON m.USERNAME = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE m.STATUS = 'HABILITADO' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    public string GetQueryElk() => @"
        SELECT
            'elk' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado, e.LOGIN AS LoginApp,
            c.Login AS LoginConsolidado, c.nombre AS NombreConsolidado,
            c.estado_entidad AS EstadoApp, c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN elk e ON e.LOGIN = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE e.ESTADO = 'Habilitado' COLLATE NOCASE
          AND c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    public string GetQueryPcrf() => @"
        SELECT
            'pcrf' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado, p.USUARIOS AS LoginApp,
            c.Login AS LoginConsolidado, c.nombre AS NombreConsolidado,
            c.estado_entidad AS EstadoApp, c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN pcrf p ON p.USUARIOS = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    public string GetQueryBigData() => @"
        SELECT
            'bigdata' AS App, 'LOGIN' AS TipoCruce,
            COALESCE(da.identificacion, 'Sin cedula') AS CedulaApp,
            c.cedula AS CedulaConsolidado, b.LOGIN AS LoginApp,
            c.Login AS LoginConsolidado, c.nombre AS NombreConsolidado,
            c.estado_entidad AS EstadoApp, c.estado_entidad AS EstadoConsolidado,
            c.fecha_retiro AS FechaRetiro
        FROM consolidado c
        INNER JOIN big_data b ON b.LOGIN = c.login COLLATE NOCASE
        LEFT JOIN directorio_activo da ON da.login = c.login COLLATE NOCASE
        WHERE c.estado_entidad NOT IN ('Habilitado', 'Habilitado.') COLLATE NOCASE";

    #endregion
}
